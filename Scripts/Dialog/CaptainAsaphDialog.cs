
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

namespace Scripts.Dialog
{
    [DialogScript(322)]
    public class CaptainAsaphDialog : CaptainAsaph, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 41:
                case 42:
                case 61:
                case 62:
                case 71:
                case 72:
                    originalScript = "not pc.follower_atmax()";
                    return !pc.HasMaxFollowers();
                case 43:
                case 63:
                case 73:
                    originalScript = "pc.follower_atmax()";
                    return pc.HasMaxFollowers();
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 51:
                    originalScript = "pc.follower_add(npc); game.sound( 4016 ); kill_asaph_guards( npc, pc )";
                    pc.AddFollower(npc);
                    Sound(4016);
                    kill_asaph_guards(npc, pc);
                    ;
                    break;
                case 72:
                case 73:
                    originalScript = "game.sound( 4016 ); kill_asaph( npc, pc )";
                    Sound(4016);
                    kill_asaph(npc, pc);
                    ;
                    break;
                case 92:
                    originalScript = "pc.follower_remove( npc )";
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
}
