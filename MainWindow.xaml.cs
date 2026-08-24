using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Management;
using System.Windows.Input;
using System.IO;

namespace DNSChanger
{
    /// <summary>
    /// Main window class for the DNS Changer application.
    /// Provides functionality to manage DNS settings for network adapters.
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DnsEntry> _dnsEntries = new();
        private const string AutomaticDnsTitle = "Automatic";
        private const int ProcessTimeoutSeconds = 30;

        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// Sets up the UI components and loads initial data.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadDnsEntries();
            DisplayCurrentDns(); // Display current DNS on startup
            LoadNetworkAdapters();
        }

        /// <summary>
        /// Handles the selection change event for the network adapter ComboBox.
        /// Updates the current DNS display when a different adapter is selected.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data containing information about the selection change.</param>
        private void NetworkAdapterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DisplayCurrentDns();
        }

        /// <summary>
        /// Retrieves all active (connected) network adapters from the system and populates the NetworkAdapterComboBox.
        /// Only adapters with a "Connected" status (NetConnectionStatus = 2) are displayed.
        /// Uses WMI queries to enumerate network adapters.
        /// </summary>
        private void LoadNetworkAdapters(string? preferredAdapter = null)
        {
            NetworkAdapterComboBox.Items.Clear();
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    var name = adapter["NetConnectionID"]?.ToString();
                    if (!string.IsNullOrEmpty(name))
                        NetworkAdapterComboBox.Items.Add(name);
                }
            }

            int preferredIndex = preferredAdapter != null ? NetworkAdapterComboBox.Items.IndexOf(preferredAdapter) : -1;
            if (preferredIndex >= 0)
                NetworkAdapterComboBox.SelectedIndex = preferredIndex;
            else if (NetworkAdapterComboBox.Items.Count > 0)
                NetworkAdapterComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Handles the click event for the refresh adapters button.
        /// Reloads the adapter list while preserving the current selection when possible.
        /// </summary>
        private void RefreshAdaptersButton_Click(object sender, RoutedEventArgs e)
        {
            LoadNetworkAdapters(NetworkAdapterComboBox.SelectedItem as string);
        }

        /// <summary>
        /// Enables dragging the borderless window via its custom title bar.
        /// </summary>
        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DragMove();
            }
            catch (InvalidOperationException)
            {
            }
        }

        /// <summary>
        /// Handles the click event for the custom close button.
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Loads DNS entries from the JSON file using FileHandler and populates the DNS ComboBox.
        /// Automatically adds an 'Automatic' option that represents DHCP DNS settings.
        /// The 'Automatic' option cannot be deleted by users.
        /// </summary>
        private void LoadDnsEntries()
        {
            _dnsEntries = FileHandler.LoadDnsEntries();
            DnsComboBox.Items.Clear(); // Clear existing items to prevent duplicates

            // Add the Automatic option for DHCP DNS settings
            DnsComboBox.Items.Add(new DnsEntry { Title = AutomaticDnsTitle, PrimaryDns = "DHCP", SecondaryDns = "Auto" });

            // Add all custom DNS entries from the configuration file
            foreach (var entry in _dnsEntries)
            {
                DnsComboBox.Items.Add(entry);
            }
        }

        /// <summary>
        /// Displays the currently configured DNS servers for the selected network adapter.
        /// Updates the CurrentDns label with the current DNS configuration.
        /// Shows "Unknown" if no adapter is selected.
        /// </summary>
        private void DisplayCurrentDns()
        {
            string adapterName = NetworkAdapterComboBox.SelectedItem as string ?? string.Empty;
            if (string.IsNullOrEmpty(adapterName))
            {
                AdapterNameText.Text = "No adapter selected";
                CurrentDns.Text = "Select a network adapter to view its DNS servers.";
                return;
            }

            AdapterNameText.Text = adapterName;
            CurrentDns.Text = GetCurrentDns(adapterName);
        }

        /// <summary>
        /// Retrieves the currently assigned DNS servers for the specified network adapter.
        /// Uses WMI queries to get DNS configuration from Win32_NetworkAdapterConfiguration.
        /// </summary>
        /// <param name="adapterName">The name of the network adapter to query.</param>
        /// <returns>A string representation of the current DNS servers, or an error message if retrieval fails.</returns>
        private string GetCurrentDns(string adapterName)
        {
            try
            {
                int adapterIndex = -1;
                string adapterQuery = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2";

                // First, find the adapter index by matching the connection ID
                using (ManagementObjectSearcher adapterSearcher = new ManagementObjectSearcher(adapterQuery))
                {
                    foreach (ManagementObject adapter in adapterSearcher.Get())
                    {
                        if (adapter["NetConnectionID"]?.ToString() == adapterName)
                        {
                            adapterIndex = Convert.ToInt32(adapter["Index"]);
                            break;
                        }
                    }
                }

                if (adapterIndex == -1)
                    return "Adapter not found";

                // Query the adapter configuration using the found index
                string configQuery = $"SELECT * FROM Win32_NetworkAdapterConfiguration WHERE Index = {adapterIndex} AND IPEnabled = True";

                using (ManagementObjectSearcher configSearcher = new ManagementObjectSearcher(configQuery))
                {
                    foreach (ManagementObject config in configSearcher.Get())
                    {
                        string[] dnses = config["DNSServerSearchOrder"] as string[] ?? Array.Empty<string>();
                        if (dnses.Length > 0)
                            return string.Join(", ", dnses);
                        else
                            return "Automatic (DHCP)";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }

            return "No DNS configuration found";
        }

        /// <summary>
        /// Handles the click event for the Set DNS button.
        /// Applies the selected DNS configuration to the chosen network adapter.
        /// If 'Automatic' is selected, resets DNS settings to DHCP.
        /// Uses asynchronous execution with timeout to prevent UI freezing during DNS changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private async void SetDnsButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate selections
            if (DnsComboBox.SelectedItem is not DnsEntry selectedDns)
            {
                MessageBox.Show("Please select a DNS entry.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string networkAdapter = NetworkAdapterComboBox.SelectedItem as string ?? string.Empty;
            if (string.IsNullOrEmpty(networkAdapter))
            {
                MessageBox.Show("Please select a network adapter.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Show loading cursor and disable UI during operation
            Mouse.OverrideCursor = Cursors.Wait;
            this.IsEnabled = false;

            try
            {
                bool dnsSet;

                // Execute DNS change operation asynchronously with timeout
                if (selectedDns.Title == AutomaticDnsTitle)
                {
                    dnsSet = await ExecuteDnsChangeAsync(() => ResetDns(networkAdapter));
                }
                else
                {
                    dnsSet = await ExecuteDnsChangeAsync(() => SetDns(networkAdapter, selectedDns.PrimaryDns, selectedDns.SecondaryDns));
                }

                // Show result message and update display
                if (dnsSet)
                {
                    MessageBox.Show($"DNS for '{networkAdapter}' changed to '{selectedDns.Title}'.",
                                    "Successful Change", MessageBoxButton.OK, MessageBoxImage.Information);
                    DisplayCurrentDns();
                }
                else
                {
                    MessageBox.Show("DNS change failed. Please make sure you run the application as Administrator.",
                                   "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (TimeoutException)
            {
                MessageBox.Show($"DNS change operation timed out after {ProcessTimeoutSeconds} seconds. Please try again.",
                               "Timeout", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}",
                               "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                // Restore UI state
                this.IsEnabled = true;
                Mouse.OverrideCursor = null;
            }
        }

        /// <summary>
        /// Executes a DNS change operation asynchronously with a timeout.
        /// </summary>
        /// <param name="operation">The DNS change operation to execute.</param>
        /// <returns>True if the operation succeeded within the timeout, false otherwise.</returns>
        private async Task<bool> ExecuteDnsChangeAsync(Func<bool> operation)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(ProcessTimeoutSeconds));

            try
            {
                return await Task.Run(() => operation(), cts.Token);
            }
            catch (OperationCanceledException)
            {
                throw new TimeoutException();
            }
        }

        /// <summary>
        /// Handles the click event for the Delete DNS button.
        /// Removes the selected DNS entry from the configuration.
        /// Prevents deletion of the 'Automatic' option to maintain system functionality.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private void DeleteDnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DnsComboBox.SelectedItem is DnsEntry selectedDns)
            {
                // Prevent deletion of the automatic option
                if (selectedDns.Title == AutomaticDnsTitle)
                {
                    MessageBox.Show("The 'Automatic' option cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Confirm deletion with user
                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{selectedDns.Title}'?",
                                                        "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dnsEntries.Remove(selectedDns);
                    FileHandler.SaveDnsEntries(_dnsEntries);
                    LoadDnsEntries(); // Refresh UI after deletion
                }
            }
        }

        /// <summary>
        /// Handles the click event for the Edit DNS button.
        /// Opens the Edit DNS window dialog for updating the selected DNS entry.
        /// If the entry is successfully updated, updates the list, saves to file, and refreshes the UI.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private void EditDnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DnsComboBox.SelectedItem is not DnsEntry selectedDns)
            {
                MessageBox.Show("Please select a DNS entry to edit.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Prevent editing the automatic option
            if (selectedDns.Title == AutomaticDnsTitle)
            {
                MessageBox.Show("The 'Automatic' option cannot be edited.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editDnsWindow = new EditDnsWindow(selectedDns) { Owner = this };
            if (editDnsWindow.ShowDialog() == true)
            {
                int index = _dnsEntries.IndexOf(selectedDns);
                if (index != -1)
                {
                    _dnsEntries[index] = editDnsWindow.UpdatedDnsEntry;
                }
                else
                {
                    // Fallback in case of reference mismatch: match by properties
                    var existing = _dnsEntries.FirstOrDefault(d => d.Title == selectedDns.Title && d.PrimaryDns == selectedDns.PrimaryDns && d.SecondaryDns == selectedDns.SecondaryDns);
                    if (existing != null)
                    {
                        existing.Title = editDnsWindow.UpdatedDnsEntry.Title;
                        existing.PrimaryDns = editDnsWindow.UpdatedDnsEntry.PrimaryDns;
                        existing.SecondaryDns = editDnsWindow.UpdatedDnsEntry.SecondaryDns;
                    }
                }

                FileHandler.SaveDnsEntries(_dnsEntries);
                LoadDnsEntries(); // Refresh UI with updated DNS entries

                // Re-select the updated entry in the ComboBox
                foreach (var item in DnsComboBox.Items)
                {
                    if (item is DnsEntry entry && entry.Title == editDnsWindow.UpdatedDnsEntry.Title &&
                        entry.PrimaryDns == editDnsWindow.UpdatedDnsEntry.PrimaryDns &&
                        entry.SecondaryDns == editDnsWindow.UpdatedDnsEntry.SecondaryDns)
                    {
                        DnsComboBox.SelectedItem = entry;
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the click event for the Add DNS button.
        /// Opens the Add DNS window dialog for creating new DNS entries.
        /// If a new entry is successfully created, updates the dropdown list and saves to file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private void AddDnsButton_Click(object sender, RoutedEventArgs e)
        {
            var addDnsWindow = new AddDnsWindow { Owner = this };
            if (addDnsWindow.ShowDialog() == true)
            {
                _dnsEntries.Add(addDnsWindow.NewDnsEntry);
                FileHandler.SaveDnsEntries(_dnsEntries);
                LoadDnsEntries(); // Refresh UI with new DNS entry
            }
        }

        /// <summary>
        /// Retrieves the name of the first active network adapter found in the system.
        /// Uses WMI to query for connected network adapters.
        /// </summary>
        /// <returns>The name of the active network adapter, or an empty string if none found.</returns>
        public string GetActiveNetworkAdapter()
        {
            string query = "SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionStatus = 2";
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject adapter in searcher.Get())
                {
                    return adapter["NetConnectionID"]?.ToString() ?? string.Empty;
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the primary and secondary DNS servers for the specified network adapter.
        /// Uses a temporary batch file to combine both commands, reducing UAC prompts.
        /// Requires administrator rights to execute successfully.
        /// </summary>
        /// <param name="adapterName">The name of the network adapter to configure.</param>
        /// <param name="primaryDns">The primary DNS server IP address.</param>
        /// <param name="secondaryDns">The secondary DNS server IP address.</param>
        /// <returns>True if the DNS configuration was successful, false otherwise.</returns>
        public bool SetDns(string adapterName, string primaryDns, string secondaryDns)
        {
            string batchFile = string.Empty;

            try
            {
                // Create a temporary batch file to combine both DNS commands
                // This reduces UAC prompts from 2 to 1
                batchFile = Path.Combine(Path.GetTempPath(), $"setdns_{Guid.NewGuid()}.bat");

                string batchContent = $@"@echo off
netsh interface ip set dns name=""{adapterName}"" static {primaryDns}
netsh interface ip add dns name=""{adapterName}"" {secondaryDns} index=2
exit /b 0";

                File.WriteAllText(batchFile, batchContent);

                // Execute the batch file with administrator privileges
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = batchFile,
                    Verb = "runas", // Request administrator privileges
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                if (process == null)
                    return false;

                // Wait for process to complete with timeout
                bool exited = process.WaitForExit(ProcessTimeoutSeconds * 1000);

                if (!exited)
                {
                    // If timeout occurs, kill the process
                    try { process.Kill(); } catch { }
                    return false;
                }

                // Check exit code (0 means success)
                return process.ExitCode == 0;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // User cancelled UAC prompt
                return false;
            }
            catch
            {
                return false;
            }
            finally
            {
                // Clean up the temporary batch file
                if (batchFile != null)
                {
                    try
                    {
                        // Small delay to ensure process has released the file
                        System.Threading.Thread.Sleep(100);
                        if (File.Exists(batchFile))
                            File.Delete(batchFile);
                    }
                    catch { /* Ignore cleanup errors */ }
                }
            }
        }

        /// <summary>
        /// Resets the DNS settings for the specified network adapter to automatic (DHCP).
        /// Uses the Windows netsh command-line utility with elevated privileges.
        /// Requires administrator rights to execute successfully.
        /// </summary>
        /// <param name="adapterName">The name of the network adapter to reset.</param>
        /// <returns>True if the DNS reset was successful, false otherwise.</returns>
        public bool ResetDns(string adapterName)
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ip set dns name=\"{adapterName}\" dhcp",
                    Verb = "runas", // Request administrator privileges
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                });

                if (process == null)
                    return false;

                // Wait for process to complete with timeout
                bool exited = process.WaitForExit(ProcessTimeoutSeconds * 1000);

                if (!exited)
                {
                    // If timeout occurs, kill the process
                    try { process.Kill(); } catch { }
                    return false;
                }

                // Check exit code (0 means success)
                return process.ExitCode == 0;
            }
            catch (System.ComponentModel.Win32Exception)
            {
                // User cancelled UAC prompt
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
