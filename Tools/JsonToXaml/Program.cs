using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;
using OpenTemple.Core;
using OpenTemple.Core.Config;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Ui.Styles;
using OpenTemple.Core.Ui.Widgets;

namespace JsonToXaml
{
    internal class Program
    {
        private static readonly (string, string)[] Files =
        {
            ("ui/alert_ui.json", "AlertUi"),
            ("ui/camping_ui.json", "CampingUi"),
            ("ui/char_help.json", "CharHelpUi"),
            ("ui/char_inventory.json", "CharSheetInventoryUi"),
            ("ui/char_looting.json", "CharSheetLootingUi"),
            ("ui/char_paperdoll.json", "CharSheetPortraitUi"),
            ("ui/char_skills.json", "CharSheetSkillsUi"),
            ("ui/char_spells.json", "CharSheetSpellsUi"),
            ("ui/key_acquired_popup.json", "LogbookKeyAcquiredPopup"),
            ("ui/main_menu_cinematics.json", "MainMenuCinematicsDialog"),
            ("ui/main_menu_setpieces.json", "MainMenuSetPiecesDialog"),
            ("ui/options_ui.json", "OptionsUi"),
            ("ui/party_ui.json", "PartyUi"),
            ("ui/pc_creation/abilities_cleric_ui.json", "ClericFeaturesUi"),
            ("ui/pc_creation/abilities_ranger_ui.json", "RangerFeaturesUi"),
            ("ui/pc_creation/abilities_wizard_ui.json", "WizardFeaturesUi"),
            ("ui/pc_creation/abilities_ui.json", "AbilitiesPage"),
            ("ui/pc_creation/stats_ui.json", "AbilityScorePage"),
            ("ui/pc_creation/alignment_ui.json", "AlignmentPage"),
            ("ui/pc_creation/class_ui.json", "ClassPage"),
            ("ui/pc_creation/deity_ui.json", "DeityPage"),
            ("ui/pc_creation/feats_ui.json", "FeatsPage"),
            ("ui/pc_creation/gender_ui.json", "GenderPage"),
            ("ui/pc_creation/hair_ui.json", "HairPage"),
            ("ui/pc_creation/height_ui.json", "HeightPage"),
            ("ui/pc_creation/portrait_ui.json", "PortraitPage"),
            ("ui/pc_creation/race_ui.json", "RacePage"),
            ("ui/pc_creation/selection_list_ui.json", "SelectionList"),
            ("ui/pc_creation/skills_ui.json", "SkillsPage"),
            ("ui/pc_creation/spells_ui.json", "SpellsPage"),
            ("ui/pc_creation/voice_ui.json", "VoicePage"),
            ("ui/pc_creation/pc_creation_ui.json", "PCCreationDialog"),
            ("ui/party_creation/party_alignment.json", "PartyAlignmentDialog"),
            ("ui/pc_creation/stat_block_ui.json", "PlayerStatBlockWidget"),
            ("ui/pc_creation/stat_block_ability_score.json", "StatBlockAbilityScore"),
            ("ui/party_pool.json", "PartyPoolUi"),
            ("ui/save_game_ui.json", "SaveGameUi"),
            ("ui/townmap_ui.json", "TownMapUi"),
            ("ui/worldmap_ui.json", "WorldMapUi"),
            ("ui/popup_ui.json", "PopupUi"),
            ("ui/text_entry_ui.json", "TextEntryUi"),
        };

        public Program()
        {
            var configManager = new GameConfigManager(new GameFolders());
            Tig.FS = Tig.CreateFileSystem(configManager.Config.InstallationFolder, "D:/OpenTemple/Data");
            Globals.WidgetTextStyles = new WidgetTextStyles();
            Globals.WidgetButtonStyles = new WidgetButtonStyles();
        }

        private static void Main(string[] args)
        {
            var program = new Program();
            program.PreloadStyles();

            program.ProcessFiles();
        }

        private void ProcessFiles()
        {
            foreach (var (path, className) in Files)
            {
                ProcessFile(path, className);
            }
        }

