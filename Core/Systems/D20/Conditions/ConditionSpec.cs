using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    /// <summary>
    /// This defines a condition and what it does, while CondNode represents an instance of this or another condition.
    /// Called CondNode in TP.
    /// </summary>
    public class ConditionSpec
    {
        public string condName { get;}

        public int numArgs { get; }

        // This is a variable length array of dispatcher hooks that this condition has
        public SubDispatcherSpec[] subDispDefs;

        public ConditionSpec(string condName, int numArgs, params SubDispatcherSpec[] subDispDefs)
        {
            this.condName = condName;
            this.numArgs = numArgs;
            this.subDispDefs = subDispDefs;
        }

        public static Builder Create(string name, int numArgs = 0) => new Builder(name, numArgs);

        public class Builder
        {

            private readonly string _name;

            private readonly int _numArgs;

            private readonly List<SubDispatcherSpec> _subDisps = new List<SubDispatcherSpec>();

            internal Builder(string name, int numArgs)
            {
                _name = name;
                _numArgs = numArgs;
            }

            /// <summary>
            /// Adds a handler that'll handle the given dispatcher type for every key.
            /// </summary>
            public Builder AddHandler(DispatcherType type, SubDispatcherCallback handler)
            {
                return this;
            }

            /// <summary>
            /// Adds a handler that'll handle the given dispatcher type only for the given key.
            /// </summary>
            public Builder AddHandler(DispatcherType type, D20DispatcherKey key, SubDispatcherCallback handler)
            {
                return this;
            }

            public ConditionSpec Build()
            {
                return new ConditionSpec(_name, _numArgs, _subDisps.ToArray());
            }
        }

    }

    public static class ConditionSpecBuilderExtensions
    {

        public static ConditionSpec.Builder AddSkillLevelHandler(this ConditionSpec.Builder builder, SkillId skill, SubDispatcherCallback callback)
        {
            builder.AddHandler(DispatcherType.SkillLevel, (D20DispatcherKey)(D20DispatcherKey.SKILL_APPRAISE + (int) skill), callback);
            return builder;
        }

        public static ConditionSpec.Builder AddQueryHandler(this ConditionSpec.Builder builder, D20DispatcherKey query, SubDispatcherCallback callback)
        {
            builder.AddHandler(DispatcherType.D20Query, query, callback);
            return builder;
        }

        public static ConditionSpec.Builder AddSignalHandler(this ConditionSpec.Builder builder, D20DispatcherKey signal, SubDispatcherCallback callback)
        {
            builder.AddHandler(DispatcherType.D20Signal, signal, callback);
            return builder;
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
        public readonly GameObjectBody objHndCaller; // Rename to obj
        public readonly DispatcherType dispType;
        public readonly D20DispatcherKey dispKey;
        public readonly object dispIO; // Rename to dispIo
        public DispatcherCallbackArgs(SubDispatcherAttachment attachment, GameObjectBody obj, DispatcherType dispType, D20DispatcherKey dispKey, object dispIo)
        {
            this.subDispNode = attachment;
            this.objHndCaller = obj ?? throw new ArgumentNullException(nameof(obj));
            this.dispType = dispType;
            this.dispKey = dispKey;
            this.dispIO = dispIo ?? throw new ArgumentNullException(nameof(dispIo));
        }
    }

    public static class DispatcherCallbackArgsExtensions {
        public static DispIoObjBonus GetDispIoObjBonus(in this DispatcherCallbackArgs args) {
            return (DispIoObjBonus) args.dispIO;
        }
    }

    public delegate void SubDispatcherCallback(
        in DispatcherCallbackArgs callbackArgs
    );

    public class SubDispatcherSpec
    {
        public DispatcherType dispType;
        public D20DispatcherKey dispKey;
        public SubDispatcherCallback callback;
        public int data1;
        public int data2;

        public SubDispatcherSpec(DispatcherType dispType, D20DispatcherKey dispKey, SubDispatcherCallback callback, int data1 = 0, int data2 = 0)
        {
            this.dispType = dispType;
            this.dispKey = dispKey;
            this.callback = callback;
            this.data1 = data1;
            this.data2 = data2;
        }
    }
}