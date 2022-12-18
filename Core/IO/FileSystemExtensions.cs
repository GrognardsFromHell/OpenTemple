using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using OpenTemple.Core.IO.MesFiles;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.IO;

/// <summary>
/// Helper functions that wrap simple file readers for use with a Vfs.
/// </summary>
public static class FileSystemExtensions
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private static readonly char[] DirSeparators = {'/', '\\'};

    public static T ReadJsonFile<T>(this IFileSystem vfs, string path, JsonTypeInfo<T> jsonTypeInfo)
    {
        using var owner = vfs.ReadFile(path);

        var result = JsonSerializer.Deserialize(owner.Memory.Span, jsonTypeInfo);
        if (result == null)
        {
            throw new InvalidDataException("The contents of JSON file " + path + " must not be a null literal");
        }

        return result;
    }

    public static Dictionary<int, string> ReadMesFile(this IFileSystem vfs, string path, bool withPatches = true)
    {
        Dictionary<int, string> result;
        {
            using var content = vfs.ReadFile(path);
            result = MesFile.Read(path, content.Memory.Span);
        }

        if (withPatches)
        {
            var patchDir = Path.ChangeExtension(path, ".d");
            var patchFiles = vfs.Search($"{patchDir}/*.mes").ToList();
            patchFiles.Sort(); // Apply natural sort
            foreach (var patchFile in patchFiles)
            {
                var patchContent = ReadMesFile(vfs, patchFile, false);
                Logger.Debug("Applying {0} patches from {1}", patchContent.Count, patchFile);
                foreach (var entry in patchContent)
                {
                    result[entry.Key] = entry.Value;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Searches for patterns such as directory1\directory2\*.txt.
    /// </summary>
    public static IEnumerable<string> Search(this IFileSystem fs, string searchPattern)
    {
        var lastIdxOfSep = searchPattern.LastIndexOfAny(DirSeparators);

        var indexOfWildcard = searchPattern.IndexOf('*');
        if (indexOfWildcard == -1)
        {
            // No wildcard means it can only result in a single entry anyway
            if (fs.DirectoryExists(searchPattern) || fs.FileExists(searchPattern))
            {
                yield return searchPattern;
            }

            yield break;
        }

        if (searchPattern.LastIndexOf('*') != indexOfWildcard)
        {
            throw new ArgumentException("Search only supports a single wildcard.");
        }

        if (indexOfWildcard < lastIdxOfSep)
        {
            throw new ArgumentException("Search only supports wildcards in the filename portion.");
        }

        var directoryPath = lastIdxOfSep != -1
            ? searchPattern.Substring(0, lastIdxOfSep)
            : "";
        string resultPrefix = directoryPath;
        if (resultPrefix.Length > 0 && !resultPrefix.EndsWith('/'))
        {
            resultPrefix += '/';
        }

        var filenamePart = lastIdxOfSep != -1
            ? searchPattern.Substring(lastIdxOfSep + 1)
            : searchPattern;
        indexOfWildcard = filenamePart.IndexOf('*');
        Trace.Assert(indexOfWildcard != -1);

        // Extract the prefix/suffix of the pattern as spans
        var prefix = filenamePart.Substring(0, indexOfWildcard);
        var suffix = filenamePart.Substring(indexOfWildcard + 1);

        foreach (var filename in fs.ListDirectory(directoryPath))
        {
            if (filename.Length < prefix.Length + suffix.Length)
            {
                continue; // Can't match
            }

            if (filename.StartsWith(prefix) && filename.EndsWith(suffix))
            {
                yield return resultPrefix + filename;
            }
        }
    }
}