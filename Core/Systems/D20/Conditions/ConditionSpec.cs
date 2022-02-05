using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Particles.Instances;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20.Conditions;

public enum UniquenessType
{
    NotUnique,
    Unique,

    /// <summary>
    /// Multiple instances can be attached, but only with different arg1.
    /// </summary>
    UniqueArg1
}

/// <summary>
/// This defines a condition and what it does, while CondNode represents an instance of this or another condition.
/// Called CondNode in TP.
/// </summary>
public class ConditionSpec
{
    public string condName { get; }

    public ImmutableArray<Type> DataTypes { get; }

    public int numArgs => DataTypes.Length;

    public ImmutableArray<SubDispatcherSpec> subDispDefs;

    public UniquenessType Uniqueness { get; }

    public bool IsExtension { get; }

    public ConditionSpec(string condName,
        ImmutableArray<Type> dataTypes,
        UniquenessType uniqueness,
        ImmutableArray<SubDispatcherSpec> subDispDefs,
        bool extension)
    {
        this.condName = condName;
        DataTypes = dataTypes;
        Uniqueness = uniqueness;
        this.subDispDefs = subDispDefs;
        IsExtension = extension;
    }

    public static Builder Create(string name, int numArgs = 0) => new(name, numArgs, false);

    public static Builder Extend(ConditionSpec baseCondition) => new(baseCondition.condName, baseCondition.numArgs, true);

    public class Builder
    {
        private readonly bool _extending;

        private readonly string _name;

        private readonly Type[] _dataTypes;

        private readonly List<SubDispatcherSpec> _subDisps = new();

        private UniquenessType _uniqueness = UniquenessType.NotUnique;

        internal Builder(string name, int numArgs, bool extending)
        {
            _extending = extending;
            _name = name;
            _dataTypes = new Type[numArgs];
            Array.Fill(_dataTypes, typeof(int));
        }

        /// <summary>
        /// Adds a handler that'll handle the given dispatcher type for every key.
        /// </summary>
        public Builder AddHandler(DispatcherType type, SubDispatcherCallback handler)
        {
            _subDisps.Add(new SubDispatcherSpec(type, D20DispatcherKey.NONE, handler));
            return this;
        }

        /// <summary>
        /// Adds a handler that'll handle the given dispatcher type only for the given key.
        /// </summary>
        public Builder AddHandler(DispatcherType type, D20DispatcherKey key, SubDispatcherCallback handler)
        {
            _subDisps.Add(new SubDispatcherSpec(type, key, handler));
            return this;
        }

        /// <summary>
        /// Make this condition unique. It can only be added once to a critter.
        /// </summary>
        public Builder SetUnique()
        {
            if (_extending)
            {
                throw new InvalidOperationException("A Condition extension cannot change whether a condition is unique.");
            }
            Trace.Assert(_uniqueness == UniquenessType.NotUnique);
            _uniqueness = UniquenessType.Unique;
            return AddHandler(
                DispatcherType.ConditionAddPre,
                PreventsItself
            );
        }

        /// <summary>
        /// Make conditions of this type with the same arg1 unique.
        /// </summary>
        public Builder SetUniqueWithKeyArg1()
        {
            if (_extending)
            {
                throw new InvalidOperationException("A Condition extension cannot change whether a condition is unique.");
            }
            Trace.Assert(_uniqueness == UniquenessType.NotUnique);
            _uniqueness = UniquenessType.UniqueArg1;
            return AddHandler(
                DispatcherType.ConditionAddPre,
                PreventItselfWithSameArg1
            );
        }

        private static void PreventsItself(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == evt.subDispNode.condNode.condStruct)
            {
                dispIo.outputFlag = false;
            }
        }

        private static void PreventItselfWithSameArg1(in DispatcherCallbackArgs evt)
        {
            var condArg1 = evt.GetConditionArg1();
            var dispIo = evt.GetDispIoCondStruct();
            if (dispIo.condStruct == evt.subDispNode.condNode.condStruct && dispIo.arg1 == condArg1)
            {
                dispIo.outputFlag = false;
            }
        }

