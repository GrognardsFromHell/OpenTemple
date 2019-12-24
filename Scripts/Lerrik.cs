
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
