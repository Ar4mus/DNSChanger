using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Management;

namespace DNSChanger
{
    public partial class MainWindow : Window
    {
        private List<DnsEntry> _dnsEntries;
        private const string AutomaticDnsTitle = "Automatic";

        public MainWindow()
        {
            InitializeComponent();
            LoadDnsEntries();
            DisplayCurrentDns(); // Display current DNS on startup
        }

        /// <summary>
        /// Loads DNS entries from the JSON file and populates the ComboBox.
        /// Adds an 'Automatic' option that cannot be deleted.
        /// </summary>
        private void LoadDnsEntries()
        {
            _dnsEntries = FileHandler.LoadDnsEntries();
            DnsComboBox.Items.Clear(); // Clear existing items

            // Add the Automatic option
            DnsComboBox.Items.Add(new DnsEntry { Title = AutomaticDnsTitle, PrimaryDns = "DHCP", SecondaryDns = "Auto" });

            foreach (var entry in _dnsEntries)
            {
                DnsComboBox.Items.Add(entry);
            }
        }

        /// <summary>
        /// Displays the currently set DNS servers in the label.
        /// </summary>
        private void DisplayCurrentDns()
        {
            string currentDns = GetCurrentDns();
            CurrentDns.Content = $"Current DNS is: {currentDns}";
        }

        /// <summary>
        /// Retrieves the currently assigned DNS servers.
        /// </summary>
        /// <returns>A string representation of the current DNS servers.</returns>
        private string GetCurrentDns()
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/C nslookup google.com",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                Process process = new Process { StartInfo = psi };
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string[] lines = output.Split('\n');
                foreach (string line in lines)
                {
                    if (line.Trim().StartsWith("Address:") && !line.Contains("#"))
                    {
                        return line.Split(':')[1].Trim();
                    }
                }
            }
            catch
            {
                return "Unknown";
            }
            return "Unknown";
        }


        /// <summary>
        /// Handles the click event for setting the selected DNS.
        /// If 'Automatic' is selected, resets DNS settings.
        /// </summary>
        private void SetDnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DnsComboBox.SelectedItem is DnsEntry selectedDns)
            {
                string networkAdapter = GetActiveNetworkAdapter();
                if (string.IsNullOrEmpty(networkAdapter))
                {
                    MessageBox.Show("No active network adapter found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                bool dnsSet;
                if (selectedDns.Title == AutomaticDnsTitle)
                {
                    dnsSet = ResetDns(networkAdapter);
                }
                else
                {
                    dnsSet = SetDns(networkAdapter, selectedDns.PrimaryDns, selectedDns.SecondaryDns);
                }

                if (dnsSet)
                {
                    MessageBox.Show($"DNS Updated: {selectedDns.Title}",
                                    "DNS Set Successfully", MessageBoxButton.OK, MessageBoxImage.Information);
                    DisplayCurrentDns();
                }
                else
                {
                    MessageBox.Show("Failed to set DNS.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handles the click event for deleting the selected DNS entry.
        /// Prevents deletion of the 'Automatic' option.
        /// </summary>
        private void DeleteDnsButton_Click(object sender, RoutedEventArgs e)
        {
            if (DnsComboBox.SelectedItem is DnsEntry selectedDns)
            {
                if (selectedDns.Title == AutomaticDnsTitle)
                {
                    MessageBox.Show("The 'Automatic' option cannot be deleted.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                MessageBoxResult result = MessageBox.Show($"Are you sure you want to delete '{selectedDns.Title}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _dnsEntries.Remove(selectedDns);
                    FileHandler.SaveDnsEntries(_dnsEntries);
                    LoadDnsEntries(); // Refresh UI after deletion
                }
            }
        }

        /// <summary>
        /// Opens the window to add a new DNS entry.
        /// If a new entry is saved, updates the dropdown list.
        /// </summary>
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
        /// Retrieves the name of the active network adapter.
        /// </summary>
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
        /// Sets the primary and secondary DNS for the given network adapter using netsh.
        /// </summary>
        public bool SetDns(string adapterName, string primaryDns, string secondaryDns)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C netsh interface ip set dns name=\"{adapterName}\" static {primaryDns}",
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
                })?.WaitForExit();

                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C netsh interface ip add dns name=\"{adapterName}\" {secondaryDns} index=2",
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
                })?.WaitForExit();

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Resets the DNS settings to automatic.
        /// </summary>
        public bool ResetDns(string adapterName)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C netsh interface ip set dns name=\"{adapterName}\" dhcp",
                    Verb = "runas",
                    CreateNoWindow = true,
                    UseShellExecute = true
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
