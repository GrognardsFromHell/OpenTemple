
using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts.Dialog
{
    [DialogScript(345)]
    public class AbaddonDialog : Abaddon, IDialogScript
    {
        public bool CheckPrecondition(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                    Trace.Assert(originalScript == "game.party_size() >= 3");
                    return GameSystems.Party.PartySize >= 3;
                case 72:
                    Trace.Assert(originalScript == "game.party_size() <= 2");
                    return GameSystems.Party.PartySize <= 2;
                default:
                    Trace.Assert(originalScript == null);
                    return true;
            }
        }
        public void ApplySideEffect(GameObjectBody npc, GameObjectBody pc, int lineNumber, string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 160)");
                    switch_to_tarah(npc, pc, 160);
                    break;
                case 21:
                    Trace.Assert(originalScript == "ward_tarah(npc,pc)");
                    ward_tarah(npc, pc);
                    break;
                case 31:
                    Trace.Assert(originalScript == "ward_kenan(npc,pc)");
                    ward_kenan(npc, pc);
                    break;
                case 41:
                    Trace.Assert(originalScript == "ward_sharar(npc,pc)");
                    ward_sharar(npc, pc);
                    break;
                case 51:
                    Trace.Assert(originalScript == "ward_abaddon(npc,pc)");
                    ward_abaddon(npc, pc);
                    break;
                case 71:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 10)");
                    switch_to_tarah(npc, pc, 10);
                    break;
                case 72:
                    Trace.Assert(originalScript == "switch_to_tarah( npc, pc, 350)");
                    switch_to_tarah(npc, pc, 350);
                    break;
                default:
                    Trace.Assert(originalScript == null);
                    return;
            }
        }
        public bool TryGetSkillCheck(int lineNumber, out DialogSkillChecks skillChecks)
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
