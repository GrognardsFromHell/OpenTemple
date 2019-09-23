
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

namespace Scripts
{
    [ObjectScript(489)]
    public class HextorInvaderAssassin : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetNameId() == 8755))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(511, GetGlobalVar(511) + 1);
                if ((GetGlobalVar(511) >= 24 && GetGlobalFlag(501)))
                {
                    SetGlobalFlag(511, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8756))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(512, GetGlobalVar(512) + 1);
                if ((GetGlobalVar(512) >= 24))
                {
                    SetGlobalFlag(512, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8757))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(513, GetGlobalVar(513) + 1);
                if ((GetGlobalVar(513) >= 24))
                {
                    SetGlobalFlag(513, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8758))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(514, GetGlobalVar(514) + 1);
                if ((GetGlobalVar(514) >= 24))
                {
                    SetGlobalFlag(514, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8759))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(515, GetGlobalVar(515) + 1);
                if ((GetGlobalVar(515) >= 24))
                {
                    SetGlobalFlag(515, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8749))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(516, GetGlobalVar(516) + 1);
                if ((GetGlobalVar(516) >= 12))
                {
                    SetGlobalFlag(516, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8750))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(517, GetGlobalVar(517) + 1);
                if ((GetGlobalVar(517) >= 12))
                {
                    SetGlobalFlag(517, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8751))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(518, GetGlobalVar(518) + 1);
                if ((GetGlobalVar(518) >= 12))
                {
                    SetGlobalFlag(518, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8752))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(519, GetGlobalVar(519) + 1);
                if ((GetGlobalVar(519) >= 12))
                {
                    SetGlobalFlag(519, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8760))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(520, GetGlobalVar(520) + 1);
                if ((GetGlobalVar(520) >= 5))
                {
                    SetGlobalFlag(520, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8761))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(521, GetGlobalVar(521) + 1);
                if ((GetGlobalVar(521) >= 6))
                {
                    SetGlobalFlag(521, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            if ((attachee.GetNameId() == 8762))
            {
                destroy_gear(attachee, triggerer);
                SetGlobalVar(522, GetGlobalVar(522) + 1);
                if ((GetGlobalVar(522) >= 6))
                {
                    SetGlobalFlag(522, true);
                    if ((GetGlobalFlag(511) && GetGlobalFlag(512) && GetGlobalFlag(513) && GetGlobalFlag(514) && GetGlobalFlag(515) && GetGlobalFlag(516) && GetGlobalFlag(517) && GetGlobalFlag(518) && GetGlobalFlag(519) && GetGlobalFlag(520) && GetGlobalFlag(521) && GetGlobalFlag(522)))
                    {
                        SetQuestState(97, QuestState.Completed);
                        PartyLeader.AddReputation(52);
                        SetGlobalVar(501, 7);
                    }
                    else
                    {
                        Sound(4132, 2);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(505) == 0))
            {
                StartTimer(7200000, () => out_of_time(attachee, triggerer)); // 2 hours
                SetGlobalVar(505, 1);
            }

            if ((!attachee.HasEquippedByName(4500) || !attachee.HasEquippedByName(4126)))
            {
                attachee.WieldBestInAllSlots();
            }

            if ((triggerer.type == ObjectType.pc))
            {
                if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8736)))
                {
                    var wakefield = Utilities.find_npc_near(triggerer, 8736);
                    if ((wakefield != null))
                    {
                        triggerer.RemoveFollower(wakefield);
                        wakefield.FloatLine(20000, triggerer);
                        wakefield.Attack(triggerer);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(97) == QuestState.Botched))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
            {
                if ((GameSystems.Combat.IsCombatActive()))
                {
                    if ((!attachee.HasEquippedByName(4161) || !attachee.HasEquippedByName(4245)))
                    {
                        attachee.WieldBestInAllSlots();
                    }

                }

            }

            // game.new_sid = 0
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(525)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }
        public static void destroy_gear(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var assassin_mithrilshirt = attachee.FindItemByName(6315);
            assassin_mithrilshirt.Destroy();
            var assassin_rapier = attachee.FindItemByName(4500);
            assassin_rapier.Destroy();
            var assassin_shortsword = attachee.FindItemByName(4126);
            assassin_shortsword.Destroy();
            var assassin_glovesdex = attachee.FindItemByName(6200);
            assassin_glovesdex.Destroy();
            return;
        }
        public static void out_of_time(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalVar(505, 3);
            return;
        }

    }
}
