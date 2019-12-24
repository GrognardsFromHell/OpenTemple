
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(97)]
    public class Otis : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(70, true);
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 350);
            }
            else if (((triggerer.FindItemByName(2202) != null) || (triggerer.FindItemByName(3008) != null)))
            {
                triggerer.BeginDialog(attachee, 330);
            }
            else if (((GetQuestState(32) == QuestState.Completed) && (!GetGlobalFlag(74))))
            {
                triggerer.BeginDialog(attachee, 270);
            }
            else if ((GetQuestState(31) == QuestState.Completed))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else if ((GetGlobalFlag(73)))
            {
                triggerer.BeginDialog(attachee, 120);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((triggerer.type == ObjectType.pc))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.RemoveFollower(attachee);
                }

                var elmo = Utilities.find_npc_near(attachee, 8000);

                if ((elmo != null))
                {
                    attachee.FloatLine(380, triggerer);
                    leader = elmo.GetLeader();

                    if ((leader != null))
                    {
                        leader.RemoveFollower(elmo);
                    }

                    elmo.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((!GetGlobalFlag(362)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            if (((obj.FindItemByName(2202) != null) || (obj.FindItemByName(3008) != null)))
                            {
                                obj.BeginDialog(attachee, 330);
                                SetGlobalFlag(362, true);
                                return RunDefault;
                            }

                        }

                    }

                }

                if ((!GetGlobalFlag(72)))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if ((obj.GetNameId() == 8000))
                        {
                            if ((Utilities.is_safe_to_talk(attachee, obj)))
                            {
                                var leader = obj.GetLeader();

                                if ((leader == null))
                                {
                                    leader = attachee.GetLeader();

                                }

                                if ((leader != null))
                                {
                                    leader.BeginDialog(attachee, 400);
                                    return RunDefault;
                                }

                            }

                        }

                    }

                }

                if ((GetGlobalFlag(366)))
                {
                    var leader = attachee.GetLeader();

                    if ((leader != null))
                    {
                        attachee.TurnTowards(leader);
                        attachee.FloatLine(12023, leader);
                        leader.RemoveFollower(attachee);
                        attachee.Attack(leader);
                        SetGlobalFlag(366, false);
                        return RunDefault;
                    }

                }

                if ((GetGlobalFlag(367)))
                {
                    var leader = attachee.GetLeader();

                    if ((leader != null))
                    {
                        attachee.TurnTowards(leader);
                        attachee.FloatLine(10014, leader);
                        leader.RemoveFollower(attachee);
                        SetGlobalFlag(367, false);
                        return RunDefault;
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var chest in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CONTAINER))
            {
                if ((chest.GetNameId() == 1202))
                {
                    chest.TransferItemByNameTo(attachee, 2202);
                    chest.TransferItemByNameTo(attachee, 3008);
                    attachee.WieldBestInAllSlots();
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in triggerer.GetPartyMembers())
            {
                if ((obj.GetNameId() == 8021))
                {
                    triggerer.RemoveFollower(obj);
                }

                if ((obj.GetNameId() == 8022))
                {
                    triggerer.RemoveFollower(obj);
                }

            }

            return RunDefault;
        }
        public static bool make_elmo_talk(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8000);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 410);
            }

            return SkipDefault;
        }
        public static bool talk_to_screng(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8021);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 470);
            }

            return SkipDefault;
        }
        public static bool switch_to_thrommel(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8031);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, 40);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 510);
            }

            return SkipDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (((attachee.GetMap() == 5066) || (attachee.GetMap() == 5067) || (attachee.GetMap() == 5078) || (attachee.GetMap() == 5079) || (attachee.GetMap() == 5080)))
            {
                SetGlobalFlag(73, true);
                if ((GetQuestState(31) == QuestState.Accepted))
                {
                    SetQuestState(31, QuestState.Completed);
                }

            }
            else if (((attachee.GetMap() == 5062) || (attachee.GetMap() == 5113) || (attachee.GetMap() == 5093)))
            {
                SetGlobalFlag(73, true);
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    if (((leader.GetAlignment() == Alignment.LAWFUL_EVIL) || (leader.GetAlignment() == Alignment.CHAOTIC_EVIL) || (leader.GetAlignment() == Alignment.NEUTRAL_EVIL)))
                    {
                        var percent = Utilities.group_percent_hp(leader);

                        if (((percent < 30) || (GetGlobalFlag(74))))
                        {
                            SetGlobalFlag(366, true);
                        }

                    }

                }

            }
            else if ((attachee.GetMap() == 5051))
            {
                if ((((GetGlobalFlag(73)) && (GetQuestState(31) == QuestState.Unknown)) || (GetQuestState(31) == QuestState.Completed)))
                {
                    var leader = attachee.GetLeader();

                    if ((leader != null))
                    {
                        SetGlobalFlag(367, true);
                    }

                }

            }

            return RunDefault;
        }


    }
}
