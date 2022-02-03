using System;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.MapSector;
using OpenTemple.Core.Systems.ObjScript;

namespace OpenTemple.Core.Systems
{
    public class TileScriptSystem : IGameSystem
    {
        private const bool IsEditor = false;

        public void Dispose()
        {
        }

        private struct TileScript
        {
            public locXY Location;
            public ObjectScript Script;
        }

        [TempleDllLocation(0x10053b20)]
        public bool TriggerTileScript(locXY tileLoc, GameObject obj)
        {
            if (GetTileScript(tileLoc, out var tileScript))
            {
                var invocation = new ObjScriptInvocation();
                invocation.script = tileScript.Script;
                invocation.triggerer = obj;
                invocation.eventId = ObjScriptEvent.Use;
                GameSystems.Script.Invoke(ref invocation);

                if (invocation.script != tileScript.Script)
                {
                    SetTileScript(in tileScript);
                }

                return true;
            }

            return false;
        }

        private bool GetTileScript(locXY tileLoc, out TileScript tileScript)
        {
            using var lockedSector = new LockedMapSector(new SectorLoc(tileLoc));
            var sector = lockedSector.Sector;
            if (sector == null)
            {
                tileScript = default;
                return false;
            }
            var tileIndex = sector.GetTileOffset(tileLoc);

            foreach (var scriptInSector in sector.tileScripts)
            {
                if (scriptInSector.tileIndex == tileIndex)
                {
                    tileScript.Location = tileLoc;
                    tileScript.Script.unk1 = scriptInSector.scriptUnk1;
                    tileScript.Script.counters = scriptInSector.scriptCounters;
                    tileScript.Script.scriptId = scriptInSector.scriptId;
                    return true;
                }
                else if (scriptInSector.tileIndex > tileIndex)
                {
                    break; // Tiles are sorted in ascending order
                }
            }

            tileScript = default;
            return false;
        }

        private void SetTileScript(in TileScript tileScript)
        {
            using var lockedSector = new LockedMapSector(new SectorLoc(tileScript.Location));
            var sector = lockedSector.Sector;
            var tileIndex = sector.GetTileOffset(tileScript.Location);

            if (!IsEditor || tileScript.Script.scriptId != 0)
                AddOrUpdateSectorTilescript(sector, tileIndex, tileScript.Script);
            else
                RemoveSectorTilescript(sector, tileIndex);
        }

        [TempleDllLocation(0x10105400)]
        public void AddOrUpdateSectorTilescript(Sector sector, int tileIndex, ObjectScript script)
        {
            // If a tile-script exists for the tile, update it accordingly
            for (var i = 0; i < sector.tileScripts.Length; i++)
            {
                ref var tileScript = ref sector.tileScripts[i];
                if (tileScript.tileIndex == tileIndex)
                {
                    tileScript.dirty = true; // Dirty flag most likely
                    tileScript.scriptUnk1 = script.unk1;
                    tileScript.scriptCounters = script.counters;
                    tileScript.scriptId = script.scriptId;
                    sector.tileScriptsDirty = true;
                    return;
                }

                if (tileScript.tileIndex > tileIndex)
                {
                    break; // Entries are sorted in ascending order
                }
            }

            Array.Resize(ref sector.tileScripts, sector.tileScripts.Length + 1);
            sector.tileScripts[^1] = new SectorTileScript
            {
                dirty = true,
                tileIndex = tileIndex,
                scriptUnk1 = script.unk1,
                scriptCounters = script.counters,
                scriptId = script.scriptId
            };
            // Ensure it is still sorted in ascending order
            Array.Sort(sector.tileScripts, SectorTileScript.TileIndexComparer);
            sector.tileScriptsDirty = true;
        }

        [TempleDllLocation(0x101054b0)]
        private void RemoveSectorTilescript(Sector sector, int tileIndex)
        {
            // Determine how many we need to remove. In normal conditions this should be 0 or 1.
            int removeCount = sector.tileScripts.Count(i => i.tileIndex == tileIndex);
            if (removeCount == 0)
            {
                return;
            }

            // Create a new array without the tile, this will maintain the sort order as well
            var outIdx = 0;
            var newScripts = new SectorTileScript[sector.tileScripts.Length - removeCount];
            foreach (var tileScript in sector.tileScripts)
            {
                if (tileScript.tileIndex != tileIndex)
                {
                    newScripts[outIdx++] = tileScript;
                }
            }

            sector.tileScripts = newScripts;
            sector.tileScriptsDirty = true;
        }

        public void TriggerSectorScript(SectorLoc loc, GameObject obj)
        {
            if (GetSectorScript(loc, out var script))
            {
                // Save for change detection
                var invocation = new ObjScriptInvocation();
                invocation.script = script;
                invocation.eventId = ObjScriptEvent.Use;
                invocation.triggerer = obj;
                GameSystems.Script.Invoke(ref invocation);

                if (invocation.script != script)
                {
                    SetSectorScript(loc, in invocation.script);
                }
            }
        }

        [TempleDllLocation(0x100538e0)]
        public bool GetSectorScript(SectorLoc sectorLoc, out ObjectScript script)
        {
            using var lockedSector = new LockedMapSector(sectorLoc);
            var sectorScript = lockedSector.Sector.sectorScript;
            script.unk1 = sectorScript.data1;
            script.counters = sectorScript.data2;
            script.scriptId = sectorScript.data3;
            return script.scriptId != 0;
        }

        [TempleDllLocation(0x10053930)]
        public void SetSectorScript(SectorLoc sectorLoc, in ObjectScript script)
        {
            using var lockedSector = new LockedMapSector(sectorLoc);
            ref var sectorScript = ref lockedSector.Sector.sectorScript;
            sectorScript.data1 = script.unk1;
            sectorScript.data2 = script.counters;
            sectorScript.data3 = script.scriptId;

            sectorScript.dirty = true;
        }
    }
}