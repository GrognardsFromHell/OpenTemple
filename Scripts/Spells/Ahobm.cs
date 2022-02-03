
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

namespace Scripts.Spells;

[SpellScript(570)]
public class Ahobm : BaseSpellScript
{
    public override void OnBeginSpellCast(SpellPacketBody spell)
    {
        Logger.Info("ahobm OnBeginSpellCast");
        Logger.Info("spell.target_list={0}", spell.Targets);
        Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        AttachParticles("sp-enchantment-conjure", spell.caster);
    }
    public override void OnSpellEffect(SpellPacketBody spell)
    {
        Logger.Info("ahobm OnSpellEffect");
        var remove_list = new List<GameObject>();
        spell.duration = 1;
        AttachParticles("sp-Bane", spell.caster);
        foreach (var target_item in spell.Targets)
        {
            var tar = target_item.Object;
            if ((tar.GetNameId() == 8064) || (tar.GetNameId() == 8042) || (tar.GetNameId() == 8043) || (tar.GetNameId() == 14455))
            {
                remove_list.Add(tar);
                tar.FloatMesFileLine("mes/spell.mes", 20048);
            }
            else if (tar.IsMonsterCategory(MonsterCategory.plant))
            {
                remove_list.Add(tar);
                tar.FloatMesFileLine("mes/spell.mes", 20049);
            }
            else if (tar.IsMonsterCategory(MonsterCategory.ooze))
            {
                remove_list.Add(tar);
                tar.FloatMesFileLine("mes/spell.mes", 20049);
            }
            else if (tar.IsMonsterCategory(MonsterCategory.aberration))
            {
                remove_list.Add(tar);
                tar.FloatMesFileLine("mes/spell.mes", 20049);
            }
            else if (tar.IsMonsterCategory(MonsterCategory.undead))
            {
                remove_list.Add(tar);
                tar.FloatMesFileLine("mes/spell.mes", 20050);
            }
            else if (tar.GetNameId() == 14455)
            {
                remove_list.Add(tar);
            }
            else
            {
                tar.FloatMesFileLine("mes/spell.mes", 30002);
                tar.FloatMesFileLine("mes/spell.mes", 20047);
                tar.KillWithDeathEffect();
            }

        }

        spell.RemoveTargets(remove_list);
        Co8.End_Spell(spell);
        spell.EndSpell();
    }
    public override void OnBeginRound(SpellPacketBody spell)
    {
        Logger.Info("ahobm OnBeginRound");
    }
    public override void OnEndSpellCast(SpellPacketBody spell)
    {
        Logger.Info("ahobm OnBeginRound");
    }

}