        public ConditionSpec Build()
        {
            return new ConditionSpec(_name, _dataTypes.ToImmutableArray(), _uniqueness, _subDisps.ToImmutableArray(), _extending);
        }
    }
}

public static class ConditionSpecBuilderExtensions
{
    public static ConditionSpec.Builder AddHandler<T>(this ConditionSpec.Builder builder, DispatcherType type,
        SubDispatcherCallback<T> handler, T data)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data);
        return builder.AddHandler(type, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddHandler<T, U>(this ConditionSpec.Builder builder, DispatcherType type,
        SubDispatcherCallback<T, U> handler, T data1, U data2)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data1, data2);
        return builder.AddHandler(type, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddHandler<T>(this ConditionSpec.Builder builder, DispatcherType type,
        D20DispatcherKey key, SubDispatcherCallback<T> handler, T data)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data);
        return builder.AddHandler(type, key, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddHandler<T, U>(this ConditionSpec.Builder builder, DispatcherType type,
        D20DispatcherKey key, SubDispatcherCallback<T, U> handler, T data1, U data2)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data1, data2);
        return builder.AddHandler(type, key, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddSkillLevelHandler(this ConditionSpec.Builder builder, SkillId skill,
        SubDispatcherCallback callback)
    {
        builder.AddHandler(DispatcherType.SkillLevel, D20DispatcherKey.SKILL_APPRAISE + (int) skill, callback);
        return builder;
    }

    public static ConditionSpec.Builder AddSkillLevelHandler<T>(this ConditionSpec.Builder builder, SkillId skill,
        SubDispatcherCallback<T> handler, T data)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data);
        return AddSkillLevelHandler(builder, skill, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddSkillLevelHandler<T, U>(this ConditionSpec.Builder builder,
        SkillId skill, SubDispatcherCallback<T, U> handler, T data1, U data2)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data1, data2);
        return AddSkillLevelHandler(builder, skill, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddQueryHandler(this ConditionSpec.Builder builder, D20DispatcherKey query,
        SubDispatcherCallback callback)
    {
        builder.AddHandler(DispatcherType.D20Query, query, callback);
        return builder;
    }

    public static ConditionSpec.Builder AddQueryHandler(this ConditionSpec.Builder builder, string query,
        SubDispatcherCallback callback)
    {
        builder.AddHandler(DispatcherType.PythonQuery, (D20DispatcherKey) ElfHash.Hash(query), callback);
        return builder;
    }

    public static ConditionSpec.Builder AddQueryHandler<T>(this ConditionSpec.Builder builder,
        D20DispatcherKey query, SubDispatcherCallback<T> handler, T data)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data);
        return AddQueryHandler(builder, query, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddQueryHandler<T, U>(this ConditionSpec.Builder builder,
        D20DispatcherKey query, SubDispatcherCallback<T, U> handler, T data1, U data2)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data1, data2);
        return AddQueryHandler(builder, query, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddSignalHandler(this ConditionSpec.Builder builder,
        D20DispatcherKey signal, SubDispatcherCallback callback)
    {
        builder.AddHandler(DispatcherType.D20Signal, signal, callback);
        return builder;
    }

    public static ConditionSpec.Builder AddSignalHandler(this ConditionSpec.Builder builder,
        string signal, SubDispatcherCallback callback)
    {
        builder.AddHandler(DispatcherType.PythonSignal, (D20DispatcherKey) ElfHash.Hash(signal), callback);
        return builder;
    }

    public static ConditionSpec.Builder AddSignalHandler<T>(this ConditionSpec.Builder builder,
        D20DispatcherKey signal, SubDispatcherCallback<T> handler, T data)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data);
        return AddSignalHandler(builder, signal, HandlerWithArgs);
    }

    public static ConditionSpec.Builder AddSignalHandler<T, U>(this ConditionSpec.Builder builder,
        D20DispatcherKey signal, SubDispatcherCallback<T, U> handler, T data1, U data2)
    {
        void HandlerWithArgs(in DispatcherCallbackArgs args) => handler(in args, data1, data2);
        return AddSignalHandler(builder, signal, HandlerWithArgs);
    }

    public static ConditionSpec.Builder Prevents(this ConditionSpec.Builder builder, ConditionSpec otherCondition)
    {
        return builder.AddHandler(
            DispatcherType.ConditionAddPre,
            CommonConditionCallbacks.CondPrevent,
            otherCondition
        );
    }

    public static ConditionSpec.Builder Prevents(this ConditionSpec.Builder builder, Predicate<ConditionSpec> conditionPredicate)
    {
        void Callback(in DispatcherCallbackArgs evt)
        {
            var dispIo = evt.GetDispIoCondStruct();
            if (conditionPredicate(dispIo.condStruct))
            {
                dispIo.outputFlag = false;
            }
        }

        return builder.AddHandler(
            DispatcherType.ConditionAddPre,
            Callback
        );
    }

    public static ConditionSpec.Builder PreventsWithSameArg1(this ConditionSpec.Builder builder,
        ConditionSpec otherCondition)
    {
        return builder.AddHandler(
            DispatcherType.ConditionAddPre,
            CommonConditionCallbacks.CondPrevent,
            otherCondition
        );
    }

    public static ConditionSpec.Builder RemovedBy(this ConditionSpec.Builder builder, ConditionSpec otherCondition)
    {
        // Uses ConditionAddPre,  and CondOverrideBy + the struct as data1
        return builder.AddHandler(
            DispatcherType.ConditionAddPre,
            CommonConditionCallbacks.CondOverrideBy,
            otherCondition
        );
    }

    // Sets the result for the given query to the given constant boolean
    public static ConditionSpec.Builder SetQueryResult(this ConditionSpec.Builder builder, D20DispatcherKey query,
        bool result)
    {
        if (result)
        {
            return builder.AddHandler(
                DispatcherType.D20Query,
                query,
                CommonConditionCallbacks.QuerySetReturnVal1
            );
        }
        else
        {
            return builder.AddHandler(
                DispatcherType.D20Query,
                query,
                CommonConditionCallbacks.QuerySetReturnVal0
            );
        }
    }

    // Removes the condition when the given signal is received
    public static ConditionSpec.Builder RemoveOnSignal(this ConditionSpec.Builder builder, D20DispatcherKey signal)
    {
        builder.AddSignalHandler(signal, (in DispatcherCallbackArgs evt) => evt.RemoveThisCondition());
        return builder;
    }

    public static ConditionSpec.Builder AddUniqueTooltip(this ConditionSpec.Builder builder, int combatMesLine)
    {
        builder.AddHandler(DispatcherType.Tooltip, (in DispatcherCallbackArgs evt) =>
        {
            var dispIo = evt.GetDispIoTooltip();
            var text = GameSystems.D20.Combat.GetCombatMesLine(combatMesLine);
            dispIo.AppendUnique(text);
        });
        return builder;
    }

    public static ConditionSpec.Builder SupportHasConditionQuery(this ConditionSpec.Builder builder)
    {
        return builder.AddQueryHandler(D20DispatcherKey.QUE_Critter_Has_Condition, (in DispatcherCallbackArgs evt) =>
        {
            var dispIo = evt.GetDispIoD20Query();
            var queryForCond = (ConditionSpec) dispIo.obj;
            if (queryForCond == evt.subDispNode.condNode.condStruct)
            {
                dispIo.return_val = 1;
            }
        });
    }

}

