
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
    [SpellScript(753)]
    public class HarpySong : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Fear OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        // game.particles( "sp-necromancy-conjure", spell.caster )

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Harpy Song OnSpellEffect");
            // remove_list = []
            var npc = spell.caster;
            if (npc.GetNameId() == 14243) // Harpy
            {
                spell.dc = 16;
            }

            spell.duration = 5;
            Sound(4187, 1);
            foreach (var target_item in spell.Targets)
            {
                if (!((PartyLeader.GetPartyMembers()).Contains(target_item.Object)))
                {
                    continue;

                }

                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    // target_item.obj.condition_add_with_args( 'Fascinate', spell.id, spell.duration, 0 )
                    target_item.Object.AddCondition("Captivating Song", spell.spellId, spell.duration, 0, 0, 0, 0, 0, 0);
                }
                else
                {
                    // target_item.partsys_id = game.particles( 'sp-Fear-Hit', target_item.obj )
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }

            }

            // spell.target_list.remove_list( remove_list )
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Song OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Song OnEndSpellCast");
        }

    }
}
