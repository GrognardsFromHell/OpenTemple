
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(608)]
    public class Gate : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Gate OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("gate OnSpellEffect");
            spell.duration = 500;

            var dice1 = Dice.D100;

            var what_summoned = dice1.Roll();

            int monster_proto_id; // DECL_PULL_UP
            int num_monsters; // DECL_PULL_UP
            if (what_summoned > 99)
            {
                monster_proto_id = 14286;

                num_monsters = 1;

            }
            else if (what_summoned > 75)
            {
                monster_proto_id = 14263;

                num_monsters = 1;

            }
            else if (what_summoned > 50)
            {
                monster_proto_id = 14259;

                var dice2 = Dice.D2;


                num_monsters = dice2.Roll();


            }
            else
            {
                monster_proto_id = 14258;

                var dice2 = Dice.D3;


                num_monsters = dice2.Roll();


            }

            var i = 0;

            while (i < num_monsters)
            {
                spell.SummonMonsters(true, monster_proto_id);
                i = i + 1;

            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Gate OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Gate OnEndSpellCast");
        }


    }
}
