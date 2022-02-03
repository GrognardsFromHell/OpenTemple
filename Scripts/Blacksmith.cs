
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
    [ObjectScript(59)]
    public class Blacksmith : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            var mycr = Utilities.find_container_near(attachee, 1053);
            if ((mycr != null && GetGlobalVar(705) == 0) && !Co8Settings.DisableNewPlots && !Co8Settings.DisableArenaOfHeroes)
            {
                triggerer.BeginDialog(attachee, 250);
            }

            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(979) && attachee.GetMap() == 5001)) // turns off original brother smyth
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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

                if ((StoryState >= 3) && !Co8Settings.DisableNewPlots && !Co8Settings.DisableArenaOfHeroes && !((GameSystems.Party.PartyMembers).Contains(attachee)))
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
