
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
    [ObjectScript(60)]
    public class Lareth : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(198)))
            {
                triggerer.BeginDialog(attachee, 260);
            }
            else if ((attachee.GetLeader() != null))
            {
                if ((GetGlobalFlag(53)))
                {
                    triggerer.BeginDialog(attachee, 320);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 220);
                }

            }
            else if ((GetGlobalFlag(52)))
            {
                triggerer.BeginDialog(attachee, 20);
            }
            else if ((GetGlobalFlag(48)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            SetGlobalFlag(37, true);
            if ((StoryState <= 1))
            {
                StoryState = 2;

            }

            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.HasReputation(18)))
                {
                    pc.RemoveReputation(18);
                }

            }

            PartyLeader.AddReputation(15);
            if ((GetGlobalFlag(340)))
            {
                var new_lareth = GameSystems.MapObject.CreateObject(14060, attachee.GetLocation());

                new_lareth.SetObjectFlag(ObjectFlag.DONTDRAW);
                foreach (var obj in ObjList.ListVicinity(new_lareth.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(new_lareth, obj)))
                    {
                        obj.BeginDialog(new_lareth, 370);
                        return RunDefault;
                    }

                }

            }
            else if ((!GetGlobalFlag(62)))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        obj.BeginDialog(attachee, 390);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDisband(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            foreach (var npc in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                foreach (var pc in GameSystems.Party.PartyMembers)
                {
                    npc.AIRemoveFromShitlist(pc);
                    npc.SetReaction(pc, 50);
                }

            }

            return RunDefault;
        }
        public override bool OnJoin(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((StoryState <= 1))
            {
                StoryState = 2;

            }

            if ((GetGlobalFlag(340)))
            {
                triggerer.BeginDialog(attachee, 380);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(37, false);
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
                    attachee.AdjustReaction(found_pc, +100);
                    found_pc.BeginDialog(attachee, 160);
                    DetachScript();

                    return SkipDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnNewMap(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5065))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 270);
                }

            }

            if (((attachee.GetMap() == 5113) && (!GetGlobalFlag(200))))
            {
                SetGlobalFlag(200, true);
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 290);
                }

            }

            if ((attachee.GetMap() == 5066))
            {
                var leader = attachee.GetLeader();

                if ((leader != null))
                {
                    leader.BeginDialog(attachee, 300);
                }

            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                attachee.AIRemoveFromShitlist(pc);
                attachee.SetReaction(pc, 50);
            }

            attachee.RunOff();
            return RunDefault;
        }
        public static bool demo_end_game(GameObjectBody attachee, GameObjectBody triggerer)
        {
            GameSystems.Movies.MovieQueueAdd(269);
            GameSystems.Movies.MovieQueuePlayAndEndGame();
            return RunDefault;
        }


    }
}
