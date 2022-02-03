
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
    [ObjectScript(148)]
    public class Deggum : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetGlobalFlag(144)))
            {
                // temple on alert
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                // haven't met
                triggerer.BeginDialog(attachee, 1);
            }
            else if ((GetGlobalFlag(165)))
            {
                triggerer.BeginDialog(attachee, 150);
            }
            else
            {
                triggerer.BeginDialog(attachee, 260);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(728, 0);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(374, true);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(779) == 0))
            {
                SetGlobalVar(779, 1);
                attachee.TurnTowards(PartyLeader);
            }
            else if ((GetGlobalVar(779) == 2 && GetGlobalFlag(825)))
            {
                attachee.RemoveFromInitiative();
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
            {
                if ((GetGlobalFlag(373)))
                {
                    if ((!Utilities.is_daytime()))
                    {
                        SetGlobalVar(742, GetGlobalVar(742) + 1);
                        if ((GetGlobalVar(742) == 3 && !GetGlobalFlag(147) && !GetGlobalFlag(990) && !GetGlobalFlag(823) && GetGlobalVar(779) == 1 && !GetGlobalFlag(825)))
                        {
                            var shocky_backup = GameSystems.MapObject.CreateObject(14233, new locXY(415, 528));
                            shocky_backup.Rotation = 3.14159265359f;
                            Sound(4035, 1);
                            AttachParticles("sp-Teleport", shocky_backup);
                            var deggsy = attachee.GetInitiative();
                            shocky_backup.AddToInitiative();
                            shocky_backup.SetInitiative(deggsy);
                            UiSystems.Combat.Initiative.UpdateIfNeeded();
                            foreach (var obj in ObjList.ListVicinity(shocky_backup.GetLocation(), ObjectListFilter.OLC_PC))
                            {
                                shocky_backup.Attack(obj);
                            }

                            SetGlobalFlag(823, true);
                        }

                    }

                }

                if ((Utilities.obj_percent_hp(attachee) <= 50))
                {
                    if ((GetGlobalVar(735) <= 3))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 443);
                        SetGlobalVar(735, GetGlobalVar(735) + 1);
                    }
                    else if ((GetGlobalVar(736) <= 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 225);
                        SetGlobalVar(736, GetGlobalVar(736) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 444);
                    }

                }
                else
                {
                    if ((GetGlobalVar(736) <= 9))
                    {
                        attachee.SetInt(obj_f.critter_strategy, 225);
                        SetGlobalVar(736, GetGlobalVar(736) + 1);
                    }
                    else
                    {
                        attachee.SetInt(obj_f.critter_strategy, 444);
                    }

                }

            }

            return RunDefault;
        }
        // SCRIPT DETAIL FOR START COMBAT							##
        // if not dead, unconscious, or prone						##
        // if under 50% health							##
        // if haven't used all 4 healing options (potions and spells)	##
        // set strategy to healing (potions and spells)		##
        // increment healing variable				##
        // otherwise, if haven't cast all 10 normal spells			##
        // set strategy to normal casting				##
        // increment normal casting variable			##
        // otherwise (if have cast all healing and normal spells)		##
        // set strategy to melee					##
        // otherwise (if over 50% health)						##
        // if haven't cast all 10 normal spells				##
        // set strategy to normal casting				##
        // increment normal casting variable			##
        // otheriwse (if have cast all normal spells)			##
        // set strategy to melee					##
        // run default									##

        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(374, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetGlobalFlag(374)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return SkipDefault;
                }

                var closest_jones = Utilities.party_closest(attachee);
                if ((attachee.DistanceTo(closest_jones) <= 100))
                {
                    SetGlobalVar(728, GetGlobalVar(728) + 1);
                    if ((attachee.GetLeader() == null))
                    {
                        if ((GetGlobalVar(728) == 4))
                        {
                            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                        if ((GetGlobalVar(728) == 8))
                        {
                            attachee.CastSpell(WellKnownSpells.Blur, attachee);
                            attachee.PendingSpellsToMemorized();
                        }

                    }

                    if ((GetGlobalVar(728) >= 400))
                    {
                        SetGlobalVar(728, 0);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                if ((GetGlobalFlag(144)))
                {
                    return RunDefault;
                }

            }

            return SkipDefault;
        }
        public static bool banter(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8035);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 90);
            }

            return SkipDefault;
        }
        public static bool banter2(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8035);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 80);
            }

            return SkipDefault;
        }

    }
}
