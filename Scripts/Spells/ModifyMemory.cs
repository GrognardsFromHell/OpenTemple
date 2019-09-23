
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
    [SpellScript(318)]
    public class ModifyMemory : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Modify Memory OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
            AttachParticles("sp-enchantment-conjure", spell.caster);
        }
        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Modify Memory OnSpellEffect");
            spell.duration = 1;
            // First find the nearest NPC to the target location
            GameObjectBody new_targ = null;
            var dist = 4;
            foreach (var obj in ObjList.ListVicinity(spell.aoeCenter, ObjectListFilter.OLC_NPC))
            {
                var NEWdistance = 0;
                if ((obj.GetLeader() == null))
                {
                    var (x1, y1) = obj.GetLocation();
                    var (x2, y2) = spell.aoeCenter;
                    if (x1 > x2)
                    {
                        var x3 = x1 - x2;
                    }
                    else
                    {
                        var x3 = x2 - x1;
                    }

                    if (y1 > y2)
                    {
                        var y3 = y1 - y2;
                    }
                    else
                    {
                        var y3 = y2 - y1;
                    }

                    NEWdistance = Math.Pow(((x3 * x3) + (y3 * y3)), 0.5f);
                    var bet = NEWdistance;
                }
                else
                {
                    NEWdistance = 6;
                }

                if (NEWdistance <= dist)
                {
                    dist = NEWdistance;
                    new_targ = obj;
                }

            }

            if (new_targ == null)
            {
                var bob = spell.caster;
                AttachParticles("Fizzle", bob);
                bob.FloatMesFileLine("mes/narrative.mes", 161);
            }
            else
            {
                AttachParticles("sp-Feat of Strength-END", new_targ);
            }

            if (new_targ.GetStat(Stat.intelligence) >= 1)
            {
                if (!(new_targ.IsMonsterCategory(MonsterCategory.undead) || new_targ.IsMonsterCategory(MonsterCategory.ooze) || new_targ.IsMonsterCategory(MonsterCategory.aberration) || new_targ.IsMonsterCategory(MonsterCategory.outsider) || new_targ.IsMonsterCategory(MonsterCategory.construct)))
                {
                    if (new_targ.IsFriendly(spell.caster))
                    {
                        if (!new_targ.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            new_targ.AdjustReaction(PartyLeader, 40);
                            new_targ.ClearNpcFlag(NpcFlag.KOS);
                            new_targ.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                            var x = new_targ.GetScriptId(ObjScriptEvent.Heartbeat);
                            new_targ.RemoveScript(ObjScriptEvent.Heartbeat);
                            StartTimer(300000, () => reset_sid(new_targ, x));
                            AttachParticles("sp-Feat of Strength-END", new_targ);
                            var cozen = GameSystems.MapObject.CreateObject(12696, new_targ.GetLocation());
                            new_targ.GetItem(cozen);
                            cozen.ExecuteObjectScript(cozen, ObjScriptEvent.RemoveItem);
                        }
                        else
                        {
                            // game.particles( "sp-summon monster I", game.party[0] )
                            // saving throw successful
                            new_targ.FloatMesFileLine("mes/spell.mes", 30001);
                            var vigilance = GameSystems.MapObject.CreateObject(12695, new_targ.GetLocation());
                            new_targ.GetItem(vigilance);
                            vigilance.ExecuteObjectScript(vigilance, ObjScriptEvent.RemoveItem);
                        }

                    }
                    else
                    {
                        // game.particles( "sp-summon monster I", game.party[0] )
                        if (!new_targ.SavingThrowSpell(spell.dc, SavingThrowType.Will, D20SavingThrowFlag.NONE, spell.caster, spell.spellId))
                        {
                            new_targ.AdjustReaction(PartyLeader, 40);
                            new_targ.ClearNpcFlag(NpcFlag.KOS);
                            new_targ.SetNpcFlag(NpcFlag.KOS_OVERRIDE);
                            var x = new_targ.GetScriptId(ObjScriptEvent.Heartbeat);
                            new_targ.RemoveScript(ObjScriptEvent.Heartbeat);
                            StartTimer(300000, () => reset_sid(new_targ, x));
                            AttachParticles("sp-Feat of Strength-END", new_targ);
                            var cozen = GameSystems.MapObject.CreateObject(12696, new_targ.GetLocation());
                            new_targ.GetItem(cozen);
                            cozen.ExecuteObjectScript(cozen, ObjScriptEvent.RemoveItem);
                        }
                        else
                        {
                            // game.particles( "sp-summon monster I", game.party[0] )
                            // saving throw successful
                            new_targ.FloatMesFileLine("mes/spell.mes", 30001);
                            var vigilance = GameSystems.MapObject.CreateObject(12695, new_targ.GetLocation());
                            new_targ.GetItem(vigilance);
                            vigilance.ExecuteObjectScript(vigilance, ObjScriptEvent.RemoveItem);
                        }

                    }

                }
                else
                {
                    // game.particles( "sp-summon monster I", game.party[0] )
                    // something weird: undead, construct etc
                    new_targ.FloatMesFileLine("mes/spell.mes", 30003);
                    AttachParticles("Fizzle", new_targ);
                }

            }
            else
            {
                // critters with no intelligence have no memories!
                new_targ.FloatMesFileLine("mes/spell.mes", 20055);
                AttachParticles("Fizzle", new_targ);
            }

            spell.EndSpell();
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Modify Memory OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Modify Memory OnEndSpellCast");
        }
        public static void reset_sid(FIXME targ, FIXME id_x)
        {
            targ.scripts/*Unknown*/[19] = id_x;
            Sound(7461, 1);
            SpawnParticles("Fizzle", targ);
            return;
        }

    }
}