        private void ProcessFile(string path, string className)
        {
            var doc = LoadDocument(path);
            var outPath = Path.Join(@"D:\OpenTemple\Core\Ui\Converted", className + ".xaml");

            var writer = new StringWriter();

            var root = doc.RootElement;

            var rootWidth = root.GetInt32Prop("width", 800);
            var rootHeight = root.GetInt32Prop("height", 600);

            writer.Write($@"
         <UserControl xmlns=""https://github.com/avaloniaui""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
        xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
        xmlns:w=""clr-namespace:OpenTemple.Widgets;assembly=OpenTemple.Widgets""
        mc:Ignorable=""d"" d:DesignWidth=""{rootWidth}"" d:DesignHeight=""{rootHeight}""
        x:Class=""OpenTemple.Core.Ui.Converted.{className}"">");

            if (root.TryGetProperty("__styles", out _) || root.TryGetProperty("__buttonStyles", out _))
            {
                writer.WriteLine("<UserControl.Styles>");

                void PrintSetter(string property, object value)
                {
                    writer.WriteLine($"<Setter Property=\"{property}\" Value=\"{value}\" />");
                }

                if (root.TryGetProperty("__styles", out var textStyles))
                {
                    foreach (var textStyleEl in textStyles.EnumerateArray())
                    {
                        writer.WriteLine(
                            $"<Style Selector=\"w|TextBlock.{textStyleEl.GetStringProp("id")}, .{textStyleEl.GetStringProp("id")} w|TextBlock\">");
                        if (textStyleEl.TryGetProperty("inherit", out var inheritProp))
                        {
                            writer.WriteLine("<!-- Inherits: " + inheritProp + " -->");
                        }

                        bool predefinedFont = false;

                        if (textStyleEl.TryGetProperty("fontFamily", out var fontFamilyProp))
                        {
                            var fontFamily = fontFamilyProp.GetString();
                            predefinedFont = true;
                            switch (fontFamily)
                            {
                                case "arial-10":
                                    PrintSetter("FontFamily", "{DynamicResource ArialFontFamily}");
                                    PrintSetter("FontSize", "10");
                                    break;
                                case "arial-12":
                                    PrintSetter("FontFamily", "{DynamicResource ArialFontFamily}");
                                    PrintSetter("FontSize", "12");
                                    break;
                                case "arial-bold-10":
                                    PrintSetter("FontFamily", "{DynamicResource ArialFontFamily}");
                                    PrintSetter("FontSize", "10");
                                    PrintSetter("FontWeight", "Bold");
                                    break;
                                case "arial-bold-24":
                                    PrintSetter("FontFamily", "{DynamicResource ArialFontFamily}");
                                    PrintSetter("FontSize", "24");
                                    PrintSetter("FontWeight", "Bold");
                                    break;
                                case "priory-12":
                                    PrintSetter("FontFamily", "{DynamicResource PrioryFontFamily}");
                                    PrintSetter("FontSize", "12");
                                    break;
                                default:
                                    PrintSetter("FontFamily", fontFamily);
                                    predefinedFont = false;
                                    break;
                            }
                        }

                        if (!predefinedFont && textStyleEl.TryGetProperty("pointSize", out var pointSize))
                        {
                            PrintSetter("FontSize", pointSize.GetInt32());
                        }

                        if (!predefinedFont && textStyleEl.TryGetProperty("bold", out var boldProp))
                        {
                            PrintSetter("FontWeight", boldProp.GetBoolean() ? "Bold" : "Normal");
                        }

                        if (!predefinedFont && textStyleEl.TryGetProperty("italic", out var italicProp))
                        {
                            PrintSetter("FontStyle", italicProp.GetBoolean() ? "Italic" : "Normal");
                        }

                        if (textStyleEl.TryGetProperty("align", out var alignProp))
                        {
                            if (alignProp.GetString() == "center")
                            {
                                PrintSetter("HorizontalAlignment", "Center");
                            }
                        }

                        if (textStyleEl.TryGetProperty("foreground", out var foregroundProp))
                        {
                            var brush = foregroundProp.GetBrush();
                            Debug.Assert(!brush.gradient);
                            if (brush.primaryColor == PackedLinearColorA.Black)
                            {
                                PrintSetter("Foreground", "Black");
                            }
                            else
                            {
                                PrintSetter("Foreground", brush.primaryColor.ToString());
                            }
                        }

                        if (textStyleEl.GetBoolProp("dropShadow", false))
                        {
                            var brush = textStyleEl.GetProperty("dropShadowBrush").GetBrush();
                            Debug.Assert(!brush.gradient);
                            if (brush.primaryColor == PackedLinearColorA.Black)
                            {
                                PrintSetter("DropShadowBrush", "Black");
                            }
                            else
                            {
                                PrintSetter("DropShadowBrush", brush.primaryColor.ToString());
                            }
                        }

                        writer.WriteLine("</Style>");
                    }
                }

                if (root.TryGetProperty("__buttonStyles", out var buttonStyles))
                {
                    foreach (var buttonStyleEl in buttonStyles.EnumerateArray())
                    {
                        writer.WriteLine($"<Style Selector=\"w|Button.{buttonStyleEl.GetStringProp("id")}\">");

                        var normalImage = buttonStyleEl.GetStringProp("normalImage", null);
                        if (normalImage != null)
                        {
                            PrintSetter("NormalImage", normalImage);
                        }

                        var hoverImage = buttonStyleEl.GetStringProp("hoverImage", null);
                        if (hoverImage != null)
                        {
                            PrintSetter("HoverImage", hoverImage);
                        }

                        var pressedImage = buttonStyleEl.GetStringProp("pressedImage", null);
                        if (pressedImage != null)
                        {
                            PrintSetter("PressedImage", pressedImage);
                        }

                        var disabledImage = buttonStyleEl.GetStringProp("disabledImage", null);
                        if (disabledImage != null)
                        {
                            PrintSetter("DisabledImage", disabledImage);
                        }

                        writer.WriteLine("</Style>");
                    }
                }

                writer.WriteLine("</UserControl.Styles>");
            }

            Process(writer, root);

            writer.WriteLine("</UserControl>");

            writer.Close();

            File.WriteAllText(outPath, PrettyXml(writer.ToString()));

            File.WriteAllText(outPath + ".cs", $@"
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace OpenTemple.Core.Ui.Converted
{{
    public class {className} : UserControl
    {{
        public {className}()
        {{
            AvaloniaXamlLoader.Load(this);
        }}
    }}
}}
");
        }

        private void PreloadStyles()
        {
            Tig.Fonts = new TigFonts();
            UiSystems.Tooltip = new TooltipUi();

            foreach (var file in Files)
            {
                WidgetDoc.Load(file.Item1);
            }
        }

        private static void Process(StringWriter writer, JsonElement element)
        {
            void PropToAttr(string propName, string attrName)
            {
                if (element.TryGetProperty(propName, out var prop))
                {
                    writer.WriteLine(attrName + "=\"" + prop.ToString() + "\"");
                }
            }

            void WriteChildren()
            {
                if (element.TryGetProperty("content", out var content) && content.ValueKind == JsonValueKind.Array)
                {
                    foreach (var contentElement in content.EnumerateArray())
                    {
                        Process(writer, contentElement);
                    }
                }

                if (element.TryGetProperty("children", out var children) && children.ValueKind == JsonValueKind.Array)
                {
                    foreach (var childElement in children.EnumerateArray())
                    {
                        Process(writer, childElement);
                    }
                }
            }

            void WriteName()
            {
                var id = element.GetStringProp("id", null);
                if (id != null)
                {
                    writer.WriteLine($"x:Name=\"{id}\"");
                }
            }

            void WriteWidthHeight()
            {
                PropToAttr("width", "Width");
                PropToAttr("height", "Height");
            }

            void WriteCanvasPos()
            {
                PropToAttr("x", "Canvas.Left");
                PropToAttr("y", "Canvas.Top");
            }

            void WriteContentPosition()
            {
                var x = element.GetInt32Prop("x", 0);
                var y = element.GetInt32Prop("y", 0);
                var width = element.GetInt32Prop("width", 0);
                var height = element.GetInt32Prop("height", 0);

                writer.WriteLine("Canvas.Left=\"" + x + "\"");
                if (width > 0)
                {
                    PropToAttr("width", "Width");
                }
                else
                {
                    writer.WriteLine("Canvas.Right=\"0\"");
                }

                writer.WriteLine("Canvas.Top=\"" + y + "\"");
                if (height > 0)
                {
                    PropToAttr("height", "Height");
                }
                else
                {
                    writer.WriteLine("Canvas.Bottom=\"0\"");
                }
            }

            void WriteClasses()
            {
                var classes = new List<string>();
                if (element.TryGetProperty("textStyle", out var textStyle))
                {
                    classes.Add(textStyle.GetString());
                }

                if (element.TryGetProperty("style", out var style))
                {
                    classes.Add(style.GetString());

                    var buttonStyle = Globals.WidgetButtonStyles.GetStyle(style.GetString());
                    var textStyleId = buttonStyle.textStyleId;

                    while (buttonStyle.inherits != null)
                    {
                        classes.Add(buttonStyle.inherits);
                        buttonStyle = Globals.WidgetButtonStyles.GetStyle(buttonStyle.inherits);
                        textStyleId ??= buttonStyle.textStyleId;
                    }

                    if (textStyleId != null)
                    {
                        classes.Add(textStyleId);
                    }
                }

                if (classes.Count > 0)
                {
                    writer.WriteLine($"Classes=\"{string.Join(" ", classes)}\"");
                }
            }

            JsonElement text;
            switch (element.GetStringProp("type"))
            {
                case "image":
                    writer.WriteLine("<Image ");
                    WriteName();
                    WriteContentPosition();
                    if (element.TryGetProperty("path", out var path))
                    {
                        writer.WriteLine($"Source=\"{path.GetString()}\"");
                    }

                    writer.WriteLine("/>");
                    break;
                case "rectangle":
                    writer.WriteLine("<Rectangle ");
                    WriteName();
                    WriteContentPosition();
                    writer.WriteLine("/>");
                    break;
                case "text":
                    writer.WriteLine("<w:TextBlock ");
                    WriteName();
                    WriteContentPosition();
                    WriteClasses();
                    if (element.TryGetProperty("text", out text) && text.GetString() != "")
                    {
                        writer.WriteLine($"Text=\"{{w:T {text.GetString()}}}\"");
                    }

                    writer.WriteLine("/>");
                    break;
                case "container":
                    writer.WriteLine("<w:Canvas ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    writer.WriteLine(">");
                    WriteChildren();
                    writer.WriteLine("</w:Canvas>");
                    break;
                case "button":
                    writer.WriteLine("<w:Button ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    WriteClasses();

                    if (element.TryGetProperty("text", out text) && text.GetString() != "")
                    {
                        writer.WriteLine($"Content=\"{{w:T {text.GetString()}}}\"");
                    }

                    writer.WriteLine("/>");
                    break;
                case "scrollBar":
                    writer.WriteLine("<ScrollBar ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    writer.WriteLine("/>");
                    break;
                case "scrollView":
                    writer.WriteLine("<ScrollViewer ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    writer.WriteLine(">");
                    WriteChildren();
                    writer.WriteLine("</ScrollViewer>");
                    break;
                case "tabBar":
                    writer.WriteLine("<TabControl ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    writer.WriteLine("/>");
                    break;
                case "custom":
                    writer.Write("<");
                    writer.Write(element.GetStringProp("customType"));
                    writer.Write(" ");
                    WriteName();
                    WriteCanvasPos();
                    WriteWidthHeight();
                    writer.WriteLine(">");
                    writer.WriteLine(element);
                    writer.WriteLine("</" + element.GetStringProp("customType") + ">");
                    break;
                default:
                    throw new ArgumentException(element.ToString());
            }
        }

        static string PrettyXml(string xml)
        {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = true;

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings))
            {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }

        private static JsonDocument LoadDocument(string path)
        {
            var content = Tig.FS.ReadBinaryFile(path);
            return JsonDocument.Parse(content);
        }
    }
}
