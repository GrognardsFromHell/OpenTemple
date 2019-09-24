
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(470)]
    public class SummonMonsterIV : SummonMonsterBase
    {

        protected override string SpellName => "Summon Monster IV";

        protected override string ParticleSystemId => "sp-Summon Monster IV";

        protected override int SpellOptionsKey => 1300;

        protected override void ModifySummonedProtoId(SpellPacketBody spell, ref int protoId)
        {
            var npc = spell.caster;
            if (npc.GetNameId() == 8047) // Alrrem
            {
                protoId = 14569; // Fiendish Huge Viper w. faction 5
            }
        }

    }
}
