using System;
using System.Collections.Generic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Actions
{

    public class ActionSequence
    {
        private static long nextSerial;

        public long Serial { get; } = nextSerial++;

        public D20Action CurrentAction => d20ActArray[d20aCurIdx];

        public List<D20Action> d20ActArray = new List<D20Action>();
        public int d20ActArrayNum => d20ActArray.Count;
        public int d20aCurIdx; // inited to -1
        public ActionSequence prevSeq;
        public ActionSequence interruptSeq;
        public TurnBasedStatus tbStatus = new TurnBasedStatus();
        public GameObjectBody performer;
        public LocAndOffsets performerLoc;
        public GameObjectBody targetObj;
        public SpellPacketBody spellPktBody = new SpellPacketBody();
        public D20Action castSpellAction;
        public bool ignoreLos;

        public ActionSequence Copy()
        {
            var result = new ActionSequence();
            CopyTo(result);
            return result;
        }

        public void CopyTo(ActionSequence otherSequence)
        {
            otherSequence.d20ActArray = new List<D20Action>(d20ActArray.Select(a => a.Copy()));
            otherSequence.d20aCurIdx = d20aCurIdx;
            otherSequence.prevSeq = prevSeq;
            otherSequence.interruptSeq = interruptSeq;
            tbStatus.CopyTo(otherSequence.tbStatus);
            otherSequence.performer = performer;
            otherSequence.performerLoc = performerLoc;
            otherSequence.targetObj = targetObj;
            otherSequence.spellPktBody = spellPktBody;
            otherSequence.castSpellAction = castSpellAction?.Copy();
            otherSequence.ignoreLos = ignoreLos;
            otherSequence.IsPerforming = IsPerforming;
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

        public void ResetSpell()
        {
            spellPktBody = new SpellPacketBody();
        }
    }
}