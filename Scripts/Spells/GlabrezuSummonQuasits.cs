
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
    [SpellScript(604)]
    public class GlabrezuSummonQuasits : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Glabrezu Summon Quasits OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Glabrezu Summon Quasits OnSpellEffect");
            // this needs to be in every summon spell script!
            spell.duration = 1 * spell.casterLevel;
            // how many quasits will there be?
            var dice = Dice.D4;
            var num_quasits = dice.Roll();
            // set the proto_id for this monster
            var quasit_proto_id = 14110;
            var npc = spell.caster; // Vrock/Hezrou/Glabrezu Guardian can summon 1d10 Quasits
            if (npc.GetNameId() == 14360 || npc.GetNameId() == 14361 || npc.GetNameId() == 14359)
            {
                var dice2 = Dice.Parse("2d3");
                num_quasits = num_quasits + dice2.Roll();
            }

            // Balor Guardian
            if (npc.GetNameId() == 14358)
            {
                var chance = RandomRange(1, 100);
                if (chance <= 25) // Lots of Quasits
                {
                    num_quasits = RandomRange(4, 10);
                }

                if (chance >= 26 && chance <= 50) // Lesser Glabrezu
                {
                    num_quasits = 1;
                    quasit_proto_id = 14263;
                }

                if (chance >= 51 && chance <= 75) // Lesser Hezrou
                {
                    num_quasits = RandomRange(1, 3);
                    quasit_proto_id = 14259;
                }

                if (chance >= 76 && chance <= 100) // Lesser Balor
                {
                    num_quasits = 1;
                    quasit_proto_id = 14286;
                }

            }

            // Vrock Guardian has 25% chance to summon a lesser Vrock instead of Quasitz
            if (npc.GetNameId() == 14361 && RandomRange(1, 100) <= 25)
            {
                num_quasits = 1;
                quasit_proto_id = 14258;
            }

            // Hezrou Guardian has 25% chance to summon a lesser Hezrou instead of Quasitz
            if (npc.GetNameId() == 14360) // and game.random_range(1,100) <= 25:
            {
                num_quasits = 1;
                quasit_proto_id = 14259;
            }

            // Glabrezu Guardian has 25% chance to summon a lesser Vrock instead of Quasitz
            if (npc.GetNameId() == 14359 && RandomRange(1, 100) <= 25)
            {
                num_quasits = 1;
                quasit_proto_id = 14258;
            }

            var i = 0;
            while (i < num_quasits)
            {
                // create monster, monster should be added to target_list
                spell.SummonMonsters(true, quasit_proto_id);
                i = i + 1;
            }

            // monster should disappear when duration is over, apply "TIMED_DISAPPEAR" condition
            // spell.target_list[0].condition_add_with_args( 'sp-Summoned', spell.id, spell.duration, 0 )
            // Think i am gonna let the creature be there indefinately
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Glabrezu Summon Quasits OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Glabrezu Summon Quasits OnEndSpellCast");
        }

    }
}