// Used to mark callbacks with the dispatcher types they're being used for,
// makes backtracing easier
public class DispTypesAttribute : Attribute
{
    public DispatcherType[] Types { get; }

    public DispTypesAttribute(params DispatcherType[] types)
    {
        Types = types;
    }
}

public readonly ref struct DispatcherCallbackArgs
{
    public readonly SubDispatcherAttachment subDispNode; // Rename to 'attachment'
    public readonly GameObject objHndCaller; // Rename to obj
    public readonly DispatcherType dispType;
    public readonly D20DispatcherKey dispKey;
    public readonly object dispIO; // Rename to dispIo

    public DispatcherCallbackArgs(SubDispatcherAttachment attachment, GameObject obj, DispatcherType dispType,
        D20DispatcherKey dispKey, object dispIo)
    {
        subDispNode = attachment;
        // This can actually be null for items
        objHndCaller = obj;
        this.dispType = dispType;
        this.dispKey = dispKey;
        dispIO = dispIo;
    }

    public DispatcherCallbackArgs WithoutIO => new(
        subDispNode,
        objHndCaller,
        dispType,
        dispKey,
        null
    );
}

[DontUseForAutoTranslation]
public static class DispatcherCallbackArgsExtensions
{
    #region DispIO Accessors

    // Checks and retrieves disp io of type 01
    [TempleDllLocation(0x1004d700)]
    public static DispIoCondStruct GetDispIoCondStruct(in this DispatcherCallbackArgs args)
    {
        return (DispIoCondStruct) args.dispIO;
    }

