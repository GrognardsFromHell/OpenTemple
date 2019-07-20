using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public class ActionSequence
    {
        public List<D20Action> d20ActArray = new List<D20Action>();
        public int d20ActArrayNum => d20ActArray.Count;
        public int d20aCurIdx; // inited to -1
        public ActionSequence prevSeq;
        public ActionSequence interruptSeq;
        public uint seqOccupied; // is actually flags; 1 - performing; 2 - aoo maybe?
        public TurnBasedStatus tbStatus;
        public GameObjectBody performer;
        public LocAndOffsets performerLoc;
        public GameObjectBody targetObj;
        public SpellPacketBody spellPktBody;
        public D20Action d20Action;
        public uint ignoreLos; // probably bool

        public bool IsLastAction => d20aCurIdx == d20ActArrayNum - 1;
    }
}
