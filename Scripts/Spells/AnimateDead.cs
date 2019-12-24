
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
    [SpellScript(11)]
    public class AnimateDead : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-necromancy-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            var cast = spell.caster;
            if (cast.GetNameId() == 8042)
            {
                foreach (var npc in ObjList.ListVicinity(cast.GetLocation(), ObjectListFilter.OLC_NPC))
                {
                    if (!(GameSystems.Party.PartyMembers).Contains(npc))
                    {
                        if (npc.GetStat(Stat.hp_current) <= -10)
                        {
                            target_item.Object = npc;
                            target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
                            target_item.ParticleSystem = AttachParticles("sp-Iuz Making Zombies", target_item.Object);
                            var zombie = GameSystems.MapObject.CreateObject(14123, npc.GetLocation());
                        }

                    }

                }

            }

            if (cast.GetNameId() == 14425)
            {
                SetGlobalFlag(811, false);
                foreach (var obj in ObjList.ListVicinity(cast.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    var curr = obj.GetStat(Stat.hp_current);
                    if ((curr <= -10 && obj.DistanceTo(cast) <= 10 && !(GameSystems.Party.PartyMembers).Contains(obj) && !GetGlobalFlag(811)))
                    {
                        if ((obj.IsMonsterCategory(MonsterCategory.humanoid) || obj.IsMonsterCategory(MonsterCategory.fey) || obj.IsMonsterCategory(MonsterCategory.giant) || obj.IsMonsterCategory(MonsterCategory.monstrous_humanoid)))
                        {
                            target_item.Object = obj;
                            SetGlobalFlag(811, true);
                        }

                    }

                }

            }

            // debit money (?)
            // Solves Radial menu problem for Wands/NPCs
            var spell_arg = spell.GetMenuArg(RadialMenuParam.MinSetting);
            if (spell_arg != 1 && spell_arg != 2)
            {
                spell_arg = RandomRange(1, 2);
            }

            // make sure target is not already an undead
            if (target_item.Object.GetStat(Stat.hp_current) >= -9)
            {
                // not a animate-able
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30016);
                AttachParticles("Fizzle", target_item.Object);
            }
            else if (target_item.Object.IsMonsterCategory(MonsterCategory.humanoid) || target_item.Object.IsMonsterCategory(MonsterCategory.fey) || target_item.Object.IsMonsterCategory(MonsterCategory.giant) || target_item.Object.IsMonsterCategory(MonsterCategory.monstrous_humanoid))
            {
                if (cast.GetNameId() != 8042 && cast.GetNameId() != 14425)
                {
                    target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, spell_arg);
                    target_item.ParticleSystem = AttachParticles("sp-Animate Dead", target_item.Object);
                }

                if (cast.GetNameId() == 14425)
                {
                    target_item.Object.AddCondition("sp-Animate Dead", spell.spellId, spell.duration, 3);
                    target_item.ParticleSystem = AttachParticles("sp-Animate Dead", target_item.Object);
                    var zombie = GameSystems.MapObject.CreateObject(14619, target_item.Object.GetLocation());
                }

            }
            else
            {
                // not a animate-able
                target_item.Object.FloatMesFileLine("mes/spell.mes", 30000);
                target_item.Object.FloatMesFileLine("mes/spell.mes", 31009);
                AttachParticles("Fizzle", target_item.Object);
            }

            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Animate Dead OnEndSpellCast");
        }

    }
}
