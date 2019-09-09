using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using SharpDX.Multimedia;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public readonly struct DaylightLookupTable
    {
        private readonly LinearColor[] _colors; // One per hour of the day

        private readonly LinearColor _colorAtDawn;

        private readonly LinearColor _colorAtDusk;

        public DaylightLookupTable(LinearColor[] colors, LinearColor colorAtDawn, LinearColor colorAtDusk)
        {
            _colors = (LinearColor[]) colors.Clone();
            _colorAtDawn = colorAtDawn;
            _colorAtDusk = colorAtDusk;
        }

        public LinearColor GetColor(int hour, int minute, int second)
        {
            var nextHour = (hour + 1) % 24;
            var fractionOfHour = (second + minute * 60) / 3600.0f;
            return hour switch
            {
                6 => LinearColor.Lerp(_colorAtDawn, _colors[nextHour], fractionOfHour),
                18 => LinearColor.Lerp(_colorAtDusk, _colors[nextHour], fractionOfHour),
                _ => LinearColor.Lerp(_colors[hour], _colors[nextHour], fractionOfHour)
            };
        }
    }

    /// <summary>
    /// Formerly known as "map daylight system"
    /// </summary>
    public class LightSystem : IGameSystem, IBufferResettingSystem
    {
        [TempleDllLocation(0x10B5DC88)]
        private const bool IsEditor = false;

        [TempleDllLocation(0x11869200)]
        public LegacyLight GlobalLight { get; private set; }

        [TempleDllLocation(0x118691E0)]
        public bool IsGlobalLightEnabled { get; private set; } = true;

        [TempleDllLocation(0x10B5DC80)]
        public bool IsNight { get; private set; }

        [TempleDllLocation(0x10B5DC50)]
        private bool _updateToggle;

        [TempleDllLocation(0x10B5DDC4)]
        private readonly Dictionary<int, DaylightLookupTable> _lightColorLookupTablesByMap;

        private readonly Dictionary<int, DaylightLookupTable> _terrainTintLookupTablesByMap;

        // The color LUT for the current map
        [TempleDllLocation(0x10B5DB28)] [TempleDllLocation(0x10B5DC5C)] [TempleDllLocation(0x10B5DDCC)]
        private DaylightLookupTable? _lightColorLookupTable;

        [TempleDllLocation(0x10B5DC90)] [TempleDllLocation(0x10B5DDB8)] [TempleDllLocation(0x10B5DC68)]
        private DaylightLookupTable? _terrainTintLookupTable;

        [TempleDllLocation(0x100a7d40)]
        public LightSystem()
        {
            _updateToggle = true;

            var defaultSunlightColor = new LinearColor(1.5f, 1.5f, 1.5f);
            GlobalLight = new LegacyLight
            {
                type = LegacyLightType.LLT_DIRECTIONAL,
                Color = defaultSunlightColor,
                dir = new Vector3(-0.707f, -0.866f, 0)
            };

            var daylightRules = Tig.FS.ReadMesFile("rules/daylight.mes");
            _terrainTintLookupTablesByMap = DaylightRulesParser.LoadLookupTables(daylightRules, 0);
            _lightColorLookupTablesByMap = DaylightRulesParser.LoadLookupTables(daylightRules, 24);
        }

        [TempleDllLocation(0x100a5b30)]
        public void ResetBuffers()
        {
            // TODO
        }

        [TempleDllLocation(0x100a7860)]
        public void Load(string dataDir)
        {
            using var reader = Tig.FS.OpenBinaryReader($"{dataDir}/global.lit");

            IsGlobalLightEnabled = reader.ReadInt32() != 0;

            var globalLight = new LegacyLight();
            globalLight.type = reader.ReadInt32() switch
            {
                1 => LegacyLightType.LLT_POINT,
                2 => LegacyLightType.LLT_SPOT,
                3 => LegacyLightType.LLT_DIRECTIONAL,
                _ => throw new InvalidOperationException("Invalid global light type.")
            };
            globalLight.Color.R = reader.ReadSingle();
            globalLight.Color.G = reader.ReadSingle();
            globalLight.Color.B = reader.ReadSingle();
            globalLight.pos = reader.ReadVector3();
            globalLight.dir = reader.ReadVector3();
            globalLight.range = reader.ReadSingle();
            globalLight.phi = reader.ReadSingle();
            globalLight.dir = Vector3.Normalize(globalLight.dir);

            GlobalLight = globalLight;
        }

        // Sets the info from daylight.mes based on map id
        [TempleDllLocation(0x100a7040)]
        public void SetMapId(int mapId)
        {
            if (_lightColorLookupTablesByMap.TryGetValue(mapId, out var lightColor))
            {
                _lightColorLookupTable = lightColor;
            }
            else
            {
                _lightColorLookupTable = null;
            }

            if (_terrainTintLookupTablesByMap.TryGetValue(mapId, out var terrainTint))
            {
                _terrainTintLookupTable = terrainTint;
            }
            else
            {
                _terrainTintLookupTable = null;
            }
        }

        [TempleDllLocation(0x100a7f80)]
        public void Dispose()
        {
            // TODO
        }

        [TempleDllLocation(0x100A85F0)]
        public void RemoveAttachedTo(GameObjectBody obj)
        {
            var renderFlags = obj.GetUInt32(obj_f.render_flags);
            if ((renderFlags & 0x80000000) != 0)
            {
                var lightHandle = obj.GetInt32(obj_f.light_handle);
                if (lightHandle != 0)
                {
                    // TODO: Free sector light
                    throw new NotImplementedException();
                }

                obj.SetUInt32(obj_f.render_flags, renderFlags & ~0x80000000);
            }
        }

        [TempleDllLocation(0x100a88c0)]
        public void SetColors(PackedLinearColorA indoorColor, PackedLinearColorA outdoorColor)
        {
            // TODO
            if (_updateToggle)
            {
                UpdateDayNightStatus();
            }
        }

        [TempleDllLocation(0x100a8840)]
        private void UpdateDayNightStatus()
        {
            // Previously a buffer in the ground/terrain system was reset here,
            // as well as the screen marked as "dirty" in the terrain system
            foreach (var sector in GameSystems.MapSector.LoadedSectors)
            {
                RefreshNocturnalScenery(sector);
            }
        }

        // TODO: It's unclear whether this is actually used
        [TempleDllLocation(0x100a8690)]
        private void RefreshNocturnalScenery(Sector sector)
        {
            Debugger.Break();

            var isDaytime = GameSystems.TimeEvent.IsDaytime;

            foreach (var obj in sector.objects)
            {
                var flags = obj.GetFlags();
                var renderFlags = obj.GetUInt32(obj_f.render_flags);
                obj.SetUInt32(obj_f.render_flags, renderFlags & ~0x600_0000u);

                if (obj.type != ObjectType.scenery)
                {
                    continue;
                }

                var sceneryFlags = obj.GetSceneryFlags();
                var isNocturnal = sceneryFlags.HasFlag(SceneryFlag.NOCTURNAL);
                if (sceneryFlags.HasFlag(SceneryFlag.RESPAWNING) || !isNocturnal)
                {
                    continue;
                }

                // Toggle the object on or off based on current daytime status
                var off = flags.HasFlag(ObjectFlag.OFF);
                if (isDaytime)
                {
                    if (!off)
                    {
                        GameSystems.MapObject.SetFlags(obj, ObjectFlag.OFF);
                    }
                    else
                    {
                        return;
                    }
                }
                else if (off)
                {
                    GameSystems.MapObject.ClearFlags(obj, ObjectFlag.OFF);
                }
                else
                {
                    return;
                }

                var lightHandle = obj.GetInt32(obj_f.light_handle);
                if ((renderFlags & 0x80000000) == 0)
                {
                    if (lightHandle != 0)
                    {
                        // TODO FreeSectorLight(lightHandle); @ 0x100a84b0
                        throw new NotImplementedException();
                    }

                    obj.SetUInt32(obj_f.render_flags, renderFlags | 0x80000000);
                }
                else
                {
                    if (lightHandle != null)
                    {
                        // TODO 0x10106030
                        throw new NotImplementedException();
                    }
                }
            }
        }

        [TempleDllLocation(0x100a75e0)]
        public void UpdateDaylight()
        {
            IsNight = !GameSystems.TimeEvent.IsDaytime;

            var hour = GameSystems.TimeEvent.HourOfDay;
            var minute = GameSystems.TimeEvent.MinuteOfHour;
            var second = GameSystems.TimeEvent.SecondOfMinute;

            if (_lightColorLookupTable.HasValue)
            {
                var newGlobalLight = GlobalLight;
                newGlobalLight.Color = _lightColorLookupTable.Value.GetColor(hour, minute, second);
                GlobalLight = newGlobalLight;
            }

            if (_terrainTintLookupTable.HasValue)
            {
                GameSystems.Terrain.Tint = _terrainTintLookupTable.Value.GetColor(hour, minute, second);
            }
            else
            {
                GameSystems.Terrain.Tint = LinearColor.White;
            }
        }

        [TempleDllLocation(0x100a8430)]
        public void MoveObjectLight(GameObjectBody obj, LocAndOffsets loc)
        {
            var lightHandle = obj.GetInt32(obj_f.light_handle);
            if (lightHandle != 0)
            {
                // TODO: Free sector light
                throw new NotImplementedException();
            }
        }

        [TempleDllLocation(0x100a8470)]
        public void MoveObjectLightOffsets(GameObjectBody obj, float offsetX, float offsetY)
        {
            var lightHandle = obj.GetInt32(obj_f.light_handle);
            if (lightHandle != 0)
            {
                throw new NotImplementedException();
            }
        }
    }

    internal static class DaylightRulesParser
    {
        [TempleDllLocation(0x100a7040)]
        public static Dictionary<int, DaylightLookupTable> LoadLookupTables(Dictionary<int, string> daylightRules,
            int offset)
        {
            var mapIds = daylightRules.Keys
                .Select(key => key / 100)
                .Where(key => key != 0)
                .Distinct();

            var result = new Dictionary<int, DaylightLookupTable>();

            Span<bool> hoursDefined = stackalloc bool[24];
            Span<LinearColor> definedColor = stackalloc LinearColor[24];
            Span<float> colorComponents = stackalloc float[6];
            LinearColor definedDawnColor = default;
            LinearColor definedDuskColor = default;

            foreach (var mapId in mapIds)
            {
                var definedHoursCount = 0;
                for (var hour = 0; hour < 24; hour++)
                {
                    var lineId = 100 * mapId + offset + hour;

                    if (daylightRules.TryGetValue(lineId, out var line))
                    {
                        var componentCount = ParseColorComponents(line, colorComponents);

                        hoursDefined[hour] = true;
                        definedColor[hour] = new LinearColor(
                            colorComponents[0],
                            colorComponents[1],
                            colorComponents[2]
                        );

                        if (hour == 6)
                        {
                            if (componentCount == 6)
                            {
                                definedDawnColor = new LinearColor(
                                    colorComponents[3],
                                    colorComponents[4],
                                    colorComponents[5]
                                );
                            }
                            else
                            {
                                definedDawnColor = definedColor[hour];
                            }
                        }
                        else
                        {
                            if (hour == 18)
                            {
                                if (componentCount == 6)
                                {
                                    definedDuskColor = new LinearColor(
                                        colorComponents[3],
                                        colorComponents[4],
                                        colorComponents[5]
                                    );
                                }
                                else
                                {
                                    definedDuskColor = definedColor[hour];
                                }
                            }
                        }

                        ++definedHoursCount;
                    }
                    else
                    {
                        hoursDefined[hour] = false;
                        definedColor[hour] = LinearColor.White;
                    }
                }

                var mapLightColorTable = new LinearColor[24];
                LinearColor mapLightColorAtDawn = default;
                LinearColor mapLightColorAtDusk = default;

                Trace.Assert(definedHoursCount > 0);

                for (var hour = 0; hour < 24; hour++)
                {
                    if (hoursDefined[hour])
                    {
                        mapLightColorTable[hour] = definedColor[hour];
                        if (hour == 6)
                        {
                            mapLightColorAtDawn = definedDawnColor;
                        }
                        else if (hour == 18)
                        {
                            mapLightColorAtDusk = definedDuskColor;
                        }
                    }
                    else
                    {
                        // Interpolate between the prev and next defined hours
                        var nextHour = FindNextDefinedColor(hour, hoursDefined);
                        var prevHour = FindPreviousDefinedColor(hour, hoursDefined);

                        if (nextHour == prevHour)
                        {
                            // This means there's just a single color defined in the entire thing...
                            mapLightColorTable[hour] = definedColor[nextHour];
                            if (hour == 6)
                            {
                                mapLightColorAtDawn = definedColor[nextHour];
                            }
                            else if (hour == 18)
                            {
                                mapLightColorAtDusk = definedColor[nextHour];
                            }
                        }
                        else
                        {
                            float lerpFactor;
                            if (nextHour >= prevHour)
                            {
                                lerpFactor = (hour - prevHour) / (float) (nextHour - prevHour);
                            }
                            else if (nextHour <= hour)
                            {
                                lerpFactor = (hour - prevHour) / (float) (nextHour - prevHour + 24);
                            }
                            else
                            {
                                lerpFactor = (hour - (prevHour - 24)) / (float) (nextHour - (prevHour - 24));
                            }

                            LinearColor baseColor;
                            if (prevHour == 6)
                            {
                                baseColor = definedDawnColor;
                            }
                            else if (prevHour == 18)
                            {
                                baseColor = definedDuskColor;
                            }
                            else
                            {
                                baseColor = definedColor[prevHour];
                            }

                            var color = LinearColor.Lerp(baseColor, definedColor[nextHour], lerpFactor);
                            mapLightColorTable[hour] = color;
                            if (hour == 6)
                            {
                                mapLightColorAtDawn = color;
                            }
                            else if (hour == 18)
                            {
                                mapLightColorAtDusk = color;
                            }
                        }
                    }
                }

                result[mapId] = new DaylightLookupTable(mapLightColorTable, mapLightColorAtDawn, mapLightColorAtDusk);
            }

            return result;
        }

        private static int ParseColorComponents(string line, Span<float> components)
        {
            var componentIdx = 0;
            foreach (var b in line.Split(',')
                .Select(text => int.Parse(text, CultureInfo.InvariantCulture)))
            {
                if (componentIdx >= 6)
                {
                    break;
                }

                components[componentIdx++] = b / 255.0f;
            }

            for (var i = componentIdx; i < 6; i++)
            {
                components[i] = 0;
            }

            return componentIdx;
        }

        private static int FindNextDefinedColor(int afterHour, Span<bool> definedHours)
        {
            // the current hour is not defined in the table,
            // so search for the next defined entry for purposes of interpolation
            int i;
            for (i = afterHour; i < 24; ++i)
            {
                if (definedHours[i])
                    break;
            }

            if (i == 24)
            {
                // Wrap around and search from the start until the hour we're at
                for (i = 0; i < afterHour; ++i)
                {
                    if (definedHours[i])
                        break;
                }
            }

            return i;
        }

        private static int FindPreviousDefinedColor(int beforeHour, Span<bool> definedHours)
        {
            int j;
            for (j = beforeHour; j >= 0; j--)
            {
                if (definedHours[j])
                {
                    break;
                }
            }

            if (j < 0)
            {
                for (j = 23; j > beforeHour; --j)
                {
                    if (definedHours[j])
                        break;
                }
            }

            return j;
        }
    }
}