
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
    [ObjectScript(117)]
    public class Lodriss : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            // Respawn
            if ((!GetGlobalFlag(912)))
            {
                StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
                SetGlobalFlag(912, true);
            }

            // The 'evac' routine
            if (ScriptDaemon.get_f("boatmens_tavern_evac_on") && SelectedPartyLeader.GetMap() == 5052)
            {
                StartTimer(300, () => ScriptDaemon.set_f("boatmens_tavern_evac_on", false));
                if (attachee.GetNameId() == 14133 && !ScriptDaemon.get_f("lodriss_killed_outside")) // For Lodriss
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    // else : # For the others
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            // game.new_sid = 0
            if (attachee.GetNameId() == 14152 && SelectedPartyLeader.GetMap() == 5051) // Tolub, outside
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(369, true);
            if (GetQuestState(42) >= QuestState.Mentioned)
            {
                SetQuestState(42, QuestState.Completed);
                triggerer.AddReputation(21);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (GetQuestState(42) == QuestState.Completed)
            {
                SetQuestState(42, QuestState.Botched);
                triggerer.RemoveReputation(21);
            }

            SetGlobalFlag(369, false);
            return RunDefault;
        }
        public static bool kill_lodriss(GameObjectBody attachee)
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
            ScriptDaemon.set_f("lodriss_killed_outside");
            return RunDefault;
        }
        public static bool kill_skole(GameObjectBody attachee)
        {
            StartTimer(86400000, () => skole_dead(attachee));
            return RunDefault;
        }
        public static bool skole_dead(GameObjectBody attachee)
        {
            SetGlobalFlag(201, true);
            return RunDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 1);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }
        public static bool make_like(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetReaction(triggerer) <= 71))
            {
                attachee.SetReaction(triggerer, 71);
            }

            return SkipDefault;
        }
        public static int check_skole(GameObjectBody attachee)
        {
            var skole = Utilities.find_npc_near(attachee, 14134);
            if ((skole == null))
            {
                return 0;
            }

            var skole_whacked = 0;
            if ((skole.GetStat(Stat.subdual_damage) > 0 || Utilities.obj_percent_hp(skole) < 100 || skole.GetLeader() != null))
            {
                skole_whacked = 1;
            }

            if ((skole_whacked == 0 && (GetGlobalFlag(202) || GetGlobalFlag(102))))
            {
                return 1;
            }

            return 0;
        }
        public static bool evac_partial(GameObjectBody attachee)
        {
            ScriptDaemon.set_f("boatmens_tavern_evac_on");
            attachee.SetScriptId(ObjScriptEvent.FirstHeartbeat, 117);
            attachee.SetObjectFlag(ObjectFlag.OFF);
            return RunDefault;
        }
        public static bool evac(GameObjectBody attachee)
        {
            ScriptDaemon.set_f("boatmens_tavern_evac_on");
            attachee.SetScriptId(ObjScriptEvent.FirstHeartbeat, 117); // san_first_heartbeat
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                if ((obj.GetLeader() == null))
                {
                    if ((obj.GetNameId() == 14133 || obj.GetNameId() == 8020 || obj.GetNameId() == 8057 || obj.GetNameId() == 14374 || obj.GetNameId() == 14290 || obj.GetNameId() == 14372 || obj.GetNameId() == 14152))
                    {
                        obj.SetScriptId(ObjScriptEvent.FirstHeartbeat, 117);
                        obj.SetObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

            return RunDefault;
        }
        public static void respawn(GameObjectBody attachee)
        {
            var box = Utilities.find_container_near(attachee, 1004);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(604800000, () => respawn(attachee)); // 604800000ms is 1 week
            return;
        }

    }
}
