using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    [Flags]
    public enum SequenceFlags
    {
        PERFORMING = 1,
        INTERRUPTED = 2
    };

    public class ActionSequence
    {
        private static long nextSerial;

        public long Serial { get; } = nextSerial++;

        public List<D20Action> d20ActArray = new List<D20Action>();
        public int d20ActArrayNum => d20ActArray.Count;
        public int d20aCurIdx; // inited to -1
        public ActionSequence prevSeq;
        public ActionSequence interruptSeq;
        public SequenceFlags seqOccupied; // is actually flags; 1 - performing; 2 - aoo maybe?
        public TurnBasedStatus tbStatus = new TurnBasedStatus();
        public GameObjectBody performer;
        public LocAndOffsets performerLoc;
        public GameObjectBody targetObj;
        public SpellPacketBody spellPktBody = new SpellPacketBody();
        public D20Action d20Action;
        public bool ignoreLos;

        public ActionSequence Copy()
        {
            var result = (ActionSequence) MemberwiseClone();

            // Copy any reference fields
            result.d20ActArray = new List<D20Action>(result.d20ActArray.Select(a => a.Copy()));
            result.tbStatus = result.tbStatus.Copy();
            result.d20Action = result.d20Action.Copy();

            return result;
        }

        // See SequenceFlags for save/load
        public bool IsPerforming { get; set; }

        // See SequenceFlags for save/load
        public bool IsInterrupted { get; set; }

        public bool IsLastAction => d20aCurIdx == d20ActArrayNum - 1;

        public override string ToString()
        {
            return $"ActionSequence({performer};{Serial})";
        }
    }
}