
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

namespace Scripts.Dialog
{
    [DialogScript(345)]
    public class AbaddonDialog : Abaddon, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 71:
                    originalScript = "game.party_size() >= 3";
                    return GameSystems.Party.PartySize >= 3;
                case 72:
                    originalScript = "game.party_size() <= 2";
                    return GameSystems.Party.PartySize <= 2;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 11:
                    originalScript = "switch_to_tarah( npc, pc, 160)";
                    switch_to_tarah(npc, pc, 160);
                    break;
                case 21:
                    originalScript = "ward_tarah(npc,pc)";
                    ward_tarah(npc, pc);
                    break;
                case 31:
                    originalScript = "ward_kenan(npc,pc)";
                    ward_kenan(npc, pc);
                    break;
                case 41:
                    originalScript = "ward_sharar(npc,pc)";
                    ward_sharar(npc, pc);
                    break;
                case 51:
                    originalScript = "ward_abaddon(npc,pc)";
                    ward_abaddon(npc, pc);
                    break;
                case 71:
                    originalScript = "switch_to_tarah( npc, pc, 10)";
                    switch_to_tarah(npc, pc, 10);
                    break;
                case 72:
                    originalScript = "switch_to_tarah( npc, pc, 350)";
                    switch_to_tarah(npc, pc, 350);
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
