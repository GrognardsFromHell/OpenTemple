using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;

namespace ConvertMapMarkers
{
    /// <summary>
    /// Converts old townmap_ui_placed_flag_locations.mes to the new JSON format.
    /// Supply a ToEE installation directory, and the files will be read the usual way.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option(
                    "-i",
                    "Directory where Temple of Elemental Evil is installed.")
                {
                    Argument = new Argument<DirectoryInfo>(),
                    Required = true
                },
                new Option(
                    "-o",
                    "Directory where resulting JSON files will be written to.")
                {
                    Argument = new Argument<DirectoryInfo>(),
                    Required = true
                }
            };

            rootCommand.Description = "Convert town map markers from the Vanilla MES to the new JSON format.";

            rootCommand.Handler =
                CommandHandler.Create<DirectoryInfo, DirectoryInfo>((i, o) => Convert(i.FullName, o.FullName));

            return rootCommand.InvokeAsync(args).Result;
        }

        private static void Convert(string toeeDir, string outputDir)
        {
            Directory.CreateDirectory(outputDir);
            using var fs = TroikaVfs.CreateFromInstallationDir(toeeDir);

            // Loads the map list
            var maps = MapListParser.Parse(fs);

            var entries = fs.ReadMesFile("rules/townmap_ui_placed_flag_locations.mes");

            var mapCount = int.Parse(entries[1]);

            for (var mapIndex = 0; mapIndex < mapCount; mapIndex++)
            {
                var mapKey = 100 * (mapIndex + 1);
                var mapId = int.Parse(entries[mapKey]);
                // See MapSystem.GetDataDir
                var mapDataDir = Path.Join(outputDir, $"maps/{maps[mapId].name}");
                Directory.CreateDirectory(mapDataDir);

                using var stream = new FileStream($"{mapDataDir}/map_markers.json", FileMode.Create);
                var options = new JsonWriterOptions {Indented = true};
                using var writer = new Utf8JsonWriter(stream, options);

                writer.WriteStartObject();
                writer.WriteString("$schema", "https://schemas.opentemple.de/townMapMarkers.json");
                writer.WritePropertyName("markers");
                writer.WriteStartArray();

                var markerCount = int.Parse(entries[mapKey + 1]);
                for (var markerIndex = 0; markerIndex < markerCount; markerIndex++)
                {
                    var markerKey = mapKey + 20 + markerIndex;
                    var locationParts = entries[markerKey].Split(",");
                    var x = int.Parse(locationParts[0].Trim());
                    var y = int.Parse(locationParts[1].Trim());

                    // Interestingly, ToEE applied a hard coded offset
                    x -= 4;
                    y -= 8;

                    writer.WriteStartObject();
                    writer.WriteNumber("id", markerIndex);
                    writer.WriteBoolean("initiallyVisible", false);
                    // For the text, we reference the translation file
                    // It however uses a different numbering scheme since ToEE itself will read it sequentially
                    // rather than by key, but our translation system must reference it by key.
                    var translationKey = markerKey - 110;
                    writer.WriteString("text", $"#{{townmap_markers:{translationKey}}}");
                    writer.WriteNumber("x", x);
                    writer.WriteNumber("y", y);
                    writer.WriteEndObject();
                }

                writer.WriteEndArray(); // markers
                writer.WriteEndObject();
            }
        }
    }
}