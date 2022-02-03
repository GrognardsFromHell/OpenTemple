using System;

namespace OpenTemple.Core.Startup;

/// <summary>
/// Indicates that the ToEE installation couldn't be found.
/// </summary>
public class ToEENotFoundException : Exception
{
    /// <summary>
    /// The assumed path where ToEE may be installed. May be null to indicate the path is unknown.
    /// </summary>
    public string Path { get; }

    public ToEENotFoundException(string path)
    {
        Path = path;
    }
}