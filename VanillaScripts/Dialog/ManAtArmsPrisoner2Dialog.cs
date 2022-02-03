
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

[DialogScript(137)]
public class ManAtArmsPrisoner2Dialog : ManAtArmsPrisoner2, IDialogScript
{
    public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 71:
            case 72:
            case 73:
            case 74:
            case 131:
            case 132:
                originalScript = "(npc.leader_get() == OBJ_HANDLE_NULL)";
                return (npc.GetLeader() == null);
            case 75:
            case 76:
                originalScript = "(npc.leader_get() != OBJ_HANDLE_NULL)";
                return (npc.GetLeader() != null);
            case 133:
            case 134:
                originalScript = "(npc.leader_get() != OBJ_HANDLE_NULL) and game.global_flags[138] == 1";
                return (npc.GetLeader() != null) && GetGlobalFlag(138);
            case 135:
            case 136:
                originalScript = "(npc.leader_get() != OBJ_HANDLE_NULL) and game.global_flags[138] == 0";
                return (npc.GetLeader() != null) && !GetGlobalFlag(138);
            default:
                originalScript = null;
                return true;
        }
    }
    public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
    {
        switch (lineNumber)
        {
            case 151:
            case 152:
                originalScript = "pc.follower_remove(npc)";
                pc.RemoveFollower(npc);
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