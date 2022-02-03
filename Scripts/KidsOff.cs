
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

namespace Scripts;

[ObjectScript(228)]
public class KidsOff : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(triggerer);
        if ((attachee.GetNameId() == 8090))
        {
            if ((attachee.GetMap() == 5042))
            {
                var jaroo = Utilities.find_npc_near(attachee, 20001);
                triggerer.BeginDialog(jaroo, 1);
                return SkipDefault;
            }
            else if ((attachee.GetMap() == 5022 || attachee.GetMap() == 5001))
            {
                triggerer.BeginDialog(attachee, 2000);
                return SkipDefault;
            }

        }
        else if ((attachee.GetNameId() == 8068))
        {
            if (((GetQuestState(106) == QuestState.Mentioned || GetQuestState(106) == QuestState.Completed) && GetQuestState(95) != QuestState.Completed && !GetGlobalFlag(378)))
            {
                triggerer.BeginDialog(attachee, 1500);
            }
            else
            {
                var r = RandomRange(1, 10);
                if ((r == 1))
                {
                    triggerer.BeginDialog(attachee, 1);
                }
                else if ((r == 2))
                {
                    triggerer.BeginDialog(attachee, 10);
                }
                else if ((r == 3))
                {
                    triggerer.BeginDialog(attachee, 20);
                }
                else if ((r == 4))
                {
                    triggerer.BeginDialog(attachee, 30);
                }
                else if ((r == 5))
                {
                    triggerer.BeginDialog(attachee, 40);
                }
                else if ((r == 6))
                {
                    triggerer.BeginDialog(attachee, 50);
                }
                else if ((r == 7))
                {
                    triggerer.BeginDialog(attachee, 60);
                }
                else if ((r == 8))
                {
                    triggerer.BeginDialog(attachee, 70);
                }
                else if ((r == 9))
                {
                    triggerer.BeginDialog(attachee, 80);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 90);
                }

            }

        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 8090))
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetMap() == 5042))
                {
                    if ((GetQuestState(99) == QuestState.Accepted))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }
                    else if ((GetGlobalFlag(862)))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                    }

                }
                else if ((attachee.GetMap() == 5022))
                {
                    if ((GetQuestState(99) == QuestState.Completed))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }

            }

        }

        return RunDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetNameId() == 14501))
        {
            SetGlobalFlag(862, true);
        }

        SetGlobalVar(23, GetGlobalVar(23) + 1);
        if ((GetGlobalVar(23) >= 1))
        {
            PartyLeader.AddReputation(92);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetNameId() == 14501))
        {
            SetGlobalFlag(862, false);
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
        {
            attachee.SetObjectFlag(ObjectFlag.OFF);
        }
        else
        {
            attachee.ClearObjectFlag(ObjectFlag.OFF);
            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((obj.DistanceTo(attachee) <= 30 && GetGlobalVar(702) == 0 && !Utilities.critter_is_unconscious(obj)))
                {
                    if ((obj.GetRace() == RaceId.tallfellow))
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 500);
                    }
                    else if ((obj.GetRace() == RaceId.half_orc))
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 600);
                    }
                    else if ((obj.GetStat(Stat.level_paladin) >= 1))
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 200);
                    }
                    else if ((obj.GetStat(Stat.level_wizard) >= 1))
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 300);
                    }
                    else if ((obj.GetStat(Stat.level_bard) >= 1))
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 400);
                    }
                    else
                    {
                        SetGlobalVar(702, 1);
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 100);
                    }

                }

            }

        }

        return RunDefault;
    }
    public static bool talk_nps(GameObject attachee, GameObject triggerer)
    {
        var npc1 = Utilities.find_npc_near(triggerer, 8002);
        if ((npc1 != null))
        {
            triggerer.BeginDialog(npc1, 5000);
            npc1.TurnTowards(attachee);
        }

        var npc2 = Utilities.find_npc_near(triggerer, 14037);
        if ((npc2 != null))
        {
            triggerer.BeginDialog(npc2, 5000);
            npc2.TurnTowards(triggerer);
        }

        var npc3 = Utilities.find_npc_near(triggerer, 8050);
        if ((npc3 != null))
        {
            triggerer.BeginDialog(npc3, 5000);
            npc3.TurnTowards(triggerer);
        }

        var npc4 = Utilities.find_npc_near(triggerer, 8062);
        if ((npc4 != null))
        {
            triggerer.BeginDialog(npc4, 5000);
            npc4.TurnTowards(triggerer);
        }

        var npc5 = Utilities.find_npc_near(triggerer, 8010);
        if ((npc5 != null))
        {
            triggerer.BeginDialog(npc5, 5000);
            npc5.TurnTowards(triggerer);
        }

        var npc6 = Utilities.find_npc_near(triggerer, 8072);
        if ((npc6 != null))
        {
            triggerer.BeginDialog(npc6, 5000);
            npc6.TurnTowards(triggerer);
        }

        var npc7 = Utilities.find_npc_near(triggerer, 8015);
        if ((npc7 != null))
        {
            triggerer.BeginDialog(npc7, 5000);
            npc7.TurnTowards(triggerer);
        }

        var npc8 = Utilities.find_npc_near(triggerer, 8003);
        if ((npc8 != null))
        {
            triggerer.BeginDialog(npc8, 5000);
            npc8.TurnTowards(triggerer);
        }

        var npc9 = Utilities.find_npc_near(triggerer, 8004);
        if ((npc9 != null))
        {
            triggerer.BeginDialog(npc9, 5000);
            npc9.TurnTowards(triggerer);
        }

        return SkipDefault;
    }

}