    // Checks and retrieves disp io of type 02
    [TempleDllLocation(0x1004d720)]
    public static DispIoBonusList GetDispIoBonusList(in this DispatcherCallbackArgs args)
    {
        return (DispIoBonusList) args.dispIO;
    }

    // Checks and retrieves disp io of type 03
    [TempleDllLocation(0x1004d740)]
    public static DispIoSavingThrow GetDispIoSavingThrow(in this DispatcherCallbackArgs args)
    {
        return (DispIoSavingThrow) args.dispIO;
    }

    // Checks and retrieves disp io of type 04
    [TempleDllLocation(0x1004d760)]
    public static DispIoDamage GetDispIoDamage(in this DispatcherCallbackArgs args)
    {
        return (DispIoDamage) args.dispIO;
    }

    // Checks and retrieves disp io of type 05
    [TempleDllLocation(0x1004d780)]
    public static DispIoAttackBonus GetDispIoAttackBonus(in this DispatcherCallbackArgs args)
    {
        return (DispIoAttackBonus) args.dispIO;
    }

    // Checks and retrieves disp io of type 06
    [TempleDllLocation(0x1004d7a0)]
    public static DispIoD20Signal GetDispIoD20Signal(in this DispatcherCallbackArgs args)
    {
        return (DispIoD20Signal) args.dispIO;
    }

    // Checks and retrieves disp io of type 07
    [TempleDllLocation(0x1004d7c0)]
    public static DispIoD20Query GetDispIoD20Query(in this DispatcherCallbackArgs args)
    {
        return (DispIoD20Query) args.dispIO;
    }

    // Checks and retrieves disp io of type 08
    [TempleDllLocation(0x1004d7e0)]
    public static DispIOTurnBasedStatus GetDispIOTurnBasedStatus(in this DispatcherCallbackArgs args)
    {
        return (DispIOTurnBasedStatus) args.dispIO;
    }

    // Checks and retrieves disp io of type 09
    [TempleDllLocation(0x1004d800)]
    public static DispIoTooltip GetDispIoTooltip(in this DispatcherCallbackArgs args)
    {
        return (DispIoTooltip) args.dispIO;
    }

    // Checks and retrieves disp io of type 10
    [TempleDllLocation(0x1004d820)]
    public static DispIoObjBonus GetDispIoObjBonus(in this DispatcherCallbackArgs args)
    {
        return (DispIoObjBonus) args.dispIO;
    }

    // Checks and retrieves disp io of type 11
    [TempleDllLocation(0x1004d840)]
    public static DispIoDispelCheck GetDispIoDispelCheck(in this DispatcherCallbackArgs args)
    {
        return (DispIoDispelCheck) args.dispIO;
    }

