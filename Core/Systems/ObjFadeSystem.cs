using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.TimeEvents;

namespace OpenTemple.Core.Systems;

internal struct ObjFadeArgs
{
    public int id;
    public int initialOpacity;
    public int goalOpacity;
    public int tickMs;
    public int tickQuantum;
    public FadeOutResult fadeOutResult;
}

/// <summary>
/// Defines if something else should happen once the opacity transition has completed.
/// </summary>
public enum FadeOutResult
{
    None = 0,

    /// <summary>
    /// Destroy the object being faded.
    /// </summary>
    Destroy = 1,

    /// <summary>
    /// Clears the AI Run Off flag and should be used if a NPC is faded out while running off.
    /// </summary>
    RunOff = 2,

    /// <summary>
    /// Drop the inventory immediately, i.e. for a fade out happening on death this is needed to make the
    /// inventory lootable since a faded out corpse is not clickable.
    /// This is sometimes used from animation files in the death animations.
    /// </summary>
    DropItemsAndDestroy = 3,

    /// <summary>
    /// Just set the OFF flag.
    /// </summary>
    SetOff = 4,
}

public class ObjFadeSystem : IGameSystem, IResetAwareSystem, ISaveGameAwareGameSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10AA3240)]
    private int _serial;

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
    public void FadeTo(GameObject obj, int targetOpacity, int tickTimeMs, int tickOpacityQuantum,
        FadeOutResult fadeResult)
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

            var newId = AppendToTable(tickOpacityQuantum, cur, targetOpacity, tickTimeMs, fadeResult);

            var evt = new TimeEvent(TimeEventType.ObjFade);
            evt.GetArg(0).int32 = newId;
            evt.GetArg(1).handle = obj;
            GameSystems.TimeEvent.Schedule(evt, tickTimeMs, out _);
        }

        if (fadeResult == FadeOutResult.DropItemsAndDestroy)
        {
            GameSystems.Item.PoopInventory(obj, true);
        }
    }

    private int AppendToTable(int quantum, int initialOpacity, int goalOpacity, int tickTimeMs,
        FadeOutResult fadeOutResult)
    {
        var result = _serial;

        var objFadeArgs = new ObjFadeArgs
        {
            id = _serial,
            initialOpacity = initialOpacity,
            goalOpacity = goalOpacity,
            tickQuantum = quantum,
            fadeOutResult = fadeOutResult,
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

    [TempleDllLocation(0x1004C570)]
    public bool TimeEventRemoved(TimeEvent evt)
    {
        var id = evt.arg1.int32;
        var handle = evt.arg2.handle;

        if (!_objFadeTable.TryGetValue(id, out var fadeArgs))
        {
            Logger.Error("ObjFadeSystem.TimeEventRemoved: Unknown fade id {0}", id);
            return true;
        }

        _objFadeTable.Remove(id);

        var curOpacity = handle.GetInt32(obj_f.transparency);
        var goalOpacity = fadeArgs.goalOpacity;
        if (curOpacity == goalOpacity)
        {
            ApplyFadeResult(fadeArgs.fadeOutResult, handle);
        }

        return true;
    }

    [TempleDllLocation(0x1004c290)]
    private static void ApplyFadeResult(FadeOutResult result, GameObject obj)
    {
        switch (result)
        {
            case FadeOutResult.Destroy:
            case FadeOutResult.DropItemsAndDestroy:
                obj.AiFlags &= AiFlag.RunningOff;
                GameSystems.Object.Destroy(obj);
                break;
            case FadeOutResult.RunOff:
                obj.AiFlags &= AiFlag.RunningOff;
                GameSystems.MapObject.SetFlags(obj, ObjectFlag.OFF);
                break;
            case FadeOutResult.SetOff:
                GameSystems.MapObject.SetFlags(obj, ObjectFlag.OFF);
                break;
            case FadeOutResult.None:
                return;
            default:
                throw new ArgumentOutOfRangeException();
        }
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
    public void SaveGame(SavedGameState savedGameState)
    {
        var savedFades = new Dictionary<int, SavedFadeSettings>(_objFadeTable.Count);
        foreach (var fadeArgs in _objFadeTable.Values)
        {
            savedFades[fadeArgs.id] = new SavedFadeSettings
            {
                Id = fadeArgs.id,
                InitialOpacity = fadeArgs.initialOpacity,
                GoalOpacity = fadeArgs.goalOpacity,
                MillisPerTick = fadeArgs.tickMs,
                OpacityPerTick = fadeArgs.tickQuantum,
                FadeOutResult = fadeArgs.fadeOutResult
            };
        }

        savedGameState.ObjFadeState = new SavedObjFadeState
        {
            NextObjectFadeId = _serial,
            ActiveFades = savedFades
        };
    }

    [TempleDllLocation(0x1004c220)]
    public void LoadGame(SavedGameState savedGameState)
    {
        var objFadeState = savedGameState.ObjFadeState;
        _serial = objFadeState.NextObjectFadeId;
        _objFadeTable.Clear();
        // Keep in mind that the actual fade is triggered by a saved time event
        foreach (var savedSettings in objFadeState.ActiveFades.Values)
        {
            _objFadeTable[savedSettings.Id] = new ObjFadeArgs
            {
                id = savedSettings.Id,
                initialOpacity = savedSettings.InitialOpacity,
                goalOpacity = savedSettings.GoalOpacity,
                tickMs = savedSettings.MillisPerTick,
                tickQuantum = savedSettings.OpacityPerTick,
                fadeOutResult = savedSettings.FadeOutResult
            };
        }
    }
}