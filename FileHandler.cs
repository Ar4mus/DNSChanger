using System.IO;
using System.Text.Json;

/// <summary>
/// Handles loading and saving DNS entries to a JSON file.
/// </summary>
public static class FileHandler
{
    /// <summary>
    /// The file path where DNS settings are stored.
    /// </summary>
    private static string FilePath => Path.Combine(Directory.GetCurrentDirectory(), "dns_settings.json");

    /// <summary>
    /// Default list of DNSs
    /// </summary>
    private static List<DnsEntry> DefaultDnsEntries => new List<DnsEntry>
    {
        new DnsEntry { Title = "Electro", PrimaryDns = "78.157.42.100", SecondaryDns = "78.157.42.101" },
        new DnsEntry { Title = "RadarGame", PrimaryDns = "10.202.10.10", SecondaryDns = "10.202.10.11" },
        new DnsEntry { Title = "Shekan", PrimaryDns = "178.22.122.100", SecondaryDns = "185.51.200.2" },
        new DnsEntry { Title = "Begzar", PrimaryDns = "185.55.226.26", SecondaryDns = "185.55.225.25" },
        new DnsEntry { Title = "403DNS", PrimaryDns = "10.202.10.202", SecondaryDns = "10.202.10.102" },
        new DnsEntry { Title = "Beshkan", PrimaryDns = "181.41.194.177", SecondaryDns = "181.41.194.186" },
        new DnsEntry { Title = "Google DNS", PrimaryDns = "8.8.8.8", SecondaryDns = "8.8.4.4" },
        new DnsEntry { Title = "Cloudflare DNS", PrimaryDns = "1.1.1.1", SecondaryDns = "1.0.0.1" },
        new DnsEntry { Title = "OpenDNS", PrimaryDns = "208.67.222.222", SecondaryDns = "208.67.220.220" }

    };

    /// <summary>
    /// Loads the list of DNS entries from the JSON file.
    /// </summary>
    /// <returns>A list of DnsEntry objects. Returns an empty list if the file does not exist or contains invalid data.</returns>
    public static List<DnsEntry> LoadDnsEntries()
    {
        // If the file does not exist, save the defaults
        if (!File.Exists(FilePath))
        {
            SaveDnsEntries(DefaultDnsEntries);
            return DefaultDnsEntries;
        }

        string json = File.ReadAllText(FilePath);
        return JsonSerializer.Deserialize<List<DnsEntry>>(json) ?? new List<DnsEntry>();
    }

    /// <summary>
    /// Saves the list of DNS entries to the JSON file.
    /// </summary>
    /// <param name="entries">The list of DNS entries to save.</param>
    public static void SaveDnsEntries(List<DnsEntry> entries)
    {
        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

}