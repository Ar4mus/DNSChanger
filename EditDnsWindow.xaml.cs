using System;
using System.Windows;
using System.Net;

namespace DNSChanger
{
    public partial class EditDnsWindow : Window
    {
        // Property to store the updated DNS entry
        public DnsEntry UpdatedDnsEntry { get; private set; }

        public EditDnsWindow()
        {
            InitializeComponent();
        }

        public EditDnsWindow(DnsEntry entryToEdit) : this()
        {
            if (entryToEdit != null)
            {
                TitleTextBox.Text = entryToEdit.Title ?? string.Empty;
                PrimaryDnsTextBox.Text = entryToEdit.PrimaryDns ?? string.Empty;
                SecondaryDnsTextBox.Text = entryToEdit.SecondaryDns ?? string.Empty;
            }
        }

        /// <summary>
        /// Validates if the given string is a valid IP address.
        /// </summary>
        /// <param name="ip">IP address as string</param>
        /// <returns>True if valid, otherwise false</returns>
        private bool IsValidIp(string ip)
        {
            return IPAddress.TryParse(ip, out _);
        }

        /// <summary>
        /// Handles the save button click event.
        /// Validates input fields and ensures DNS addresses are valid IPs before saving.
        /// </summary>
        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrimaryDnsTextBox.Text) ||
                string.IsNullOrWhiteSpace(SecondaryDnsTextBox.Text))
            {
                MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate DNS IP addresses
            if (!IsValidIp(PrimaryDnsTextBox.Text.Trim()) || !IsValidIp(SecondaryDnsTextBox.Text.Trim()))
            {
                MessageBox.Show("Invalid DNS address. Please enter a valid IP.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create updated DNS entry object
            UpdatedDnsEntry = new DnsEntry
            {
                Title = TitleTextBox.Text.Trim(),
                PrimaryDns = PrimaryDnsTextBox.Text.Trim(),
                SecondaryDns = SecondaryDnsTextBox.Text.Trim()
            };

            // Close the window and return the result
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// Handles the cancel button click event.
        /// Closes the window without saving any data.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
