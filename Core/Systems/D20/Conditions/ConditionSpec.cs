using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20.Conditions
{
    /// <summary>
    /// This defines a condition and what it does, while CondNode represents an instance of this or another condition.
    /// Called CondNode in TP.
    /// </summary>
    public class ConditionSpec
    {
        public string condName;

        public int numArgs;

        // This is a variable length array of dispatcher hooks that this condition has
        public SubDispatcherSpec[] subDispDefs;
    }

    public delegate void SubDispatcherCallback(
        in SubDispatcherAttachment attachment,
        GameObjectBody obj,
        DispatcherType dispType,
        D20DispatcherKey dispKey,
        object dispIo
    );

    public class SubDispatcherSpec
    {
        public DispatcherType dispType;
        public D20DispatcherKey dispKey;
        public SubDispatcherCallback callback;
        public uint data1;
        public uint data2;
    }
}