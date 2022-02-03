
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog;

[DialogScript(69)]
public class SpinsterDialog : Spinster, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 4:
            case 5:
                originalScript = "game.quests[12].state == qs_accepted and game.global_flags[43] == 1 and pc.stat_level_get( stat_gender ) == gender_male";
                return GetQuestState(12) == QuestState.Accepted && GetGlobalFlag(43) && pc.GetGender() == Gender.Male;
            case 6:
            case 7:
                originalScript = "game.quests[12].state == qs_accepted and game.global_flags[43] == 0 and pc.stat_level_get( stat_gender ) == gender_male";
                return GetQuestState(12) == QuestState.Accepted && !GetGlobalFlag(43) && pc.GetGender() == Gender.Male;
            case 13:
            case 14:
                originalScript = "game.global_flags[26] == 1 and quest and game.quests[12].state == qs_unknown";
                return GetGlobalFlag(26) && GetQuestState(12) == QuestState.Unknown;
            case 15:
                originalScript = "pc.stat_level_get(stat_charisma) <= 8 and pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetStat(Stat.charisma) <= 8 && pc.GetGender() == Gender.Male;
            case 16:
                originalScript = "pc.stat_level_get(stat_charisma) >= 14 and pc.stat_level_get( stat_gender ) == gender_male";
                return pc.GetStat(Stat.charisma) >= 14 && pc.GetGender() == Gender.Male;
            case 53:
            case 54:
                originalScript = "game.quests[12].state == qs_accepted and game.global_flags[42] == 0 and game.global_flags[43] == 0 and pc.stat_level_get( stat_gender ) == gender_male";
                return GetQuestState(12) == QuestState.Accepted && !GetGlobalFlag(42) && !GetGlobalFlag(43) && pc.GetGender() == Gender.Male;
            case 55:
            case 56:
                originalScript = "game.global_flags[43] == 1 and game.global_flags[42] == 0 and pc.stat_level_get( stat_gender ) == gender_male";
                return GetGlobalFlag(43) && !GetGlobalFlag(42) && pc.GetGender() == Gender.Male;
            case 57:
            case 58:
                originalScript = "game.global_flags[43] == 1 and game.global_flags[42] == 1 and pc.stat_level_get( stat_gender ) == gender_male and not pc.follower_atmax()";
                return GetGlobalFlag(43) && GetGlobalFlag(42) && pc.GetGender() == Gender.Male && !pc.HasMaxFollowers();
            case 59:
            case 60:
                originalScript = "game.global_flags[43] == 0 and game.global_flags[42] == 1";
                return !GetGlobalFlag(43) && GetGlobalFlag(42);
            case 61:
            case 62:
                originalScript = "game.global_flags[43] == 1 and game.global_flags[42] == 1 and pc.stat_level_get( stat_gender ) == gender_male and pc.follower_atmax()";
                return GetGlobalFlag(43) && GetGlobalFlag(42) && pc.GetGender() == Gender.Male && pc.HasMaxFollowers();
            case 81:
            case 82:
                originalScript = "not pc.follower_atmax()";
                return !pc.HasMaxFollowers();
            case 83:
            case 84:
                originalScript = "pc.follower_atmax()";
                return pc.HasMaxFollowers();
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 70:
                originalScript = "game.global_flags[42] = 1";
                SetGlobalFlag(42, true);
                break;
            case 101:
            case 102:
                originalScript = "pc.follower_add( npc ); game.quests[12].state = qs_completed";
                pc.AddFollower(npc);
                SetQuestState(12, QuestState.Completed);
                ;
                break;
            default:
                originalScript = null;
                return;
        }
    }
    public bool TryGetSkillChecks(int lineNumber, out DialogSkillChecks skillChecks)
    {
        switch (lineNumber)
        {
            default:
                skillChecks = default;
                return false;
        }
    }
}