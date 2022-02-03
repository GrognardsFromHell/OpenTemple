
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

[DialogScript(203)]
public class NodeGlabrezuDialog : NodeGlabrezu, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 11:
            case 13:
                originalScript = "pc.stat_level_get(stat_race) == race_dwarf or pc.stat_level_get(stat_race) == race_halfling or pc.stat_level_get(stat_race) == race_gnome";
                return pc.GetRace() == RaceId.derro || pc.GetRace() == RaceId.tallfellow || pc.GetRace() == RaceId.svirfneblin;
            case 12:
            case 14:
                originalScript = "pc.stat_level_get(stat_race) != race_dwarf and pc.stat_level_get(stat_race) != race_halfling and pc.stat_level_get(stat_race) != race_gnome";
                return pc.GetRace() != RaceId.derro && pc.GetRace() != RaceId.tallfellow && pc.GetRace() != RaceId.svirfneblin;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 51:
                originalScript = "npc.attack(pc)";
                npc.Attack(pc);
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