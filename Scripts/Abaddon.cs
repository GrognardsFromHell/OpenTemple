
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
    [ObjectScript(345)]
    public class Abaddon : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(993) == 2))
            {
                attachee.ClearObjectFlag(ObjectFlag.OFF);
            }
            else if ((GetGlobalVar(993) == 3))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(952, true);
            if ((GetGlobalFlag(948) && GetGlobalFlag(949) && GetGlobalFlag(950) && GetGlobalFlag(951) && GetGlobalFlag(953) && GetGlobalFlag(954)))
            {
                PartyLeader.AddReputation(40);
            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(952, false);
            PartyLeader.RemoveReputation(40);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(703) == 0))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                    {
                        if ((is_better_to_talk(attachee, obj)))
                        {
                            attachee.CastSpell(WellKnownSpells.Prayer, attachee);
                            SetGlobalVar(703, 1);
                        }

                    }

                }

            }
            else if ((GetGlobalVar(703) == 2))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((is_better_to_talk(attachee, PartyLeader)))
                    {
                        StartTimer(5000, () => all_done(attachee, triggerer));
                        SetGlobalVar(703, 3);
                    }

                }

            }

            return RunDefault;
        }
        public static void all_done(GameObjectBody attachee, GameObjectBody triggerer)
        {
            PartyLeader.BeginDialog(attachee, 70);
            return;
        }
        public static bool is_better_to_talk(GameObjectBody speaker, GameObjectBody listener)
        {
            if ((speaker.DistanceTo(listener) <= 55))
            {
                return true;
            }

            return false;
        }
        public static bool switch_to_tarah(GameObjectBody attachee, GameObjectBody triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8805);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool ward_tarah(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            attachee.CastSpell(WellKnownSpells.DeathWard, tarah);
            StartTimer(5000, () => ward_kenan(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_kenan(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var kenan = Utilities.find_npc_near(attachee, 8804);
            attachee.CastSpell(WellKnownSpells.DeathWard, kenan);
            StartTimer(5000, () => ward_sharar(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_sharar(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var sharar = Utilities.find_npc_near(attachee, 8806);
            attachee.CastSpell(WellKnownSpells.DeathWard, sharar);
            StartTimer(5000, () => ward_abaddon(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_gadham(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var gadham = Utilities.find_npc_near(attachee, 8807);
            attachee.CastSpell(WellKnownSpells.DeathWard, gadham);
            return RunDefault;
        }
        public static bool ward_abaddon(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
            SetGlobalVar(703, 2);
            return RunDefault;
        }

    }
}
