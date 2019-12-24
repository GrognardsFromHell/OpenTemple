
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

namespace Scripts
{
    [ObjectScript(329)]
    public class Rakham : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(109) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 430);
            }
            else if ((GetGlobalFlag(537)))
            {
                triggerer.BeginDialog(attachee, 400);
            }
            else if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 180);
            }
            else if ((GetGlobalFlag(536)))
            {
                triggerer.BeginDialog(attachee, 420);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5169 && GetGlobalVar(549) == 3))
            {
                if ((attachee.GetLeader() == null))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5172 && GetGlobalVar(549) == 3))
            {
                if ((attachee.GetLeader() == null))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5141))
            {
                if ((GetQuestState(109) == QuestState.Completed && GetGlobalVar(542) == 3))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((Utilities.is_daytime()))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((!Utilities.is_daytime()))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                pc.AddCondition("fallen_paladin");
            }

            SetGlobalFlag(539, true);
            return RunDefault;
        }

        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(537)))
            {
                var leader = PartyLeader;
                Co8.StopCombat(attachee, 0);
                leader.BeginDialog(attachee, 400);
            }

            return SkipDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(539, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        StartTimer(2000, () => howdy_ho(attachee, triggerer));
                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.CauseFear || spell.spellEnum == WellKnownSpells.Fear))
            {
                SetGlobalFlag(537, true);
                StartTimer(3600000, () => revert_ggf_537(attachee, triggerer));
            }

            return RunDefault;
        }
        public static void increment_var_536(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!attachee.HasMet(triggerer)))
            {
                SetGlobalVar(536, GetGlobalVar(536) + 1);
            }

            return;
        }
        public static void increment_var_543(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(543, GetGlobalVar(543) + 1);
            return;
        }
        public static void increment_var_544(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(544, GetGlobalVar(544) + 1);
            return;
        }
        public static void increment_var_545(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(545, GetGlobalVar(545) + 1);
            return;
        }
        public void increment_var_555(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(555, GetGlobalVar(555) + 1);
            DetachScript();
            return;
        }
        public void increment_var_556(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(556, GetGlobalVar(556) + 1);
            DetachScript();
            return;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.HasLineOfSight(listener)))
            {
                if ((speaker.DistanceTo(listener) <= 40))
                {
                    return true;
                }

            }

            return false;
        }
        public static bool howdy_ho(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 10);
            return RunDefault;
        }
        public static void gen_panathaes_loc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(539) == 0))
            {
                var chooser = RandomRange(1, 8);
                if ((chooser == 1))
                {
                    SetGlobalVar(539, 1);
                }
                else if ((chooser == 2))
                {
                    SetGlobalVar(539, 2);
                }
                else if ((chooser == 3))
                {
                    SetGlobalVar(539, 3);
                }
                else if ((chooser == 4))
                {
                    SetGlobalVar(539, 4);
                }
                else if ((chooser == 5))
                {
                    SetGlobalVar(539, 5);
                }
                else if ((chooser == 6))
                {
                    SetGlobalVar(539, 6);
                }
                else if ((chooser == 7))
                {
                    SetGlobalVar(539, 7);
                }
                else if ((chooser == 8))
                {
                    SetGlobalVar(539, 8);
                }

            }

            return;
        }
        public static void pick_kidnapper(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(542) == 0))
            {
                var picker = RandomRange(1, 4);
                if ((picker == 1))
                {
                    SetGlobalVar(542, 1);
                }
                // boroquin is kidnapper
                else if ((picker == 2 || picker == 3))
                {
                    SetGlobalVar(542, 2);
                }
                // panathaes is kidnapper
                else if ((picker == 4))
                {
                    SetGlobalVar(542, 3);
                }

            }

            // rakham is kidnapper
            return;
        }
        public static void gen_kids_loc(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(540) == 0 && GetGlobalVar(541) == 0))
            {
                var chooser = RandomRange(1, 4);
                if ((chooser == 1))
                {
                    SetGlobalVar(540, 1);
                    SetGlobalVar(541, 1);
                }
                else if ((chooser == 2))
                {
                    SetGlobalVar(540, 2);
                    SetGlobalVar(541, 2);
                }
                else if ((chooser == 3))
                {
                    SetGlobalVar(540, 3);
                    SetGlobalVar(541, 3);
                }
                else if ((chooser == 4))
                {
                    SetGlobalVar(540, 4);
                    SetGlobalVar(541, 4);
                }

            }

            return;
        }
        public static void check_for_locket(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(542) == 3))
            {
                Utilities.create_item_in_inventory(11061, attachee);
            }

            return;
        }
        public static void check_evidence_rep_bor(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(72)))
            {
                PartyLeader.AddReputation(75);
                PartyLeader.RemoveReputation(72);
            }
            else if ((PartyLeader.HasReputation(69)))
            {
                PartyLeader.AddReputation(72);
                PartyLeader.RemoveReputation(69);
            }
            else if ((!PartyLeader.HasReputation(69)))
            {
                if ((!PartyLeader.HasReputation(72)))
                {
                    if ((!PartyLeader.HasReputation(75)))
                    {
                        PartyLeader.AddReputation(69);
                    }

                }

            }

            return;
        }
        public static void check_evidence_rep_pan(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(73)))
            {
                PartyLeader.AddReputation(76);
                PartyLeader.RemoveReputation(73);
            }
            else if ((PartyLeader.HasReputation(70)))
            {
                PartyLeader.AddReputation(73);
                PartyLeader.RemoveReputation(70);
            }
            else if ((!PartyLeader.HasReputation(70)))
            {
                if ((!PartyLeader.HasReputation(73)))
                {
                    if ((!PartyLeader.HasReputation(76)))
                    {
                        PartyLeader.AddReputation(70);
                    }

                }

            }

            return;
        }
        public static void check_evidence_rep_rak(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((PartyLeader.HasReputation(74)))
            {
                PartyLeader.AddReputation(77);
                PartyLeader.RemoveReputation(74);
            }
            else if ((PartyLeader.HasReputation(71)))
            {
                PartyLeader.AddReputation(74);
                PartyLeader.RemoveReputation(71);
            }
            else if ((!PartyLeader.HasReputation(71)))
            {
                if ((!PartyLeader.HasReputation(74)))
                {
                    if ((!PartyLeader.HasReputation(77)))
                    {
                        PartyLeader.AddReputation(71);
                    }

                }

            }

            return;
        }
        public static void revert_ggf_537(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(537, false);
            return;
        }

    }
}