    // Checks and retrieves disp io of type 12
    [TempleDllLocation(0x1004d860)]
    public static DispIoD20ActionTurnBased GetDispIoD20ActionTurnBased(in this DispatcherCallbackArgs args)
    {
        return (DispIoD20ActionTurnBased) args.dispIO;
    }

    // Checks and retrieves disp io of type 13
    [TempleDllLocation(0x1004d880)]
    public static DispIoMoveSpeed GetDispIoMoveSpeed(in this DispatcherCallbackArgs args)
    {
        return (DispIoMoveSpeed) args.dispIO;
    }

    // Checks and retrieves disp io of type 14
    [TempleDllLocation(0x1004d8a0)]
    public static DispIoBonusAndSpellEntry GetDispIOBonusListAndSpellEntry(in this DispatcherCallbackArgs args)
    {
        return (DispIoBonusAndSpellEntry) args.dispIO;
    }

    // Checks and retrieves disp io of type 15
    [TempleDllLocation(0x1004d8c0)]
    public static DispIoReflexThrow GetDispIoReflexThrow(in this DispatcherCallbackArgs args)
    {
        return (DispIoReflexThrow) args.dispIO;
    }

    // Checks and retrieves disp io of type 17
    [TempleDllLocation(0x1004d8e0)]
    public static DispIoObjEvent GetDispIoObjEvent(in this DispatcherCallbackArgs args)
    {
        return (DispIoObjEvent) args.dispIO;
    }

    // Checks and retrieves disp io of type 19
    [TempleDllLocation(0x1004d920)]
    public static DispIoAbilityLoss GetDispIoAbilityLoss(in this DispatcherCallbackArgs args)
    {
        return (DispIoAbilityLoss) args.dispIO;
    }

    // Checks and retrieves disp io of type 20
    [TempleDllLocation(0x1004d940)]
    public static DispIoAttackDice GetDispIoAttackDice(in this DispatcherCallbackArgs args)
    {
        return (DispIoAttackDice) args.dispIO;
    }

    // Checks and retrieves disp io of type 21
    [TempleDllLocation(0x1004d900)]
    public static DispIoTypeImmunityTrigger GetDispIoTypeImmunityTrigger(in this DispatcherCallbackArgs args)
    {
        return (DispIoTypeImmunityTrigger) args.dispIO;
    }

    // Checks and retrieves disp io of type 23
    [TempleDllLocation(0x1004d960)]
    public static DispIoImmunity GetDispIoImmunity(in this DispatcherCallbackArgs args)
    {
        return (DispIoImmunity) args.dispIO;
    }

    // Checks and retrieves disp io of type 24
    [TempleDllLocation(0x1004d980)]
    public static DispIoEffectTooltip GetDispIoEffectTooltip(in this DispatcherCallbackArgs args)
    {
        return (DispIoEffectTooltip) args.dispIO;
    }

    // Custom TemplePlus DispIO's
    public static EvtObjSpellCaster GetEvtObjSpellCaster(in this DispatcherCallbackArgs args)
    {
        return (EvtObjSpellCaster) args.dispIO;
    }

    public static EvtObjActionCost GetEvtObjActionCost(in this DispatcherCallbackArgs args)
    {
        return (EvtObjActionCost) args.dispIO;
    }

    public static EvtObjSpellTargetBonus GetEvtObjSpellTargetBonus(in this DispatcherCallbackArgs args)
    {
        return (EvtObjSpellTargetBonus) args.dispIO;
    }
    public static EvtObjSpecialAttack GetEvtObjSpecialAttack(in this DispatcherCallbackArgs args)
    {
        return (EvtObjSpecialAttack) args.dispIO;
    }

    public static EvtObjRangeIncrementBonus GetEvtObjRangeIncrementBonus(in this DispatcherCallbackArgs args)
    {
        return (EvtObjRangeIncrementBonus) args.dispIO;
    }

    public static EvtObjMetaMagic GetEvtObjMetaMagic(in this DispatcherCallbackArgs args)
    {
        return (EvtObjMetaMagic) args.dispIO;
    }

