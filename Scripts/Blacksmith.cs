
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
    [ObjectScript(59)]
    public class Blacksmith : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var mycr = Utilities.find_container_near(attachee, 1053);
            if ((mycr != null && GetGlobalVar(705) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 12)) == 0))
            {
                triggerer.BeginDialog(attachee, 250);
            }

            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(979) && attachee.GetMap() == 5001)) // turns off original brother smyth
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(979)))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

                if ((GetGlobalFlag(875) && !GetGlobalFlag(876) && GetQuestState(99) != QuestState.Completed && !triggerer.GetPartyMembers().Any(o => o.HasItemByName(12900))))
                {
                    SetGlobalFlag(876, true);
                    StartTimer(140000000, () => amii_dies());
                }

                if ((StoryState >= 3) && ((GetGlobalVar(450) & Math.Pow(2, 0)) == 0) && ((GetGlobalVar(450) & Math.Pow(2, 12)) == 0) && !((GameSystems.Party.PartyMembers).Contains(attachee)))
                {
                    foreach (var chest in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CONTAINER))
                    {
                        if ((chest.GetNameId() == 1053))
                        {
                            return RunDefault;
                        }

                    }

                    var mycr = GameSystems.MapObject.CreateObject(1053, new locXY(572, 438));
                    mycr.Rotation = 2.5f;
                }

            }

            return RunDefault;
        }
        public static bool amii_dies()
        {
            SetQuestState(99, QuestState.Botched);
            SetGlobalFlag(862, true);
            return RunDefault;
        }

    }
}
