using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.TimeEvents;

namespace SpicyTemple.Core.Systems
{
    internal struct ObjFadeArgs
    {
        public int id;
        public int initialOpacity;
        public int goalOpacity;
        public int tickMs;
        public int tickQuantum;
        public int flags;
    }

    public class ObjFadeSystem : IGameSystem, IResetAwareSystem, ISaveGameAwareGameSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x10AA3240)] private int _serial;

        [TempleDllLocation(0x10AA3230)]
        private readonly Dictionary<int, ObjFadeArgs> _objFadeTable = new Dictionary<int, ObjFadeArgs>();

        [TempleDllLocation(0x1004c130)]
        public ObjFadeSystem()
        {
        }

        [TempleDllLocation(0x1004c170)]
        public void Dispose()
        {
            _objFadeTable.Clear();
        }

        [TempleDllLocation(0x1004c390)]
        public void FadeTo(GameObjectBody obj, int targetOpacity, int tickTimeMs, int tickOpacityQuantum,
            int callbackMode)
        {
            var cur = obj.GetInt32(obj_f.transparency);
            if (cur != targetOpacity)
            {
                // Remove previous timers for the same object
                GameSystems.TimeEvent.Remove(TimeEventType.ObjFade, existingEvent =>
                {
                    if (existingEvent.GetArg(1).handle != obj)
                    {
                        return false;
                    }

                    GameSystems.ObjFade.RemoveFromTable(existingEvent.GetArg(0).int32);
                    return true;
                });

                var newId = AppendToTable(tickOpacityQuantum, cur, targetOpacity, tickTimeMs, callbackMode);

                var evt = new TimeEvent(TimeEventType.ObjFade);
                evt.GetArg(0).int32 = newId;
                evt.GetArg(1).handle = obj;
                GameSystems.TimeEvent.Schedule(evt, tickTimeMs, out _);
            }

            if (callbackMode == 3)
            {
                GameSystems.Item.PoopInventory(obj, true);
            }
        }

        private int AppendToTable(int quantum, int initialOpacity, int goalOpacity, int tickTimeMs, int flags)
        {
            var result = _serial;

            var objFadeArgs = new ObjFadeArgs
            {
                id = _serial,
                initialOpacity = initialOpacity,
                goalOpacity = goalOpacity,
                tickQuantum = quantum,
                flags = flags,
                tickMs = tickTimeMs
            };

            _objFadeTable[objFadeArgs.id] = objFadeArgs;
            _serial++;

            return result;
        }

        private void RemoveFromTable(int id)
        {
            _objFadeTable.Remove(id);
        }

        [TempleDllLocation(0x1004c490)]
        public bool TimeEventExpired(TimeEvent evt)
        {
            var id = evt.arg1.int32;
            var handle = evt.arg2.handle;

            if (!_objFadeTable.TryGetValue(id, out var fadeArgs))
            {
                Logger.Error("ObjFadeSystem.TimeEventExpired: Unknown fade id {0}", id);
                return true;
            }

            var curOpacity = handle.GetInt32(obj_f.transparency);
            var goalOpacity = fadeArgs.goalOpacity;

            int newOpacity;
            if (fadeArgs.initialOpacity <= goalOpacity)
            {
                newOpacity = curOpacity + fadeArgs.tickQuantum;
                if (newOpacity >= goalOpacity)
                {
                    GameSystems.MapObject.SetTransparency(handle, goalOpacity);
                    return true;
                }
            }
            else
            {
                newOpacity = curOpacity - fadeArgs.tickQuantum;
                if (newOpacity <= goalOpacity)
                {
                    GameSystems.MapObject.SetTransparency(handle, goalOpacity);
                    return true;
                }
            }

            var newEvt = new TimeEvent(TimeEventType.ObjFade);
            newEvt.arg1.int32 = id;
            newEvt.arg2.handle = handle;

            GameSystems.TimeEvent.Schedule(newEvt, fadeArgs.tickMs, out _);
            GameSystems.MapObject.SetTransparency(handle, newOpacity);
            return true;
        }

        [TempleDllLocation(0x1004c190)]
        public void Reset()
        {
            _serial = 0;
            _objFadeTable.Clear();
        }

        [TempleDllLocation(0x1004c1c0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
            /*signed int result; // eax@3

            if ( SaveVisitedMaps((int)&objFadeIdxTable, a1) )
            {
                if ( tio_fwrite(&objFadeSerial, 4, 1, a1) )
                {
                    result = 1;
                }
                else
                {
                    Logger.Error("objfade.c: ERROR: could not save next_fade_id");
                    result = 0;
                }
            }
            else
            {
                Logger.Error("objfade.c: ERROR: could not save fade_table");
                result = 0;
            }
            return result;*/
        }

        [TempleDllLocation(0x1004c220)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
            /*
            if ( ReadIdxTable(&objFadeIdxTable, *(GameSystemSaveFile **)(a1 + 4)) )
            {
                if ( !tio_fread(&objFadeSerial, 4, 1, *(TioFile **)(a1 + 4)) )
                    Logger.Error("objfade.c: ERROR: could not read next_fade_id");
            }
            else
            {
                Logger.Error("objfade.c: ERROR: could not read fade_table");
            }
            */
        }
    }
}