    public static EvtObjDealingSpellDamage GetEvtObjDealingSpellDamage(in this DispatcherCallbackArgs args)
    {
        return (EvtObjDealingSpellDamage) args.dispIO;
    }

    #endregion

    #region Condition Argument Getters and Setters

    [TempleDllLocation(0x100e1ab0)]
    public static int GetConditionArg(in this DispatcherCallbackArgs args, int index)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs > index);
        return (int) args.subDispNode.condNode.args[index];
    }

    [TempleDllLocation(0x100e1ab0)]
    public static int GetConditionArg1(in this DispatcherCallbackArgs args)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 1);
        return (int) args.subDispNode.condNode.args[0];
    }

    [TempleDllLocation(0x100e1ab0)]
    public static int GetConditionArg2(in this DispatcherCallbackArgs args)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 2);
        return (int) args.subDispNode.condNode.args[1];
    }

    [TempleDllLocation(0x100e1ab0)]
    public static int GetConditionArg3(in this DispatcherCallbackArgs args)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 3);
        return (int) args.subDispNode.condNode.args[2];
    }

    [TempleDllLocation(0x100e1ab0)]
    public static int GetConditionArg4(in this DispatcherCallbackArgs args)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 4);
        return (int) args.subDispNode.condNode.args[3];
    }

    [TempleDllLocation(0x100e1ab0)]
    public static GameObject GetConditionObjArg(in this DispatcherCallbackArgs args, int argIndex)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        // Special case translation for 0
        var value = args.subDispNode.condNode.args[argIndex];
        if (value == null || value.Equals(0))
        {
            return null;
        }
        return (GameObject) value;
    }

    public static Dice GetConditionDiceArg(in this DispatcherCallbackArgs args, int argIndex)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs > argIndex);
        return (Dice) args.subDispNode.condNode.args[argIndex];
    }

    public static ObjectId GetConditionObjectIdArg(in this DispatcherCallbackArgs args, int argIndex)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        return (ObjectId) args.subDispNode.condNode.args[argIndex];
    }

    public static void SetConditionObjectIdArg(in this DispatcherCallbackArgs args, int argIndex, ObjectId objectId)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        throw new NotImplementedException();
    }

    public static string GetConditionStringArg(in this DispatcherCallbackArgs args, int argIndex)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        throw new NotImplementedException();
    }

    public static void SetConditionStringArg(in this DispatcherCallbackArgs args, int argIndex, string str)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        throw new NotImplementedException();
    }

    public static void SetConditionObjArg(in this DispatcherCallbackArgs args, int argIndex, GameObject obj)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs + 1 > argIndex);
        args.subDispNode.condNode.args[argIndex] = obj;
    }

    public static PartSys GetConditionPartSysArg(in this DispatcherCallbackArgs args, int index)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs > index);
        throw new NotImplementedException();
    }

    public static void SetConditionPartSysArg(in this DispatcherCallbackArgs args, int index, PartSys partSys)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs > index);
        throw new NotImplementedException();
    }

    public static void KillPartSysInArg(in this DispatcherCallbackArgs args, int index)
    {
        var partSys = args.GetConditionPartSysArg(index);
        if (partSys != null)
        {
            GameSystems.ParticleSys.Remove(partSys);
            args.SetConditionPartSysArg(index, null);
        }
        throw new NotImplementedException();
    }

    public static void EndPartSysInArg(in this DispatcherCallbackArgs args, int index)
    {
        var partSys = args.GetConditionPartSysArg(index);
        if (partSys != null)
        {
            GameSystems.ParticleSys.End(partSys);
            args.SetConditionPartSysArg(index, null);
        }
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x100e1ad0)]
    public static void SetConditionArg(in this DispatcherCallbackArgs args, int argIndex, int value)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs > argIndex);
        args.subDispNode.condNode.args[argIndex] = value;
    }

    [TempleDllLocation(0x100e1ad0)]
    public static void SetConditionArg1(in this DispatcherCallbackArgs args, int value)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 1);
        args.subDispNode.condNode.args[0] = value;
    }

    [TempleDllLocation(0x100e1ad0)]
    public static void SetConditionArg2(in this DispatcherCallbackArgs args, int value)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 2);
        args.subDispNode.condNode.args[1] = value;
    }

    [TempleDllLocation(0x100e1ad0)]
    public static void SetConditionArg3(in this DispatcherCallbackArgs args, int value)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 3);
        args.subDispNode.condNode.args[2] = value;
    }

    [TempleDllLocation(0x100e1ad0)]
    public static void SetConditionArg4(in this DispatcherCallbackArgs args, int value)
    {
        Debug.Assert(args.subDispNode.condNode.condStruct.numArgs >= 4);
        args.subDispNode.condNode.args[3] = value;
    }

    #endregion

    [TempleDllLocation(0x1004d5f0)]
    public static void RemoveThisCondition(in this DispatcherCallbackArgs args)
    {
        var dispatcher = args.objHndCaller.GetDispatcher() as Dispatcher;
        dispatcher.DispatchRemoveCondition(args.subDispNode.condNode);
    }

    public static Stat GetAttributeFromDispatcherKey(in this DispatcherCallbackArgs evt)
    {
        return (Stat) (evt.dispKey - 1);
    }

    public static SavingThrowType GetSavingThrowTypeFromDispatcherKey(in this DispatcherCallbackArgs evt)
    {
        return (SavingThrowType) (evt.dispKey - 7);
    }

    public static Stat GetClassFromDispatcherKey(in this DispatcherCallbackArgs evt)
    {
        return (Stat) (evt.dispKey - 63);
    }

    public static SkillId GetSkillIdFromDispatcherKey(in this DispatcherCallbackArgs evt)
    {
        return (SkillId) (evt.dispKey - 20);
    }

    public static string GetConditionName(in this DispatcherCallbackArgs evt)
        => evt.subDispNode.condNode.condStruct.condName;

    // Convenice function for creating a toggle radial menu entry for a condition argument, to toggle between
    // 0 and 1
    public static RadialMenuEntry CreateToggleForArg(in this DispatcherCallbackArgs evt, int arg)
    {
        // The condition attachment is a heap type and we'll keep it around in the closure
        var condAttachment = evt.subDispNode.condNode;
        Debug.Assert(condAttachment.condStruct.numArgs >= arg + 1);

        bool Getter() => (int) condAttachment.args[arg] != 0;
        void Setter(bool newValue) => condAttachment.args[arg] = newValue ? 1 : 0;

        return RadialMenuEntry.CreateToggle(Getter, Setter);
    }

    // Convenice function for creating a slider radial menu entry for a condition argument
    public static RadialMenuEntry CreateSliderForArg(in this DispatcherCallbackArgs evt, int arg,
        int minVal, int maxVal)
    {
        // The condition attachment is a heap type and we'll keep it around in the closure
        var condAttachment = evt.subDispNode.condNode;
        Debug.Assert(condAttachment.condStruct.numArgs >= arg + 1);

        return RadialMenuEntry.CreateSlider(
            () => (int) condAttachment.args[arg],
            newVal => condAttachment.args[arg] = newVal,
            minVal,
            maxVal
        );
    }

}

public delegate void SubDispatcherCallback(
    in DispatcherCallbackArgs evt
);

public delegate void SubDispatcherCallback<T>(
    in DispatcherCallbackArgs evt,
    T data1
);

public delegate void SubDispatcherCallback<T, U>(
    in DispatcherCallbackArgs evt,
    T data1,
    U data2
);

public class SubDispatcherSpec
{
    public DispatcherType dispType;
    public D20DispatcherKey dispKey;
    public SubDispatcherCallback callback;

    public SubDispatcherSpec(DispatcherType dispType, D20DispatcherKey dispKey, SubDispatcherCallback callback)
    {
        this.dispType = dispType;
        this.dispKey = dispKey;
        this.callback = callback;
    }
}