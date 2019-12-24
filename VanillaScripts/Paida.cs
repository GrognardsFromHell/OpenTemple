
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace VanillaScripts
{
    [ObjectScript(143)]
    public class Paida : BaseObjectScript
    {

        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((attachee.GetLeader() != null))
            {
                triggerer.BeginDialog(attachee, 110);
            }
            else if ((!GetGlobalFlag(148)))
            {
                if ((GetGlobalFlag(325)))
                {
                    triggerer.BeginDialog(attachee, 230);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1);
                }

            }
            else if ((GetGlobalFlag(38)))
            {
                triggerer.BeginDialog(attachee, 170);
            }
            else
            {
                triggerer.BeginDialog(attachee, 160);
            }

            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(153, true);
            if ((attachee.GetLeader() != null))
            {
                SetGlobalVar(29, GetGlobalVar(29) + 1);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(153, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                if ((GetQuestState(20) == QuestState.Completed))
                {
                    foreach (var pc in GameSystems.Party.PartyMembers)
                    {
                        if (pc.HasFollowerByName(8001))
                        {
                            attachee.AdjustReaction(pc, +30);
                            pc.RemoveFollower(attachee);
                            DetachScript();

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(149)))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObjectBody attachee, GameObjectBody triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.DispelMagic || spell.spellEnum == WellKnownSpells.BreakEnchantment || spell.spellEnum == WellKnownSpells.DispelEvil))
            {
                SetGlobalFlag(148, true);
                triggerer.BeginDialog(attachee, 70);
            }

            return RunDefault;
        }
        public static bool run_off(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.SetStandpoint(StandPointType.Night, 257);
            attachee.SetStandpoint(StandPointType.Day, 257);
            attachee.RunOff();
            return RunDefault;
        }
        public static bool LookHedrack(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8032);

            if ((npc != null) && (!GetGlobalFlag(146)))
            {
                triggerer.BeginDialog(npc, line);
                npc.TurnTowards(attachee);
                attachee.TurnTowards(npc);
            }
            else
            {
                triggerer.BeginDialog(attachee, 10);
            }

            return SkipDefault;
        }
        public static bool get_rep(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (!triggerer.HasReputation(7))
            {
                triggerer.AddReputation(7);
            }

            SetGlobalVar(25, GetGlobalVar(25) + 1);
            if ((GetGlobalVar(25) >= 3 && !triggerer.HasReputation(8)))
            {
                triggerer.AddReputation(8);
            }

            return RunDefault;
        }


    }
}
