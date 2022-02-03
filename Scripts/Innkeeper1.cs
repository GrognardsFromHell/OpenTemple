
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
    [ObjectScript(73)]
    public class Innkeeper1 : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5008)) // upstairs - arranging for lodging for a PC
            {
                triggerer.BeginDialog(attachee, 420);
            }

            if ((attachee.GetMap() == 5006))
            {
                triggerer.BeginDialog(attachee, 350);
            }
            else if (((triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8003))) && ((GetQuestState(18) == QuestState.Unknown) || (GetQuestState(18) == QuestState.Mentioned) || (GetQuestState(18) == QuestState.Accepted))))
            {
                triggerer.BeginDialog(attachee, 200);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5007))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5006))
            {
                if ((GetGlobalVar(510) != 2))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public static bool set_room_flag(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(56, true);
            StartTimer(86390000, () => room_no_longer_available());
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static bool room_no_longer_available()
        {
            SetGlobalFlag(56, false);
            GameSystems.RandomEncounter.UpdateSleepStatus();
            return RunDefault;
        }
        public static int can_stay_behind(GameObject obj)
        {
            if (obj.type == ObjectType.pc)
            {
                return 1;
            }

            return 0;
        }
        public static void mark_pc_dropoff(GameObject obj)
        {
            obj.SetScriptId(ObjScriptEvent.Dialog, 439);
            ScriptDaemon.set_f("pc_dropoff");
        }
        public static bool contest_who(GameObject attachee)
        {
            foreach (var n in new[] { 8010, 8005, 8011, 8000 })
            {
                var npc = Utilities.find_npc_near(attachee, n);
                if ((npc != null))
                {
                    npc.FloatLine(300, attachee);
                }

            }

            return RunDefault;
        }
        public static bool contest_drink(GameObject attachee, GameObject triggerer)
        {
            var npcs_awake = 0;
            foreach (var n in new[] { 8009, 8010, 8005, 8011 })
            {
                var npc = Utilities.find_npc_near(attachee, n);
                if ((npc != null))
                {
                    if ((npc.GetStat(Stat.subdual_damage) < npc.GetStat(Stat.hp_current)))
                    {
                        npc.FloatLine(301, attachee);
                        npc.Damage(null, DamageType.Subdual, Dice.D3, D20AttackPower.NORMAL);
                        if ((npc.GetStat(Stat.subdual_damage) < npc.GetStat(Stat.hp_current)))
                        {
                            npcs_awake = npcs_awake + 1;

                        }

                    }

                }

            }

            var damage_dice = Dice.D3;
            var damage = damage_dice.Roll();
            if (((triggerer.GetStat(Stat.subdual_damage) + damage) >= triggerer.GetStat(Stat.hp_current)))
            {
                if ((npcs_awake == 0))
                {
                    attachee.FloatLine(310, triggerer);
                }
                else if ((npcs_awake == 1))
                {
                    attachee.FloatLine(300, triggerer);
                }
                else
                {
                    attachee.FloatLine(280, triggerer);
                }

            }
            else
            {
                damage_dice = Dice.Constant(0);
                damage_dice = damage_dice.WithModifier(damage);
                triggerer.Damage(null, DamageType.Subdual, damage_dice, D20AttackPower.NORMAL);
                if ((npcs_awake == 0))
                {
                    triggerer.BeginDialog(attachee, 290);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 270);
                }

            }

            return RunDefault;
        }

    }
}
