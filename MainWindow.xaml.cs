using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Management;
using System.Windows.Input;

namespace DNSChanger
{
    /// <summary>
    /// Main window class for the DNS Changer application.
    /// Provides functionality to manage DNS settings for network adapters.
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<DnsEntry> _dnsEntries;
        private const string AutomaticDnsTitle = "Automatic";

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
        private void LoadNetworkAdapters()
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
            string adapterName = NetworkAdapterComboBox.SelectedItem as string;
            if (string.IsNullOrEmpty(adapterName))
            {
                CurrentDns.Text = "Current DNS: Unknown";
                return;
            }

            string currentDns = GetCurrentDns(adapterName);
            CurrentDns.Text = $"Current DNS for '{adapterName}' is: {currentDns}";
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
                        string[] dnses = config["DNSServerSearchOrder"] as string[];
                        if (dnses != null && dnses.Length > 0)
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
        /// Uses asynchronous execution to prevent UI freezing during DNS changes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private async void SetDnsButton_Click(object sender, RoutedEventArgs e)
        {
            // Show loading cursor and disable UI during operation
            Mouse.OverrideCursor = Cursors.Wait;
            this.IsEnabled = false;

            if (DnsComboBox.SelectedItem is DnsEntry selectedDns)
            {
                string networkAdapter = NetworkAdapterComboBox.SelectedItem as string;
                if (string.IsNullOrEmpty(networkAdapter))
                {
                    MessageBox.Show("Please select a network adapter.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    this.IsEnabled = true;
                    Mouse.OverrideCursor = null;
                    return;
                }

                bool dnsSet = false;

                // Execute DNS change operation asynchronously
                await Task.Run(() =>
                {
                    if (selectedDns.Title == AutomaticDnsTitle)
                    {
                        dnsSet = ResetDns(networkAdapter);
                    }
                    else
                    {
                        dnsSet = SetDns(networkAdapter, selectedDns.PrimaryDns, selectedDns.SecondaryDns);
                    }
                });

                // Restore UI state
                this.IsEnabled = true;
                Mouse.OverrideCursor = null;

                // Show result message and update display
                if (dnsSet)
                {
                    MessageBox.Show($"DNS for '{networkAdapter}' changed to '{selectedDns.Title}'.",
                                    "Successful Change", MessageBoxButton.OK, MessageBoxImage.Information);
                    DisplayCurrentDns();
                }
                else
                {
                    MessageBox.Show("DNS change failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
        /// Handles the click event for the Add DNS button.
        /// Opens the Add DNS window dialog for creating new DNS entries.
        /// If a new entry is successfully created, updates the dropdown list and saves to file.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">Event data for the button click.</param>
        private void AddDnsButton_Click(object sender, RoutedEventArgs e)
        {
            var addDnsWindow = new AddDnsWindow();
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
                    return adapter["NetConnectionID"]?.ToString();
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Sets the primary and secondary DNS servers for the specified network adapter.
        /// Uses the Windows netsh command-line utility with elevated privileges.
        /// Requires administrator rights to execute successfully.
        /// </summary>
        /// <param name="adapterName">The name of the network adapter to configure.</param>
        /// <param name="primaryDns">The primary DNS server IP address.</param>
        /// <param name="secondaryDns">The secondary DNS server IP address.</param>
        /// <returns>True if the DNS configuration was successful, false otherwise.</returns>
        public bool SetDns(string adapterName, string primaryDns, string secondaryDns)
        {
            try
            {
                // Set primary DNS server
                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ip set dns name=\"{adapterName}\" static {primaryDns}",
                    Verb = "runas", // Request administrator privileges
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                })?.WaitForExit();

                // Add secondary DNS server
                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ip add dns name=\"{adapterName}\" {secondaryDns} index=2",
                    Verb = "runas", // Request administrator privileges
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                })?.WaitForExit();

                return true;
            }
            catch
            {
                return false;
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
                Process.Start(new ProcessStartInfo
                {
                    FileName = "netsh.exe",
                    Arguments = $"interface ip set dns name=\"{adapterName}\" dhcp",
                    Verb = "runas", // Request administrator privileges
                    CreateNoWindow = true,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                })?.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
