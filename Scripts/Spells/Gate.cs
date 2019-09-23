
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
            // this needs to be in every summon spell script!
            spell.duration = 500;
            // What Demon will it be?
            var dice1 = Dice.D100;
            var what_summoned = dice1.Roll();
            if (what_summoned > 99)
            {
                // set the  proto_id for Balor
                var monster_proto_id = 14286;
                var num_monsters = 1;
            }
            else if (what_summoned > 75)
            {
                // set the  proto_id for Glabrezu
                var monster_proto_id = 14263;
                var num_monsters = 1;
            }
            else if (what_summoned > 50)
            {
                // set the  proto_id for Hezrou
                var monster_proto_id = 14259;
                var dice2 = Dice.D2;

                var num_monsters = dice2.Roll();

            }
            else
            {
                // set the  proto_id for Vrock
                var monster_proto_id = 14258;
                var dice2 = Dice.D3;

                var num_monsters = dice2.Roll();

            }

            var i = 0;
            while (i < num_monsters)
            {
                // create monster, monster should be added to target_list
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
