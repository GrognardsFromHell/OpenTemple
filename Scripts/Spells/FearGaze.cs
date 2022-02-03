
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
    [SpellScript(755)]
    public class FearGaze : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Fear OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Fear OnSpellEffect");
            // remove_list = []
            var npc = spell.caster;
            if (npc.GetNameId() == 14358) // Balor Guardian
            {
                spell.dc = 25;
            }

            if (npc.GetNameId() == 14999) // Old White Dragon frightful presence
            {
                spell.dc = 23;
            }

            if (npc.GetNameId() == 14280) // Groaning Spirit
            {
                spell.dc = 19;
            }

            spell.duration = Math.Min(1 * spell.casterLevel, 10);
            if (npc.GetNameId() == 14958) // Nightwalker
            {
                spell.dc = 24;
                spell.duration = RandomRange(1, 8);
            }

            AttachParticles("sp-Fear", spell.caster);
            // get all targets in a 25ft + 2ft/level cone (60')
            foreach (var target_item in spell.Targets)
            {
                if (npc.GetNameId() == 14999)
                {
                    if (target_item.Object.type == ObjectType.pc)
                    {
                        if (target_item.Object.GetStat(Stat.level) >= 18)
                        {
                            // creatures with equal or greater HD than the dragon are unaffected
                            continue;

                        }

                    }

                }

                if (!target_item.Object.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                {
                    // saving throw unsuccessful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30002);
                    target_item.Object.AddCondition("sp-Fear", spell.spellId, spell.duration, 0);
                    target_item.ParticleSystem = AttachParticles("sp-Fear-Hit", target_item.Object);
                }
                else
                {
                    // saving throw successful
                    target_item.Object.FloatMesFileLine("mes/spell.mes", 30001);
                }

            }

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Fear OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Fear OnEndSpellCast");
        }

    }
}
