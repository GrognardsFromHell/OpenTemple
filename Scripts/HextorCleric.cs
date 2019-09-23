
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
    [ObjectScript(339)]
    public class HextorCleric : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            if ((GetGlobalVar(928) == 1))
            {
                triggerer.BeginDialog(attachee, 300);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                if ((GetQuestState(78) == QuestState.Accepted && GetGlobalVar(929) == 0))
                {
                    triggerer.BeginDialog(attachee, 90);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 40);
                }

            }
            else
            {
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetMap() == 5140))
            {
                if ((GetGlobalVar(949) == 2 || PartyLeader.HasReputation(47)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else if ((GetGlobalFlag(937)))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }
            else if ((attachee.GetMap() == 5064))
            {
                if ((GetGlobalVar(949) == 1))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            if ((attachee.GetNameId() == 14654 && attachee.GetMap() == 5064))
            {
                SetGlobalVar(949, 2);
            }
            else if ((attachee.GetNameId() == 14717 && attachee.GetMap() == 5140))
            {
                SetGlobalFlag(973, true);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetNameId() == 14717 && attachee.GetMap() == 5140))
            {
                SetGlobalFlag(973, false);
            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((is_better_to_talk(attachee, obj)))
                    {
                        attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
                        attachee.PendingSpellsToMemorized();
                        StartTimer(5000, () => next_spell(attachee, triggerer));
                        DetachScript();
                    }

                }

            }

            return RunDefault;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 200))
            {
                return true;
            }

            return false;
        }
        public static bool next_spell(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.CastSpell(WellKnownSpells.SpellResistance, attachee);
            attachee.PendingSpellsToMemorized();
            return RunDefault;
        }
        public static bool thaddeus_countdown(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(172800000, () => stop_watch()); // 2 days
            return RunDefault;
        }
        public static bool stop_watch()
        {
            SetGlobalVar(960, 2);
            return RunDefault;
        }

    }
}
