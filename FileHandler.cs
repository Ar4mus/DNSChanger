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
        new DnsEntry { Title = "shelter 1", PrimaryDns = "2.189.86.103", SecondaryDns = "2.189.86.10" },
        new DnsEntry { Title = "shelter 2", PrimaryDns = "94.183.166.195", SecondaryDns = "94.183.166.209" },
        new DnsEntry { Title = "shelter 3", PrimaryDns = "2.189.86.41", SecondaryDns = "" },
        new DnsEntry { Title = "shelter 4", PrimaryDns = "2.189.86.10", SecondaryDns = "" },
        new DnsEntry { Title = "shelter 5", PrimaryDns = "2.186.86.103", SecondaryDns = "" },
        new DnsEntry { Title = "shelter 6", PrimaryDns = "2.189.86.31", SecondaryDns = "" },
        new DnsEntry { Title = "Shekan", PrimaryDns = "178.22.122.100", SecondaryDns = "185.51.200.2" },
        new DnsEntry { Title = "Begzar", PrimaryDns = "185.55.226.26", SecondaryDns = "185.55.225.25" },
        new DnsEntry { Title = "403DNS", PrimaryDns = "10.202.10.202", SecondaryDns = "10.202.10.102" },
        new DnsEntry { Title = "Beshkan", PrimaryDns = "181.41.194.177", SecondaryDns = "181.41.194.186" },
        new DnsEntry { Title = "Google DNS", PrimaryDns = "8.8.8.8", SecondaryDns = "8.8.4.4" },
        new DnsEntry { Title = "Cloudflare DNS", PrimaryDns = "1.1.1.1", SecondaryDns = "1.0.0.1" },
        new DnsEntry { Title = "OpenDNS", PrimaryDns = "208.67.222.222", SecondaryDns = "208.67.220.220" },
        new DnsEntry { Title = "Asiatech", PrimaryDns = "194.36.174.161", SecondaryDns = "178.22.122.100" },
        new DnsEntry { Title = "Quad9", PrimaryDns = "149.112.112.112", SecondaryDns = "9.9.9.9" },
        new DnsEntry { Title = "UltraDNS", PrimaryDns = "64.6.65.6", SecondaryDns = "64.6.64.6" },
        new DnsEntry { Title = "UltraDNS 2", PrimaryDns = "156.154.71.2", SecondaryDns = "156.154.70.2" },
        new DnsEntry { Title = "LagZero", PrimaryDns = "95.38.132.152", SecondaryDns = "95.38.132.153" },
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