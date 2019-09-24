
using System;
using System.Collections.Generic;
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

namespace Scripts
{
    [ObjectScript(563)]
    public class KobortAmbushCaveexit : BaseObjectScript
    {
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(765) == 2 || GetGlobalVar(765) == 3 || GetGlobalFlag(283)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                return SkipDefault;
            }
            else if (((attachee.GetMap() == 5091) && (GetGlobalVar(765) == 0) && (PartyAlignment != Alignment.LAWFUL_EVIL)) && !Co8Settings.DisableNewPlots && ((GetGlobalVar(450) & Math.Pow(2, 11)) == 0))
            {
                if (!SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8002)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8004)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8005)) && !SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8010)))
                {
                    if (((!GetGlobalFlag(44)) && (!GetGlobalFlag(45)) && (!GetGlobalFlag(700)) && (GetGlobalFlag(37)) && (!GetGlobalFlag(283))))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                        if ((!GameSystems.Combat.IsCombatActive()))
                        {
                            if ((is_better_to_talk(attachee, PartyLeader)))
                            {
                                StartTimer(2000, () => start_talking(attachee, triggerer));
                                DetachScript();
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 50))
            {
                return true;
            }

            return false;
        }
        public static bool start_talking(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 600);
            return RunDefault;
        }

    }
}
