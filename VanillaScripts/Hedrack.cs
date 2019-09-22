
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
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(174)]
    public class Hedrack : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(144)))
            {
                if ((!attachee.HasMet(triggerer)))
                {
                    triggerer.BeginDialog(attachee, 10);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 290);
                }

            }
            else if ((GetQuestState(58) >= QuestState.Accepted))
            {
                triggerer.BeginDialog(attachee, 480);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 490);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((Utilities.is_safe_to_talk(attachee, obj)))
                {
                    if ((GetQuestState(58) != QuestState.Unknown))
                    {
                        DetachScript();

                        return SkipDefault;
                    }
                    else if ((GetQuestState(54) == QuestState.Completed) && (!attachee.HasMet(obj)))
                    {
                        obj.BeginDialog(attachee, 40);
                    }
                    else if ((GetQuestState(51) == QuestState.Completed) && (!attachee.HasMet(obj)))
                    {
                        obj.BeginDialog(attachee, 30);
                    }
                    else if ((GetQuestState(45) == QuestState.Completed) && (!attachee.HasMet(obj)))
                    {
                        obj.BeginDialog(attachee, 20);
                    }
                    else if ((GetGlobalFlag(144)))
                    {
                        if ((!attachee.HasMet(obj)))
                        {
                            obj.BeginDialog(attachee, 10);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 290);
                        }

                    }
                    else if ((GetQuestState(58) >= QuestState.Accepted))
                    {
                        obj.BeginDialog(attachee, 480);
                    }
                    else if ((attachee.HasMet(obj)))
                    {
                        obj.BeginDialog(attachee, 490);
                    }
                    else
                    {
                        obj.BeginDialog(attachee, 1);
                    }

                    DetachScript();

                }

            }

            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        if ((GetQuestState(54) == QuestState.Completed))
                        {
                            obj.BeginDialog(attachee, 40);
                        }
                        else if ((GetQuestState(51) == QuestState.Completed))
                        {
                            obj.BeginDialog(attachee, 30);
                        }
                        else if ((GetQuestState(45) == QuestState.Completed))
                        {
                            obj.BeginDialog(attachee, 20);
                        }
                        else
                        {
                            obj.BeginDialog(attachee, 1);
                        }

                        DetachScript();

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(146, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(146, false);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var npc = Utilities.find_npc_near(attachee, 8001);

            if ((npc != null))
            {
                SetGlobalFlag(325, true);
            }

            npc = Utilities.find_npc_near(attachee, 8059);

            if ((npc != null))
            {
                SetGlobalFlag(325, true);
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((Utilities.obj_percent_hp(attachee) < 50))
            {
                GameObjectBody found_pc = null;

                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    if (pc.type == ObjectType.pc)
                    {
                        found_pc = pc;

                        attachee.AIRemoveFromShitlist(pc);
                    }

                }

                if (found_pc != null)
                {
                    found_pc.BeginDialog(attachee, 190);
                    DetachScript();

                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public static bool talk_Romag(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8037);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 520);
            }

            return SkipDefault;
        }
        public static bool summon_Iuz(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var location = attachee.GetLocationFull();
            location.location.locx -= 4;

            GameSystems.MapObject.CreateObject(14266, location);
            return SkipDefault;
        }
        public static bool talk_Iuz(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8037);

            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 30);
            }

            return SkipDefault;
        }
        public static bool end_game(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(339, true);
            Utilities.set_join_slides(attachee, triggerer);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return SkipDefault;
        }
        public static bool give_robes(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                Utilities.create_item_in_inventory(6113, pc);
            }

            return SkipDefault;
        }


    }
}
