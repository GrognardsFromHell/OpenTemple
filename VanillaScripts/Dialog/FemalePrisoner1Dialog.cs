
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

namespace VanillaScripts.Dialog
{
    [DialogScript(161)]
    public class FemalePrisoner1Dialog : FemalePrisoner1, IDialogScript
    {
        public bool CheckPrecondition(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 6:
                case 7:
                    originalScript = "game.party_alignment == NEUTRAL_EVIL or game.party_alignment == CHAOTIC_EVIL";
                    return PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL;
                case 23:
                case 33:
                case 34:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_male";
                    return pc.GetGender() == Gender.Male;
                case 24:
                case 36:
                case 37:
                    originalScript = "pc.stat_level_get( stat_gender ) == gender_female";
                    return pc.GetGender() == Gender.Female;
                case 35:
                    originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD ) and ( pc.stat_level_get( stat_gender ) == gender_male )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && (pc.GetGender() == Gender.Male);
                case 38:
                    originalScript = "( game.party_alignment == LAWFUL_GOOD or game.party_alignment == NEUTRAL_GOOD or game.party_alignment == CHAOTIC_GOOD ) and ( pc.stat_level_get( stat_gender ) == gender_female )";
                    return (PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.CHAOTIC_GOOD) && (pc.GetGender() == Gender.Female);
                case 61:
                case 62:
                    originalScript = "pc.money_get() >= 100000";
                    return pc.GetMoney() >= 100000;
                default:
                    originalScript = null;
                    return true;
            }
        }
        public void ApplySideEffect(GameObject npc, GameObject pc, int lineNumber, out string originalScript)
        {
            switch (lineNumber)
            {
                case 40:
                case 50:
                    originalScript = "game.global_flags[131] = 1; free_rep( npc, pc )";
                    SetGlobalFlag(131, true);
                    free_rep(npc, pc);
                    ;
                    break;
                case 61:
                case 62:
                    originalScript = "pc.money_adj(-100000)";
                    pc.AdjustMoney(-100000);
                    break;
                case 73:
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
}
