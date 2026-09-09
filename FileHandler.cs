using System.IO;
using System.Text.Json;

/// <summary>
/// Handles loading and saving DNS entries to a JSON file.
/// </summary>
public static class FileHandler
{
    /// <summary>
    /// The file path where DNS settings are stored under %APPDATA%\DNSChanger.
    /// A fixed location is used because the current directory is not reliable
    /// for an elevated app (it can become C:\Windows\System32 when started
    /// from Task Scheduler or certain shortcuts).
    /// </summary>
    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "DNSChanger",
        "dns_settings.json");

    /// <summary>
    /// Legacy location of the settings file next to the executable.
    /// Used only to migrate existing user settings once.
    /// </summary>
    private static string LegacyFilePath => Path.Combine(AppContext.BaseDirectory, "dns_settings.json");

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
    /// Falls back to the defaults if the file is missing or corrupt.
    /// </summary>
    /// <returns>A list of DnsEntry objects.</returns>
    public static List<DnsEntry> LoadDnsEntries()
    {
        // One-time migration from versions that stored settings next to the executable
        if (!File.Exists(FilePath) && File.Exists(LegacyFilePath))
        {
            try
            {
                Directory.CreateDirectory(GetSettingsDirectory());
                File.Copy(LegacyFilePath, FilePath, overwrite: false);
            }
            catch
            {
                // Migration is best-effort; continue with a normal load
            }
        }

        // If the file does not exist, save the defaults
        if (!File.Exists(FilePath))
        {
            SaveDnsEntries(DefaultDnsEntries);
            return DefaultDnsEntries;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            var entries = JsonSerializer.Deserialize<List<DnsEntry>>(json);

            // Drop null items and normalize null properties so the ComboBox never receives broken data
            return (entries ?? new List<DnsEntry>())
                .OfType<DnsEntry>()
                .Select(entry => new DnsEntry
                {
                    Title = entry.Title ?? string.Empty,
                    PrimaryDns = entry.PrimaryDns ?? string.Empty,
                    SecondaryDns = entry.SecondaryDns ?? string.Empty
                })
                .ToList();
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            // Preserve the corrupt file for inspection instead of crashing at startup
            BackupCorruptFile();

            SaveDnsEntries(DefaultDnsEntries);
            return DefaultDnsEntries;
        }
    }

    /// <summary>
    /// Saves the list of DNS entries to the JSON file.
    /// </summary>
    /// <param name="entries">The list of DNS entries to save.</param>
    public static void SaveDnsEntries(List<DnsEntry> entries)
    {
        Directory.CreateDirectory(GetSettingsDirectory());
        string json = JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(FilePath, json);
    }

    private static string GetSettingsDirectory()
    {
        return Path.GetDirectoryName(FilePath)!;
    }

    /// <summary>
    /// Renames the corrupt settings file so it can be inspected later.
    /// </summary>
    private static void BackupCorruptFile()
    {
        try
        {
            Directory.CreateDirectory(GetSettingsDirectory());
            string backupPath = FilePath + ".corrupt-bak";
            File.Move(FilePath, backupPath, overwrite: true);
        }
        catch
        {
            // Best-effort only
        }
    }
}
