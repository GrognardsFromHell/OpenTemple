using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SpicyTemple.Core;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.SaveGames.Archive;
using SpicyTemple.Core.TigSubsystems;

namespace Launcher
{
    public static class LauncherProgram
    {
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--extract-save")
            {
                ExtractSaveArchive.Main(args.Skip(1).ToArray());
                return;
            }
            if (args.Length > 0 && args[0] == "--dump-addresses")
            {
                using var writer = new StreamWriter("addresses.json");
                writer.WriteLine("{");
                foreach (var definedType in typeof(TempleDllLocationAttribute).Assembly.DefinedTypes)
                {
                    string accessBase = null;

                    if (definedType.Namespace?.StartsWith("SpicyTemple") != true)
                    {
                        continue;
                    }

                    // How do we access the type???
                    var interfaces = definedType.ImplementedInterfaces.Select(i => i.Name).ToHashSet();
                    if (interfaces.Contains("IGameSystem"))
                    {
                        accessBase = "GameSystems." + definedType.Name.Replace("System", "");
                    }
                    else
                    {
                        accessBase = definedType.Name switch
                        {
                            "GameUiBridge" => "GameUiBridge",
                            "D20ActionSystem" => "GameSystems.D20.Actions",
                            "D20CombatSystem" => "GameSystems.D20.Combat",
                            "D20DamageSystem" => "GameSystems.D20.Damage",
                            "D20ObjectRegistry" => "GameSystems.D20.ObjectRegistry",
                            "BonusSystem" => "GameSystems.D20.BonusSystem",
                            "ConditionRegistry" => "GameSystems.D20.Conditions",
                            "D20StatusSystem" => "GameSystems.D20.StatusSystem",
                            "D20Initiative" => "GameSystems.D20.Initiative",
                            "RadialMenuSystem" => "GameSystems.D20.RadialMenu",
                            "HotkeySystem" => "GameSystems.D20.Hotkeys",
                            "D20BuffDebuffSystem" => "GameSystems.D20.BuffDebuff",

                            // TIG
                            "TigFonts" => "Tig.Fonts",
                            _ => null
                        };
                    }

                    if (accessBase == null)
                    {
                        continue;
                    }



                    foreach (var member in definedType.DeclaredMembers)
                    {
                        var attrs = member.GetCustomAttributes<TempleDllLocationAttribute>();
                        foreach (var attr in attrs)
                        {
                            writer.WriteLine($"\"0x{attr.Location:x}\": \"{accessBase}.{member.Name}\",");
                        }
                    }
                }

                writer.WriteLine("\"\": \"\"");
                writer.WriteLine("}");

                return;
            }

            using var spicyTemple = new SpicyTemple.Core.SpicyTemple();

            spicyTemple.Run();

            var camera = Tig.RenderingDevice.GetCamera();
            camera.CenterOn(0, 0, 0);

            var gameLoop = new GameLoop(
                Tig.MessageQueue,
                Tig.RenderingDevice,
                Tig.ShapeRenderer2d,
                Globals.Config.Rendering,
                Tig.DebugUI
            );
            gameLoop.Run();
        }
    }
}