
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
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(174)]
    public class Hedrack : BaseObjectScript
    {

        public override bool OnDialog(GameObject attachee, GameObject triggerer)
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
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(146, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(146, false);
            return RunDefault;
        }
        public override bool OnEndCombat(GameObject attachee, GameObject triggerer)
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
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if ((Utilities.obj_percent_hp(attachee) < 50))
            {
                GameObject found_pc = null;

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
        public static bool talk_Romag(GameObject attachee, GameObject triggerer, int line)
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
        public static bool summon_Iuz(GameObject attachee, GameObject triggerer)
        {
            var location = attachee.GetLocationFull();
            location.location.locx -= 4;

            GameSystems.MapObject.CreateObject(14266, location);
            return SkipDefault;
        }
        public static bool talk_Iuz(GameObject attachee, GameObject triggerer, int line)
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
        public static bool end_game(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(339, true);
            Utilities.set_join_slides(attachee, triggerer);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return SkipDefault;
        }
        public static bool give_robes(GameObject attachee, GameObject triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                Utilities.create_item_in_inventory(6113, pc);
            }

            return SkipDefault;
        }


    }
}
