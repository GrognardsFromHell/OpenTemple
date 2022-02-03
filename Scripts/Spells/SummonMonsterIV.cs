
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

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
