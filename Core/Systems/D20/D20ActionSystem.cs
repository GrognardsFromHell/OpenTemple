using System;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20ActionSystem : IDisposable
    {
        [TempleDllLocation(0x10092800)]
        public D20ActionSystem()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10089ef0)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10097be0)]
        public void ActionSequencesResetOnCombatEnd()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10095fd0)]
        public bool TurnBasedStatusInit(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10094A00)]
        public void CurSeqReset(GameObjectBody obj)
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100949e0)]
        public void GlobD20ActnInit()
        {
            // NOTE: TEMPLEPLUS REPLACED
            // D20ActnInit(globD20Action->d20APerformer, globD20Action);
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10097C20)]
        public void ActionAddToSeq()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100961C0)]
        public void sequencePerform()
        {
            // NOTE: TEMPLEPLUS REPLACED
            throw new NotImplementedException();
        }
    }
}