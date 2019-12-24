
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace VanillaScripts.Dialog
{
    [DialogScript(136)]
    public class ManAtArmsPrisoner1Dialog : ManAtArmsPrisoner1, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
                case 52:
                case 153:
                case 154:
                    originalScript = "game.party_npc_size() == 0";
                    return GameSystems.Party.NPCFollowersSize == 0;
                case 55:
                case 56:
                    originalScript = "game.party_npc_size() >= 1";
                    return GameSystems.Party.NPCFollowersSize >= 1;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 4:
                case 5:
                case 81:
                case 82:
                    originalScript = "game.global_flags[133] = 1; get_rep( npc, pc )";
                    SetGlobalFlag(133, true);
                    get_rep(npc, pc);
                    ;
                    break;
                case 120:
                case 171:
                case 181:
                case 182:
                    originalScript = "pc.follower_remove(npc)";
                    pc.RemoveFollower(npc);
                    break;
                case 140:
                    originalScript = "pc.follower_add(npc)";
                    pc.AddFollower(npc);
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
}
