
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
    [ObjectScript(551)]
    public class Nightwalker : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5005))
            {
                if ((GetQuestState(95) == QuestState.Mentioned && GetGlobalVar(755) >= 9))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    SetGlobalVar(565, 0);
                    attachee.SetScriptId(ObjScriptEvent.Heartbeat, 551);
                }

            }

            attachee.D20SendSignal(D20DispatcherKey.SIG_SetPowerAttack, 5);
            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var dude in GameSystems.Party.PartyMembers)
                {
                    if ((attachee.DistanceTo(dude) <= 100))
                    {
                        SetGlobalVar(565, GetGlobalVar(565) + 1);
                        if ((attachee.GetLeader() == null))
                        {
                            if ((GetGlobalVar(565) == 4))
                            {
                                attachee.CastSpell(WellKnownSpells.SeeInvisibility, attachee);
                                attachee.PendingSpellsToMemorized();
                            }

                            if ((GetGlobalVar(565) >= 400))
                            {
                                SetGlobalVar(565, 0);
                            }

                        }

                        break;

                    }

                }

            }

            // game.sound( 4171, 1 )
            return RunDefault;
        }

    }
}
