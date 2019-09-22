
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(603)]
    public class ZuggtmoySummonFungi : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Zuggtmoy Summon Fungi OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Zuggtmoy Summon Fungi OnSpellEffect");
            spell.duration = 1 * spell.casterLevel;

            var dice1 = Dice.D3;

            var num_summoned = dice1.Roll() + 1;

            var monster_proto_id = 14281;

            var i = 0;

            while (i < num_summoned)
            {
                spell.SummonMonsters(true, monster_proto_id);
                i = i + 1;

            }

            var dice2 = Dice.D3;

            num_summoned = dice2.Roll() + 1;

            monster_proto_id = 14284;

            var j = 0;

            while (j < num_summoned)
            {
                spell.SummonMonsters(true, monster_proto_id);
                j = j + 1;

            }

            var dice3 = Dice.D3;

            num_summoned = dice3.Roll() + 1;

            monster_proto_id = 14283;

            var k = 0;

            while (k < num_summoned)
            {
                spell.SummonMonsters(true, monster_proto_id);
                k = k + 1;

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Zuggtmoy Summon Fungi OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Zuggtmoy Summon Fungi OnEndSpellCast");
        }


    }
}
