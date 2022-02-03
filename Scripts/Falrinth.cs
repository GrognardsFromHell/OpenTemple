
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

namespace Scripts
{
    [ObjectScript(155)]
    public class Falrinth : BaseObjectScript
    {
        private static readonly string MINOR_GLOBE_OF_INVULNERABILITY_KEY = "Sp311_MINOR_GLOBE_OF_INVULNERABILITY_Activelist";
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalVar(901) == 2))
            {
                return RunDefault;
            }
            else if ((GetGlobalFlag(164)))
            {
                triggerer.BeginDialog(attachee, 130);
            }
            else
            {
                return RunDefault;
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(721, 0);
                    Co8.StopCombat(attachee, 1);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(335, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(821)))
            {
                Utilities.create_item_in_inventory(8904, attachee);
                SetGlobalVar(763, 0);
                Utilities.create_item_in_inventory(9173, attachee);
                SetGlobalFlag(821, true);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var leader = SelectedPartyLeader;
            SetGlobalVar(763, GetGlobalVar(763) + 1);
            if ((GetGlobalVar(763) == 2 || GetGlobalVar(763) == 35))
            {
                Utilities.create_item_in_inventory(8900, attachee);
                Utilities.create_item_in_inventory(8037, attachee);
            }

            if ((GetGlobalVar(763) == 3 || GetGlobalVar(763) == 40))
            {
                Utilities.create_item_in_inventory(8901, attachee);
            }

            if ((GetGlobalVar(763) == 8))
            {
                Utilities.create_item_in_inventory(8902, attachee);
            }

            if ((GetGlobalVar(763) == 11 || GetGlobalVar(763) == 30))
            {
                Utilities.create_item_in_inventory(8904, attachee);
            }

            if ((GetGlobalFlag(164) && GetGlobalVar(901) == 2))
            {
                return RunDefault;
            }
            // game.new_sid = 0
            else if ((Utilities.obj_percent_hp(attachee) < 50))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    // if pc.type == obj_t_pc:
                    attachee.AIRemoveFromShitlist(pc);
                }

                // game.global_flags[822] = 1
                if ((GetGlobalVar(901) == 0))
                {
                    leader.BeginDialog(attachee, 200);
                    return SkipDefault;
                }
                else if ((GetGlobalVar(901) == 1))
                {
                    leader.BeginDialog(attachee, 130);
                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(335, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GetGlobalFlag(821)))
            {
                Utilities.create_item_in_inventory(8904, attachee);
                SetGlobalVar(763, 0);
                Utilities.create_item_in_inventory(9173, attachee);
                SetGlobalFlag(821, true);
            }

            if ((GameSystems.Combat.IsCombatActive()))
            {
                return RunDefault;
            }

            if ((attachee.FindItemByName(4060) != null && attachee.GetLeader() == null))
            {
                var itemA = attachee.FindItemByName(4060);
                itemA.Destroy();
                Utilities.create_item_in_inventory(4177, attachee);
                Utilities.create_item_in_inventory(5011, attachee);
                Utilities.create_item_in_inventory(5011, attachee);
                Utilities.create_item_in_inventory(5011, attachee);
            }

            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            obj.TurnTowards(attachee); // added by Livonya
                            attachee.TurnTowards(obj); // added by Livonya
                            obj.BeginDialog(attachee, 1);
                        }

                    }

                }

            }

            // game.new_sid = 0			## removed by Livonya
            if ((GetGlobalVar(721) == 0))
            {
                attachee.CastSpell(WellKnownSpells.FoxsCunning, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(721) == 4))
            {
                attachee.CastSpell(WellKnownSpells.CatsGrace, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(721) == 8))
            {
                var loc = attachee.GetLocation();
                attachee.CastSpell(WellKnownSpells.Heroism, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(721) == 12))
            {
                attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(721, GetGlobalVar(721) + 1);
            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(164)))
            {
                return SkipDefault;
            }

            return RunDefault;
        }
        public static bool falrinth_escape(GameObject attachee, GameObject triggerer)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            SetGlobalVar(901, 1); // added by Gaear
            StartTimer(43200000, () => falrinth_return(attachee)); // 43200000ms is 12 hours
                                                                   // activeList = Co8PersistentData.getData(MINOR_GLOBE_OF_INVULNERABILITY_KEY)
                                                                   // if activeList == null:
                                                                   // print "ERROR! Active Globe spell without activeList!"
                                                                   // return
                                                                   // for entry in activeList:
                                                                   // spellID, target = entry
                                                                   // targetObj = refHandle(target)
                                                                   // if spellID == targetObj.id:
                                                                   // targetObj.spell_end( targetObj.id )
            return RunDefault;
        }
        public static bool falrinth_return(GameObject attachee)
        {
            // game.global_flags[164] = 0	## removed by Gaear
            attachee.PendingSpellsToMemorized(); // added by Livonya
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            AttachParticles("sp-Dimension Door", attachee);
            Sound(4018, 1);
            return RunDefault;
        }
        public static bool falrinth_well(GameObject attachee, GameObject pc)
        {
            var dice = Dice.Parse("1d10+1000");
            attachee.Heal(null, dice);
            attachee.HealSubdual(null, dice);
            return RunDefault;
        }

    }
}
