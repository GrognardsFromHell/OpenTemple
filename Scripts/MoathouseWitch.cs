
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
    [ObjectScript(546)]
    public class MoathouseWitch : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(249, true);
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (attachee.GetNameId() == 14958) // Nightwalker
            {
                var dummy = 1;
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
