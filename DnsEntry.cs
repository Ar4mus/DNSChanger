using System;

/// <summary>
/// Represents a DNS entry with a title, primary DNS, and secondary DNS.
/// </summary>
public class DnsEntry
{
    /// <summary>
    /// Gets or sets the title of the DNS entry.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the primary DNS address.
    /// </summary>
    public string PrimaryDns { get; set; }

    /// <summary>
    /// Gets or sets the secondary DNS address.
    /// </summary>
    public string SecondaryDns { get; set; }

    /// <summary>
    /// Returns a string representation of the DNS entry.
    /// </summary>
    /// <returns>A formatted string containing the title and DNS addresses.</returns>
    public override string ToString()
    {
        return $"{Title} - {PrimaryDns}, {SecondaryDns}";
    }
}
