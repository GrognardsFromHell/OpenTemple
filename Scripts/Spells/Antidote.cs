
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
    [SpellScript(777)]
    public class Antidote : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Antidote OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-conjuration-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Antidote OnSpellEffect");
            spell.duration = 600;
            if (Co8.find_spell_obj_with_flag(spell.caster, 12706, Co8SpellFlag.AnalyzeDweomer) == null)
            {
                var spell_obj = GameSystems.MapObject.CreateObject(12706, spell.caster.GetLocation());
                Co8.set_spell_flag(spell_obj, Co8SpellFlag.AnalyzeDweomer);
                spell_obj.AddConditionToItem("Luck Poison Save Bonus", 5, 0);
                spell.caster.GetItem(spell_obj);
                spell.ClearTargets();
                spell.AddTarget(spell.caster);
                spell.caster.AddCondition("sp-Endurance", spell.spellId, spell.duration, 0);
            }
            else
            {
                spell.caster.FloatMesFileLine("mes/spell.mes", 16007);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Antidote OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Antidote OnEndSpellCast");
            Co8.destroy_spell_obj_with_flag(spell.caster, 12706, Co8SpellFlag.AnalyzeDweomer);
        }

    }
}
