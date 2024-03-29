
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

[ObjectScript(486)]
public class KingBattlehammer : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        triggerer.BeginDialog(attachee, 100);
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetNameId() == 8735))
        {
            SetGlobalFlag(506, true);
            if ((GetQuestState(96) == QuestState.Accepted || GetQuestState(96) == QuestState.Mentioned))
            {
                SetQuestState(96, QuestState.Completed);
            }

            SetGlobalVar(972, GetGlobalVar(972) + 1);
        }
        else
        {
            SetGlobalVar(972, GetGlobalVar(972) + 1);
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(506, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee != null && !Utilities.critter_is_unconscious(attachee) && !attachee.D20Query(D20DispatcherKey.QUE_Prone)))
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        StartTimer(2000, () => start_talking(attachee, triggerer));
                        DetachScript();
                    }

                }

            }

        }

        return RunDefault;
    }
    // def tools_transfer( attachee, triggerer ): Dont think works
    // itemA = attachee.item_find(12640)
    // if (itemA != OBJ_HANDLE_NULL):
    // itemA.destroy()
    // create_item_in_inventory( 12640, triggerer )
    // return RUN_DEFAULT

    public static bool is_better_to_talk(GameObject speaker, GameObject listener)
    {
        if ((speaker.HasLineOfSight(listener)))
        {
            if ((speaker.DistanceTo(listener) <= 40))
            {
                return true;
            }

        }

        return false;
    }
    public static bool start_talking(GameObject attachee, GameObject triggerer)
    {
        attachee.TurnTowards(PartyLeader);
        PartyLeader.BeginDialog(attachee, 100);
        return RunDefault;
    }
    public static bool switch_to_lieutenant(GameObject attachee, GameObject triggerer, int line)
    {
        var npc = Utilities.find_npc_near(attachee, 8893);
        if ((npc != null))
        {
            npc.TurnTowards(PartyLeader);
            triggerer.BeginDialog(npc, line);
        }

        return SkipDefault;
    }
    public static bool trap(GameObject attachee, GameObject triggerer)
    {
        AttachParticles("Mon-EarthElem-Unconceal", triggerer);
        AttachParticles("Mon-EarthElem-body120", triggerer);
        AttachParticles("Orb-Summon-Earth Elemental", triggerer);
        AttachParticles("sp-Calm Animals", triggerer);
        AttachParticles("sp-Quench", triggerer);
        Sound(4042, 1);
        foreach (var obj in PartyLeader.GetPartyMembers())
        {
            var damage_dice = Dice.Parse("4d8");
            obj.FloatMesFileLine("mes/float.mes", 3);
            AttachParticles("hit-BLUDGEONING-medium", obj);
            if ((obj.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Bludgeoning, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE)))
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30002);
            }

        }

        return RunDefault;
    }
    public static bool runoff(GameObject attachee, GameObject triggerer)
    {
        attachee.RunOff();
        return RunDefault;
    }

}