using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Actions
{

    [Flags]
    public enum SequenceFlags {
        PERFORMING = 1,
        UNK2 = 2
    };

    public class ActionSequence
    {
        public List<D20Action> d20ActArray = new List<D20Action>();
        public int d20ActArrayNum => d20ActArray.Count;
        public int d20aCurIdx; // inited to -1
        public ActionSequence prevSeq;
        public ActionSequence interruptSeq;
        public SequenceFlags seqOccupied; // is actually flags; 1 - performing; 2 - aoo maybe?
        public TurnBasedStatus tbStatus;
        public GameObjectBody performer;
        public LocAndOffsets performerLoc;
        public GameObjectBody targetObj;
        public SpellPacketBody spellPktBody;
        public D20Action d20Action;
        public uint ignoreLos; // probably bool

        public ActionSequence Copy()
        {
            var result = (ActionSequence) MemberwiseClone();

            // Copy any reference fields
            result.d20ActArray = new List<D20Action>(result.d20ActArray.Select(a => a.Copy()));
            result.tbStatus = result.tbStatus.Copy();
            result.d20Action = result.d20Action.Copy();

            return result;
        }

        public bool IsPerforming
        {
            get => seqOccupied.HasFlag(SequenceFlags.PERFORMING);
            set => seqOccupied |= SequenceFlags.PERFORMING;
        }

        public bool IsLastAction => d20aCurIdx == d20ActArrayNum - 1;
    }
}
