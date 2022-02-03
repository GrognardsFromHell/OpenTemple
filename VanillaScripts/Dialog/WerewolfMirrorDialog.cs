
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

[DialogScript(150)]
public class WerewolfMirrorDialog : WerewolfMirror, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 2:
            case 4:
                originalScript = "game.party_alignment != LAWFUL_EVIL and game.party_alignment != NEUTRAL_EVIL and game.party_alignment != CHAOTIC_EVIL";
                return PartyAlignment != Alignment.LAWFUL_EVIL && PartyAlignment != Alignment.NEUTRAL_EVIL && PartyAlignment != Alignment.CHAOTIC_EVIL;
            case 3:
            case 5:
                originalScript = "game.party_alignment == LAWFUL_EVIL or game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                return PartyAlignment == Alignment.LAWFUL_EVIL || PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
            case 25:
            case 26:
            case 71:
            case 72:
                originalScript = "pc.skill_level_get(npc, skill_sense_motive) >= 10";
                return pc.GetSkillLevel(npc, SkillId.sense_motive) >= 10;
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 1:
                originalScript = "game.global_flags[154] = 1";
                SetGlobalFlag(154, true);
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
            case 25:
            case 26:
            case 71:
            case 72:
                skillChecks = new DialogSkillChecks(SkillId.sense_motive, 10);
                return true;
            default:
                skillChecks = default;
                return false;
        }
    }
}