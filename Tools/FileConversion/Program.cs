using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace ConvertMapToText
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            var mapMobiles = new Command("map-mobiles",
                "Converts the objects found on a map to text format for easy debugging.")
            {
                new Argument<int>("map-id")
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Description = "Map ID"
                }
            };
            mapMobiles.Handler = CommandHandler.Create<DirectoryInfo, int>((toee, mapId) =>
                DumpMap(toee.FullName, mapId));

            var protos = new Command("protos", "Converts all protos to a text-based format.");
            protos.Handler = CommandHandler.Create<DirectoryInfo>(toee => ConvertProtos(toee.FullName));

            // Create a root command with some options
            var rootCommand = new RootCommand
            {
                new Option<DirectoryInfo>("--toee", "Directory where ToEE is installed")
                {
                    Argument = new Argument<DirectoryInfo>()
                },
                mapMobiles,
                protos
            };

            return rootCommand.Invoke(args);
        }

        private static void ConvertProtos(string toeeDir)
        {
            ProtosConverter.Convert(toeeDir);
        }

        private static void DumpMap(string toeeDir, int mapId)
        {
            MapMobileConverter.Convert(toeeDir, mapId);
        }
    }
}