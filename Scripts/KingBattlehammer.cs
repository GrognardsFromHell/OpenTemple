
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
    [ObjectScript(486)]
    public class KingBattlehammer : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            triggerer.BeginDialog(attachee, 100);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(506, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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

        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
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
        public static bool start_talking(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(PartyLeader);
            PartyLeader.BeginDialog(attachee, 100);
            return RunDefault;
        }
        public static bool switch_to_lieutenant(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8893);
            if ((npc != null))
            {
                npc.TurnTowards(PartyLeader);
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool trap(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static bool runoff(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.RunOff();
            return RunDefault;
        }

    }
}
