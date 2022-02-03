
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts.Spells
{
    [SpellScript(120)]
    public class DetectUndead : BaseSpellScript
    {

        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Undead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-divination-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Detect Undead OnSpellEffect");
            spell.duration = 10 * spell.casterLevel;

            var target = spell.Targets[0];

            target.Object.AddCondition("sp-Detect Undead", spell.spellId, spell.duration, 0);
            target.ParticleSystem = AttachParticles("sp-Detect Undead", target.Object);

        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Detect Undead OnBeginRound");
            foreach (var obj in ObjList.ListCone(spell.caster, ObjectListFilter.OLC_CRITTERS, spell.spellRange, -45, 90))
            {
                if (obj.IsMonsterCategory(MonsterCategory.undead))
                {
                    if (GameSystems.Critter.GetHitDiceNum(obj) >= 11)
                    {
                        AttachParticles("sp-Detect Undead 3 High", obj);
                    }
                    else if (GameSystems.Critter.GetHitDiceNum(obj) >= 5)
                    {
                        AttachParticles("sp-Detect Undead 2 Med", obj);
                    }
                    else
                    {
                        AttachParticles("sp-Detect Undead 1 Low", obj);
                    }

                }

            }

        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Detect Undead OnEndSpellCast");
        }


    }
}
