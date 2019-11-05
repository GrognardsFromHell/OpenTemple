using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualBasic.ApplicationServices;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class LightSchemeSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x102BED40)]
        private int _defaultScheme = 0;

        [TempleDllLocation(0x102BED3C)]
        private int _currentScheme = -1;

        [TempleDllLocation(0x10AA9514)]
        private SchemeEntry[] _schemesByHour = new SchemeEntry[24];

        private Dictionary<int, string> _schemeIndex = new Dictionary<int, string>();

        [TempleDllLocation(0x10AA9518)]
        private int _currentHourOfDay;

        [TempleDllLocation(0x10AA951C)]
        private bool _hourOfDayUpdating;

        [TempleDllLocation(0x1006ef30)]
        public LightSchemeSystem()
        {
        }

        [TempleDllLocation(0x1006f0c0)]
        public bool IsUpdating => _hourOfDayUpdating;

        [TempleDllLocation(0x1006ef80)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x1006f440)]
        public void LoadModule()
        {
            _schemeIndex = Tig.FS.ReadMesFile("rules/Lighting Schemes.mes");
            SetCurrentScheme(0, 12);
        }

        [TempleDllLocation(0x1006ef50)]
        public void UnloadModule()
        {
            _schemeIndex.Clear();
            _currentScheme = -1;
        }

        [TempleDllLocation(0x1006f430)]
        public void Reset()
        {
            SetCurrentScheme(0, 12);
        }

        [TempleDllLocation(0x1006ef90)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006f470)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1006f000)]
        public int GetCurrentScheme() => _currentScheme;

        [TempleDllLocation(0x1006f350)]
        public bool SetCurrentScheme(int lightSchemeId, int hourOfDay)
        {
            Trace.Assert(_schemeIndex != null);

            if (lightSchemeId == 0)
            {
                lightSchemeId = _defaultScheme;
            }

            if (_currentScheme == lightSchemeId)
            {
                return SetHourOfDay(hourOfDay);
            }

            if (lightSchemeId == 0)
            {
                return false; // The first entry is invalid in the .mes file
            }

            if (!_schemeIndex.TryGetValue(lightSchemeId, out var schemeFilename))
            {
                return false;
            }

            var schemePath = $"rules/{schemeFilename}.mes";

            LoadLightScheme(schemePath);

            _currentScheme = lightSchemeId;
            return SetHourOfDay(hourOfDay);
        }

        private void LoadLightScheme(string schemePath)
        {
            var schemeDescription = Tig.FS.ReadMesFile(schemePath);

            int lastSpecified = 0;
            for (var hourOfDay = 0; hourOfDay < 24; hourOfDay++)
            {
                var schemeLine = schemeDescription[hourOfDay];
                if (schemeLine.Length > 0) {
                    var parts = schemeLine.Split(new []{' ', ','}, StringSplitOptions.RemoveEmptyEntries);

                    // First is RGB for outdoors
                    var ro = byte.Parse(parts[0]);
                    var go = byte.Parse(parts[1]);
                    var bo = byte.Parse(parts[2]);
                    // Next is RGB for indoors
                    var ri = byte.Parse(parts[3]);
                    var gi = byte.Parse(parts[4]);
                    var bi = byte.Parse(parts[5]);

                    ref var scheme = ref _schemesByHour[hourOfDay];
                    scheme.outdoorRed = ro;
                    scheme.outdoorGreen = go;
                    scheme.outdoorBlue = bo;
                    scheme.indoorRed = ri;
                    scheme.indoorGreen = gi;
                    scheme.indoorBlue = bi;

                    // Interpolate previous entries as needed
                    int hourSpan = hourOfDay - lastSpecified;
                    if (hourSpan > 1)
                    {
                        ref var lastScheme = ref _schemesByHour[lastSpecified];

                        int roStep = (ro - lastScheme.outdoorRed) / hourSpan;
                        int goStep = (go - lastScheme.outdoorGreen) / hourSpan;
                        int boStep = (bo - lastScheme.outdoorBlue) / hourSpan;
                        int riStep = (ri - lastScheme.indoorRed) / hourSpan;
                        int giStep = (gi - lastScheme.indoorGreen) / hourSpan;
                        int biStep = (bi - lastScheme.indoorBlue) / hourSpan;

                        for (int i = 1; i < hourSpan; i++)
                        {
                            ref var interpolatedScheme = ref _schemesByHour[lastSpecified + i];
                            interpolatedScheme.outdoorRed = (byte) (lastScheme.outdoorRed + roStep * i);
                            interpolatedScheme.outdoorGreen = (byte) (lastScheme.outdoorGreen + goStep * i);
                            interpolatedScheme.outdoorBlue = (byte) (lastScheme.outdoorBlue + boStep * i);
                            interpolatedScheme.indoorRed = (byte) (lastScheme.indoorRed + riStep * i);
                            interpolatedScheme.indoorGreen = (byte) (lastScheme.indoorGreen + giStep * i);
                            interpolatedScheme.indoorBlue = (byte) (lastScheme.indoorBlue + biStep * i);

                        }
                    }

                    lastSpecified = hourOfDay;
                }

            }
        }

        public int GetHourOfDay() => _currentHourOfDay;

        [TempleDllLocation(0x1006f010)]
        public bool SetHourOfDay(int hourOfDay)
        {
            if (hourOfDay < 0 || hourOfDay >= 24)
            {
                return false;
            }

            _currentHourOfDay = hourOfDay;

            var schemeEntry = _schemesByHour[hourOfDay];

            _hourOfDayUpdating = true;
            GameSystems.Light.SetColors(schemeEntry.Indoor, schemeEntry.Outdoor);
            _hourOfDayUpdating = false;

            return true;
        }

        [TempleDllLocation(0x1006efd0)]
        public void SetDefaultScheme(int scheme)
        {
            if (scheme >= 0)
            {
                return;
            }

            _defaultScheme = scheme;
        }

        [TempleDllLocation(0x1006eff0)]
        public int GetDefaultScheme() => _defaultScheme;

        private struct SchemeEntry
        {
            public byte indoorRed;
            public byte indoorGreen;
            public byte indoorBlue;

            public PackedLinearColorA Indoor => new PackedLinearColorA(indoorRed, indoorGreen, indoorBlue, 255);

            public byte outdoorRed;
            public byte outdoorGreen;
            public byte outdoorBlue;

            public PackedLinearColorA Outdoor => new PackedLinearColorA(outdoorRed, outdoorGreen, outdoorBlue, 255);

        }
    }
}