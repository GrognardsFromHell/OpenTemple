
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
    [ObjectScript(345)]
    public class Abaddon : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            return RunDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
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
        public override bool OnDying(GameObject attachee, GameObject triggerer)
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
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(952, false);
            PartyLeader.RemoveReputation(40);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
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
        public static void all_done(GameObject attachee, GameObject triggerer)
        {
            PartyLeader.BeginDialog(attachee, 70);
            return;
        }
        public static bool is_better_to_talk(GameObject speaker, GameObject listener)
        {
            if ((speaker.DistanceTo(listener) <= 55))
            {
                return true;
            }

            return false;
        }
        public static bool switch_to_tarah(GameObject attachee, GameObject triggerer, int line)
        {
            var npc = Utilities.find_npc_near(attachee, 8805);
            if ((npc != null))
            {
                triggerer.BeginDialog(npc, line);
            }

            return SkipDefault;
        }
        public static bool ward_tarah(GameObject attachee, GameObject triggerer)
        {
            var tarah = Utilities.find_npc_near(attachee, 8805);
            attachee.CastSpell(WellKnownSpells.DeathWard, tarah);
            StartTimer(5000, () => ward_kenan(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_kenan(GameObject attachee, GameObject triggerer)
        {
            var kenan = Utilities.find_npc_near(attachee, 8804);
            attachee.CastSpell(WellKnownSpells.DeathWard, kenan);
            StartTimer(5000, () => ward_sharar(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_sharar(GameObject attachee, GameObject triggerer)
        {
            var sharar = Utilities.find_npc_near(attachee, 8806);
            attachee.CastSpell(WellKnownSpells.DeathWard, sharar);
            StartTimer(5000, () => ward_abaddon(attachee, triggerer));
            return RunDefault;
        }
        public static bool ward_gadham(GameObject attachee, GameObject triggerer)
        {
            var gadham = Utilities.find_npc_near(attachee, 8807);
            attachee.CastSpell(WellKnownSpells.DeathWard, gadham);
            return RunDefault;
        }
        public static bool ward_abaddon(GameObject attachee, GameObject triggerer)
        {
            attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
            SetGlobalVar(703, 2);
            return RunDefault;
        }

    }
}
