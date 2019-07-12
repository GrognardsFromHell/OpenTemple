using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.Pathfinding;
using SpicyTemple.Core.Systems.Spells;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    public class D20Action
    {
        public D20ActionType d20ActType;
        public int data1; // generic piece of data
        public D20CAF d20Caf; // Based on D20_CAF flags
        public uint field_C; // unknown use
        public GameObjectBody d20APerformer;
        public GameObjectBody d20ATarget;
        public LocAndOffsets destLoc; // action located (usually movement destination)
        public float distTraversed; // distanced traversed by a move action
        public uint radialMenuActualArg; // the value chosen by radial menu toggle/slider
        public int rollHistId0 = -1;
        public int rollHistId1 = -1;
        public int rollHistId2 = -1;

        // TODO D20SpellData d20SpellData;
        public uint spellId;
        public uint animID;
        public PathQueryResult path;

        public D20Action()
        {
        }

        public D20Action(D20ActionType type)
        {
            d20ActType = type;
        }

        public int FilterSpellTargets(SpellPacketBody spellPkt) // returns number of remaining targets
        {
            throw new NotImplementedException();
        }

        public D20ADF GetActionDefinitionFlags()
        {
            return GameSystems.D20.Actions.GetActionFlags(d20ActType);
        }

        public bool IsMeleeHit()
        {
            return d20Caf.HasFlag(D20CAF.HIT) && !d20Caf.HasFlag(D20CAF.RANGED);
        }
    }
}