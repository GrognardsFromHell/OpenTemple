using System.Collections.Generic;
using System.IO;

namespace OpenTemple.Core.Startup;

/// <summary>
/// Validate a presumed ToEE installation folder.
/// </summary>
public static class ToEEInstallationValidator
{
    private static readonly string[] CheckFiles =
    {
        "tig.dat",
        "ToEE1.dat",
        "ToEE2.dat",
        "ToEE3.dat",
        "ToEE4.dat",
        "Modules/ToEE.dat"
    };

    public static ValidationReport Validate(string path)
    {
        var report = new ValidationReport();
        report.IsValid = true;

        if (string.IsNullOrEmpty(path))
        {
            report.IsValid = false;
            report.Messages.Add("No installation directory was set.");
            return report;
        }

        if (!Directory.Exists(path))
        {
            report.IsValid = false;
            report.Messages.Add("The installation directory is missing.");
            return report;
        }

        // Check for the individual archives we'd load
        foreach (var file in CheckFiles)
        {
            var filePath = Path.Join(path, file);
            if (!File.Exists(filePath))
            {
                report.IsValid = false;
                report.Messages.Add($"{file} is missing.");
            }
        }

        return report;
    }
}

public class ValidationReport
{
    public bool IsValid { get; set; }

    public List<string> Messages { get; } = new();
}