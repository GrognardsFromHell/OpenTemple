
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

namespace Scripts;

[ObjectScript(551)]
public class Nightwalker : BaseObjectScript
{
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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