using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using SpicyTemple.Core;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO.MesFiles;
using SpicyTemple.Core.IO.SaveGames.Archive;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.TigSubsystems;

namespace Launcher
{
    public static class LauncherProgram
    {
        public static void Main(string[] args)
        {
/*
            using (var stream = new StreamWriter("D:/skills.txt"))
            {
                //foreach (var field in typeof(WellKnownSpells).GetFields())
                //{
                //   var constant = (int) field.GetRawConstantValue();
                //    stream.WriteLine($"{(int) constant}: 'WellKnownSpells.{field.Name}',");
                //}

                string[] names = Enum.GetNames(typeof(SkillId));
                foreach (var name in names)
                {
                    var literal = Enum.Parse<SkillId>(name);
                    stream.WriteLine($"{(int) literal}: 'SkillId.{name}',");
                }
            }

            return;*/

            if (args.Length > 0 && args[0] == "--extract-save")
            {
                ExtractSaveArchive.Main(args.Skip(1).ToArray());
                return;
            }
            if (args.Length == 2 && args[0] == "--mes-to-json") {
                var mesContent = MesFile.Read(args[1]);
                var newFile = Path.ChangeExtension(args[1], ".json");
                var options = new JsonSerializerOptions();
                options.WriteIndented = true;
                var jsonContent = JsonSerializer.Serialize(mesContent.ToDictionary(
                    kvp => kvp.Key.ToString(),
                    kvp => kvp.Value
                ), options);
                File.WriteAllText(newFile, jsonContent);
                return;
            }
            if (args.Length > 0 && args[0] == "--dump-addresses")
            {
                using var writer = new StreamWriter("addresses.json");
                writer.WriteLine("{");
                foreach (var definedType in typeof(TempleDllLocationAttribute).Assembly.DefinedTypes)
                {

                    if (definedType.GetCustomAttribute<DontUseForAutoTranslationAttribute>() != null)
                    {
                        continue;
                    }

                    string accessBase = null;

                    if (definedType.Namespace?.StartsWith("SpicyTemple") != true)
                    {
                        continue;
                    }

                    // How do we access the type???
                    var interfaces = definedType.ImplementedInterfaces.Select(i => i.Name).ToHashSet();
                    if (interfaces.Contains("IGameSystem"))
                    {
                        if (definedType.Name == "D20StatSystem")
                        {
                            accessBase = "GameSystems.Stat";
                        }
                        else
                        {
                            accessBase = "GameSystems." + definedType.Name.Replace("System", "");
                        }                        
                    }
                    else
                    {
                        accessBase = definedType.Name switch
                        {
                            "SpellScriptSystem" => "GameSystems.Script.Spells",
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
                        accessBase = definedType.Name;
                        
                        // Check all static members
                        foreach (var member in definedType.GetMembers(BindingFlags.Static|BindingFlags.Public))
                        {
                            if (member.GetCustomAttribute<DontUseForAutoTranslationAttribute>() != null)
                            {
                                continue;
                            }

                            var attrs = member.GetCustomAttributes<TempleDllLocationAttribute>();
                            foreach (var attr in attrs)
                            {
                                writer.WriteLine($"\"0x{attr.Location:x}\": \"{accessBase}.{member.Name}\",");
                            }
                        }

                        continue;
                    }

                    foreach (var member in definedType.DeclaredMembers)
                    {
                        // Switch to using the class name if the member is static
                        var actualAccessBase = accessBase;
                        if (member is FieldInfo field && field.IsStatic)
                        {
                            actualAccessBase = definedType.Name;
                        }
                        else if (member is MethodBase method && method.IsStatic)
                        {
                            actualAccessBase = definedType.Name;
                        }
                        else if (member is PropertyInfo prop && prop.GetMethod.IsStatic)
                        {
                            actualAccessBase = definedType.Name;
                        }

                    var attrs = member.GetCustomAttributes<TempleDllLocationAttribute>();
                        foreach (var attr in attrs)
                        {
                            writer.WriteLine($"\"0x{attr.Location:x}\": \"{actualAccessBase}.{member.Name}\",");
                        }
                    }
                }

                writer.WriteLine("\"\": \"\"");
                writer.WriteLine("}");

                return;
            }

            using var spicyTemple = new SpicyTemple.Core.MainGame();

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