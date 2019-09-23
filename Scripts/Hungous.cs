
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
    [ObjectScript(596)]
    public class Hungous : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            attachee.TurnTowards(triggerer);
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
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
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            return SkipDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
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
            PartyLeader.BeginDialog(attachee, 1);
            return RunDefault;
        }
        public static bool increment_rep(GameObjectBody attachee, GameObjectBody triggerer)
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
        public static void buff_1(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var witch = Utilities.find_npc_near(attachee, 8627);
            witch.CastSpell(WellKnownSpells.GreaterHeroism, attachee);
            return;
        }
        public static void buff_2(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var witch = Utilities.find_npc_near(attachee, 8627);
            var krunch = Utilities.find_npc_near(attachee, 8802);
            witch.CastSpell(WellKnownSpells.GreaterHeroism, krunch);
            return;
        }
        public static void buff_3(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var warlock = Utilities.find_npc_near(attachee, 8626);
            warlock.CastSpell(WellKnownSpells.ProtectionFromGood, attachee);
            return;
        }
        public static void buff_4(GameObjectBody attachee, GameObjectBody triggerer)
        {
            var warlock = Utilities.find_npc_near(attachee, 8626);
            var krunch = Utilities.find_npc_near(attachee, 8802);
            warlock.CastSpell(WellKnownSpells.ProtectionFromGood, krunch);
            return;
        }

    }
}
