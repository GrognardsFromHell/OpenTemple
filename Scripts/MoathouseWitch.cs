
using System;
using System.Collections.Generic;
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

namespace Scripts
{
    [ObjectScript(546)]
    public class MoathouseWitch : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5005))
            {
                if ((GetQuestState(95) == QuestState.Mentioned && GetGlobalVar(756) >= 7))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
                    attachee.PendingSpellsToMemorized();
                    SetGlobalVar(566, 0);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(249, true);
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            if (attachee.GetNameId() == 14958) // Nightwalker
            {
                var dummy = 1;
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5005))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var dude in GameSystems.Party.PartyMembers)
                    {
                        if ((attachee.DistanceTo(dude) <= 100))
                        {
                            SetGlobalVar(566, GetGlobalVar(566) + 1);
                            if ((attachee.GetLeader() == null))
                            {
                                if ((GetGlobalVar(566) == 4))
                                {
                                    attachee.CastSpell(WellKnownSpells.Mislead, attachee);
                                    attachee.PendingSpellsToMemorized();
                                }

                                if ((GetGlobalVar(566) >= 400))
                                {
                                    SetGlobalVar(566, 0);
                                }

                            }

                            return RunDefault;
                        }

                    }

                }

            }

            return RunDefault;
        }

    }
}
