
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
    [ObjectScript(382)]
    public class Lerrik : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetQuestState(77) == QuestState.Completed && GetGlobalFlag(992) && !GetGlobalFlag(935)))
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 360);
            }
            else
            {
                attachee.TurnTowards(triggerer);
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(974)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5170 && GetQuestState(77) == QuestState.Completed && GetGlobalFlag(992) && !GetGlobalFlag(935)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((attachee.GetMap() == 5169 && GetQuestState(77) == QuestState.Completed && GetGlobalFlag(992) && !GetGlobalFlag(935)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(974, true);
            if ((GetQuestState(77) == QuestState.Completed && GetGlobalFlag(992) && !GetGlobalFlag(935)))
            {
                PartyLeader.AddReputation(43);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(974, false);
            return RunDefault;
        }
        public static int party_spot_check()
        {
            var highest_spot = -999;
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if (pc.GetSkillLevel(SkillId.spot) > highest_spot)
                {
                    highest_spot = pc.GetSkillLevel(SkillId.spot);
                }

            }

            return highest_spot;
        }

    }
}
