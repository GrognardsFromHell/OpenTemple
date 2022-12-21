using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.Pathfinding;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Systems.Anim;

// Has to be 0x10 in size
public struct AnimParam
{
    private object? _value;

    public GameObject? obj
    {
        get => (GameObject?) _value;
        set => this._value = value;
    }

    public LocAndOffsets location
    {
        get => (LocAndOffsets) _value;
        set => this._value = value;
    }

    public int number
    {
        get => (int) _value;
        set => this._value = value;
    }

    public int spellId
    {
        get => (int) _value;
        set => this._value = value;
    }

    public float floatNum
    {
        // Isn't ToEE great!
        get => BitConverter.Int32BitsToSingle((int) _value);
        set => this._value = BitConverter.SingleToInt32Bits(value);
    }
}

[Flags]
public enum AnimPathFlag
{
    UNK_1 = 1,
    UNK_2 = 2,
    UNK_4 = 4,
    UNK_8 = 8,
    UNK_20 = 0x20,
}

public struct AnimPath
{
    public AnimPathFlag flags;
    public sbyte[] deltas; // xy delta pairs describing deltas for drawing a line in screenspace
    public int range;
    public CompassDirection fieldD0;
    public int fieldD4; // Current index
    public int deltaIdxMax;
    public int fieldDC;
    public int maxPathLength;
    public int fieldE4;
    public locXY objLoc;
    public locXY tgtLoc;

    public static AnimPath Empty => new()
    {
        flags = AnimPathFlag.UNK_1,
        deltas = new sbyte[200],
        range = 200
    };
}

public class AnimSlot
{
    public AnimSlotId id;
    public AnimSlotFlag flags;
    public int currentState;
    public int field_14; // Compared against currentGoal
    public GameTime nextTriggerTime;
    public GameObject animObj;
    public int currentGoal;
    public int field_2C;
    public List<AnimSlotGoalStackEntry> goals = new();
    public AnimSlotGoalStackEntry pCurrentGoal;
    public uint unk1; // field_1134
    public AnimPath animPath = AnimPath.Empty;
    public PathQueryResult path = new();
    public AnimParam param1; // Used as parameters for goal states
    public AnimParam param2; // Used as parameters for goal states
    public int stateFlagData;
    public uint[] unknown = new uint[5];
    public TimePoint gametimeSth;
    public int currentPing; // I.e. used in
    public int uniqueActionId; // ID assigned when triggered by a D20 action

    public bool IsActive => flags.HasFlag(AnimSlotFlag.ACTIVE);

    public bool IsStopProcessing => flags.HasFlag(AnimSlotFlag.STOP_PROCESSING);

    public bool IsStackFull => currentGoal >= 7;

    public bool IsStackEmpty => currentGoal < 0;

    public void Clear()
    {
        id.Clear();
        pCurrentGoal = null;
        animObj = null;
        flags = 0;
        currentGoal = -1;
        animPath.flags = 0;
    }

    public void ClearPath()
    {
        path.flags &= ~PathFlags.PF_COMPLETE;
        GameSystems.Raycast.GoalDestinationsRemove(path.mover);
    }

    public override string ToString()
    {
        var activeInactive = IsActive ? "ACTIVE" : "INACTIVE";
        return $"Slot({id};{animObj};{activeInactive};{pCurrentGoal?.goalType})";
    }
}