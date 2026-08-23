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
        new DnsEntry { Title = "shelter 1", PrimaryDns = "78.157.62.66", SecondaryDns = "10.30.72.39" },
        new DnsEntry { Title = "shelter 2", PrimaryDns = "2.189.86.30", SecondaryDns = "78.157.44.138" },
        new DnsEntry { Title = "shelter programmer", PrimaryDns = "78.157.34.250", SecondaryDns = "78.157.34.251" },
        new DnsEntry { Title = "shelter mix", PrimaryDns = "78.157.62.66", SecondaryDns = "78.157.44.138" },
        new DnsEntry { Title = "Electro", PrimaryDns = "78.157.42.100", SecondaryDns = "78.157.42.101" },
        new DnsEntry { Title = "RadarGame", PrimaryDns = "10.202.10.10", SecondaryDns = "10.202.10.11" },
        new DnsEntry { Title = "Shekan", PrimaryDns = "178.22.122.100", SecondaryDns = "185.51.200.2" },
        new DnsEntry { Title = "Begzar", PrimaryDns = "185.55.226.26", SecondaryDns = "185.55.225.25" },
        new DnsEntry { Title = "403DNS", PrimaryDns = "10.202.10.202", SecondaryDns = "10.202.10.102" },
        new DnsEntry { Title = "Beshkan", PrimaryDns = "181.41.194.177", SecondaryDns = "181.41.194.186" },
        new DnsEntry { Title = "Google DNS", PrimaryDns = "8.8.8.8", SecondaryDns = "8.8.4.4" },
        new DnsEntry { Title = "Cloudflare DNS", PrimaryDns = "1.1.1.1", SecondaryDns = "1.0.0.1" },
        new DnsEntry { Title = "OpenDNS", PrimaryDns = "208.67.222.222", SecondaryDns = "208.67.220.220" },
        new DnsEntry { Title = "Asiatech", PrimaryDns = "194.36.174.161", SecondaryDns = "178.22.122.100" },
        new DnsEntry { Title = "new 1", PrimaryDns = "109.68.8.51", SecondaryDns = "74.82.42.42" },
        new DnsEntry { Title = "new 2", PrimaryDns = "2.189.86.10", SecondaryDns = "2.189.95.11" },
        new DnsEntry { Title = "new 3", PrimaryDns = "95.38.132.152", SecondaryDns = "95.38.132.153" },
        new DnsEntry { Title = "new 5", PrimaryDns = "178.22.122.100", SecondaryDns = "185.51.200.2" },
        new DnsEntry { Title = "new 6", PrimaryDns = "78.157.42.100", SecondaryDns = "10.202.10.11" },
        new DnsEntry { Title = "new 7", PrimaryDns = "178.22.122.100", SecondaryDns = "78.157.42.100" },
        new DnsEntry { Title = "new 8", PrimaryDns = "10.202.10.10", SecondaryDns = "196.251.117.155" },
        new DnsEntry { Title = "new 9", PrimaryDns = "185.55.226.26", SecondaryDns = "74.82.42.42" },
        new DnsEntry { Title = "Quad9", PrimaryDns = "149.112.112.112", SecondaryDns = "9.9.9.9" },
        new DnsEntry { Title = "UltraDNS", PrimaryDns = "64.6.65.6", SecondaryDns = "64.6.64.6" },
        new DnsEntry { Title = "UltraDNS 2", PrimaryDns = "156.154.71.2", SecondaryDns = "156.154.70.2" },
        new DnsEntry { Title = "LagZero", PrimaryDns = "95.38.132.152", SecondaryDns = "95.38.132.153" },
        new DnsEntry { Title = "new 10", PrimaryDns = "78.157.34.250", SecondaryDns = "176.99.11.77" },
        new DnsEntry { Title = "PIA", PrimaryDns = "76.223.113.79", SecondaryDns = "76.223.86.98" }
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