
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
    [ObjectScript(98)]
    public class Ophelia : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((!attachee.HasMet(triggerer)))
            {
                if ((GetQuestState(35) <= QuestState.Accepted))
                {
                    // if (game.quests[34].state <= qs_accepted):
                    triggerer.BeginDialog(attachee, 1);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 430);
                }

            }
            else if ((GetGlobalFlag(75)))
            {
                triggerer.BeginDialog(attachee, 580);
            }
            else if ((GetQuestState(35) == QuestState.Completed))
            {
                // elif (game.quests[34].state == qs_completed):
                triggerer.BeginDialog(attachee, 790);
            }
            else
            {
                triggerer.BeginDialog(attachee, 700);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(961) == 5))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        SetGlobalVar(961, 6);
                        attachee.TurnTowards(PartyLeader);
                        PartyLeader.BeginDialog(attachee, 420);
                    }

                }

            }

            return RunDefault;
        }
        public static bool buttin(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8015);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 760);
            }

            return SkipDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 15))
            {
                return true;
            }

            return false;
        }

    }
}
