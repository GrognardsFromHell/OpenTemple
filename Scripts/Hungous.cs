
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
    [ObjectScript(596)]
    public class Hungous : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            attachee.FloatLine(1000, triggerer);
            SetGlobalVar(986, 3);
            SetGlobalFlag(560, true);
            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5115))
            {
                if ((!GameSystems.Combat.IsCombatActive()))
                {
                    if ((GetGlobalVar(570) == 1))
                    {
                        attachee.Unconceal();
                        attachee.SetStandpoint(StandPointType.Night, 738);
                        attachee.SetStandpoint(StandPointType.Day, 738);
                        SetGlobalVar(570, 2);
                        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                        {
                            if ((is_better_to_talk(attachee, obj)))
                            {
                                StartTimer(4000, () => start_talking(attachee, triggerer));
                                SetGlobalVar(570, 3);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
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
            PartyLeader.BeginDialog(attachee, 1);
            return RunDefault;
        }
        public static bool increment_rep(GameObject attachee, GameObject triggerer)
        {
            if ((PartyLeader.HasReputation(81)))
            {
                PartyLeader.AddReputation(82);
                PartyLeader.RemoveReputation(81);
            }
            else if ((PartyLeader.HasReputation(82)))
            {
                PartyLeader.AddReputation(83);
                PartyLeader.RemoveReputation(82);
            }
            else if ((PartyLeader.HasReputation(83)))
            {
                PartyLeader.AddReputation(84);
                PartyLeader.RemoveReputation(83);
            }
            else if ((PartyLeader.HasReputation(84)))
            {
                PartyLeader.AddReputation(85);
                PartyLeader.RemoveReputation(84);
            }
            else if ((PartyLeader.HasReputation(85)))
            {
                PartyLeader.AddReputation(86);
                PartyLeader.RemoveReputation(85);
            }
            else if ((PartyLeader.HasReputation(86)))
            {
                PartyLeader.AddReputation(87);
                PartyLeader.RemoveReputation(86);
            }
            else if ((PartyLeader.HasReputation(87)))
            {
                PartyLeader.AddReputation(88);
                PartyLeader.RemoveReputation(87);
            }
            else if ((PartyLeader.HasReputation(88)))
            {
                PartyLeader.AddReputation(89);
                PartyLeader.RemoveReputation(88);
            }
            else
            {
                PartyLeader.AddReputation(81);
            }

            return RunDefault;
        }
        public static void buff_1(GameObject attachee, GameObject triggerer)
        {
            var witch = Utilities.find_npc_near(attachee, 8627);
            witch.CastSpell(WellKnownSpells.GreaterHeroism, attachee);
            return;
        }
        public static void buff_2(GameObject attachee, GameObject triggerer)
        {
            var witch = Utilities.find_npc_near(attachee, 8627);
            var krunch = Utilities.find_npc_near(attachee, 8802);
            witch.CastSpell(WellKnownSpells.GreaterHeroism, krunch);
            return;
        }
        public static void buff_3(GameObject attachee, GameObject triggerer)
        {
            var warlock = Utilities.find_npc_near(attachee, 8626);
            warlock.CastSpell(WellKnownSpells.ProtectionFromGood, attachee);
            return;
        }
        public static void buff_4(GameObject attachee, GameObject triggerer)
        {
            var warlock = Utilities.find_npc_near(attachee, 8626);
            var krunch = Utilities.find_npc_near(attachee, 8802);
            warlock.CastSpell(WellKnownSpells.ProtectionFromGood, krunch);
            return;
        }

    }
}
