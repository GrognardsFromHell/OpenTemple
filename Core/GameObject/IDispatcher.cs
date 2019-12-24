using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Conditions;

namespace OpenTemple.Core.GameObject
{
    public interface IDispatcher
    {
        void Process(DispatcherType type, D20DispatcherKey key, object args);

    }

    public struct SubDispatcherAttachment
    {
        public SubDispatcherSpec subDispDef;
        public ConditionAttachment condNode;

        public SubDispatcherAttachment(SubDispatcherSpec subDispDef, ConditionAttachment condNode)
        {
            this.subDispDef = subDispDef;
            this.condNode = condNode;
        }
    }
}