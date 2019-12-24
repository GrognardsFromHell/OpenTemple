
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
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Spells
{
    [SpellScript(567)]
    public class Reincarnation : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Reincarnation OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-transmutation-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Reincarnation OnSpellEffect");
            spell.duration = 1;
            var target_item = spell.Targets[0];
            target_item.Object.AddCondition("sp-Raise Dead", spell.spellId, spell.duration, 0);
            // target_item.partsys_id = game.particles( 'sp-Raise Dead', target_item.obj )
            // target_item.obj.stat_base_set(stat_race, (race_gnome))
            if (target_item.Object.GetStat(Stat.level) == 1)
            {
                SetGlobalVar(752, 0);
            }
            else if (target_item.Object.GetStat(Stat.level) == 2)
            {
                SetGlobalVar(752, 500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 3)
            {
                SetGlobalVar(752, 2000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 4)
            {
                SetGlobalVar(752, 4500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 5)
            {
                SetGlobalVar(752, 8000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 6)
            {
                SetGlobalVar(752, 12500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 7)
            {
                SetGlobalVar(752, 18000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 8)
            {
                SetGlobalVar(752, 24500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 9)
            {
                SetGlobalVar(752, 32000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 10)
            {
                SetGlobalVar(752, 40500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 11)
            {
                SetGlobalVar(752, 50000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 12)
            {
                SetGlobalVar(752, 60500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 13)
            {
                SetGlobalVar(752, 72000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 14)
            {
                SetGlobalVar(752, 84500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 15)
            {
                SetGlobalVar(752, 98000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 16)
            {
                SetGlobalVar(752, 112500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 17)
            {
                SetGlobalVar(752, 128000);
            }
            else if (target_item.Object.GetStat(Stat.level) == 18)
            {
                SetGlobalVar(752, 144500);
            }
            else if (target_item.Object.GetStat(Stat.level) == 19)
            {
                SetGlobalVar(752, 162000);
            }
            else
            {
                SetGlobalVar(752, 180500);
            }

            target_item.Object.SetBaseStat(Stat.experience, GetGlobalVar(752));
            target_item.Object.ExecuteObjectScript(target_item.Object, ObjScriptEvent.Resurrect);
            Co8.End_Spell(spell);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Reincarnation OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Reincarnation OnEndSpellCast");
        }

    }
}
