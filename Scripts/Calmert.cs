
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
    [ObjectScript(5)]
    public class Calmert : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(28) && !triggerer.HasReputation(2)))
            {
                triggerer.AddReputation(2);
            }

            if ((attachee.GetMap() == 5013))
            {
                attachee.FloatLine(23000, triggerer);
            }
            else if ((attachee.HasMet(triggerer)))
            {
                if ((GetGlobalVar(5) <= 7))
                {
                    attachee.TurnTowards(triggerer); // added by Livonya
                    triggerer.BeginDialog(attachee, 10);
                }
                else
                {
                    attachee.TurnTowards(triggerer); // added by Livonya
                    triggerer.BeginDialog(attachee, 450);
                }

            }
            else
            {
                attachee.TurnTowards(triggerer); // added by Livonya
                triggerer.BeginDialog(attachee, 1);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5012))
            {
                if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((GetGlobalFlag(817)))
                    {
                        attachee.SetObjectFlag(ObjectFlag.OFF);
                        return RunDefault;
                    }

                    if ((!GameSystems.Combat.IsCombatActive()))
                    {
                        SetGlobalVar(724, 0);
                    }

                    if ((!GetGlobalFlag(902)))
                    {
                        StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
                        SetGlobalFlag(902, true);
                    }

                }

            }
            else if ((attachee.GetMap() == 5013))
            {
                if ((GetGlobalVar(510) != 2))
                {
                    if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
                    {
                        attachee.ClearObjectFlag(ObjectFlag.OFF);
                    }

                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(817, true);
            SetGlobalVar(23, GetGlobalVar(23) + 1);
            if ((GetGlobalVar(23) >= 2))
            {
                PartyLeader.AddReputation(92);
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
        {
            if (SelectedPartyLeader.GetPartyMembers().Any(o => o.HasFollowerByName(8730)))
            {
                var ron = Utilities.find_npc_near(SelectedPartyLeader, 8730);
                if (ron != null)
                {
                    SelectedPartyLeader.RemoveFollower(ron);
                }

            }

            if ((!GetGlobalFlag(816) && !GetGlobalFlag(818)))
            {
                SetGlobalFlag(818, true);
                foreach (var target in PartyLeader.GetPartyMembers())
                {
                    if ((target.GetNameId() != 8072 && attachee.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
                    {
                        attachee.TurnTowards(target);
                        target.BeginDialog(attachee, 2000);
                        return SkipDefault;
                    }

                }

                attachee.FloatLine(2010, triggerer);
                var terjonLoc = attachee.GetLocation();
                terjonLoc.locx -= 4;
                var terjon = GameSystems.MapObject.CreateObject(14007, terjonLoc);
                AttachParticles("sp-Dimension Door", terjon);
                terjon.TurnTowards(attachee);
                var calmert = attachee.GetInitiative();
                terjon.AddToInitiative();
                terjon.SetInitiative(calmert);
                UiSystems.Combat.Initiative.UpdateIfNeeded();
                terjon.Attack(triggerer);
            }

            foreach (var npc in PartyLeader.GetPartyMembers())
            {
                if ((npc.GetNameId() == 8072 && npc.GetLeader() != null && !GetGlobalFlag(819)))
                {
                    var curr = npc.GetStat(Stat.hp_current);
                    if ((curr >= 1))
                    {
                        foreach (var target in PartyLeader.GetPartyMembers())
                        {
                            if ((target.GetNameId() != 8072 && npc.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
                            {
                                npc.TurnTowards(target);
                                target.BeginDialog(npc, 1000);
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8000)))
            {
                var elmo = Utilities.find_npc_near(triggerer, 8000);
                if ((elmo != null))
                {
                    triggerer.RemoveFollower(elmo);
                    elmo.FloatLine(12021, triggerer);
                    elmo.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8001)))
            {
                var paida = Utilities.find_npc_near(triggerer, 8001);
                if ((paida != null))
                {
                    triggerer.RemoveFollower(paida);
                    paida.FloatLine(12021, triggerer);
                    paida.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8014)))
            {
                var otis = Utilities.find_npc_near(triggerer, 8014);
                if ((otis != null))
                {
                    triggerer.RemoveFollower(otis);
                    otis.FloatLine(12021, triggerer);
                    otis.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8015)))
            {
                var meleny = Utilities.find_npc_near(triggerer, 8015);
                if ((meleny != null))
                {
                    triggerer.RemoveFollower(meleny);
                    meleny.FloatLine(12021, triggerer);
                    meleny.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8021)))
            {
                var ydey = Utilities.find_npc_near(triggerer, 8021);
                if ((ydey != null))
                {
                    triggerer.RemoveFollower(ydey);
                    ydey.FloatLine(12021, triggerer);
                    ydey.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8022)))
            {
                var murfles = Utilities.find_npc_near(triggerer, 8022);
                if ((murfles != null))
                {
                    triggerer.RemoveFollower(murfles);
                    murfles.FloatLine(12021, triggerer);
                    murfles.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8031)))
            {
                var thrommel = Utilities.find_npc_near(triggerer, 8031);
                if ((thrommel != null))
                {
                    triggerer.RemoveFollower(thrommel);
                    thrommel.FloatLine(12021, triggerer);
                    thrommel.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8039)))
            {
                var taki = Utilities.find_npc_near(triggerer, 8039);
                if ((taki != null))
                {
                    triggerer.RemoveFollower(taki);
                    taki.FloatLine(12021, triggerer);
                    taki.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8054)))
            {
                var burne = Utilities.find_npc_near(triggerer, 8054);
                if ((burne != null))
                {
                    triggerer.RemoveFollower(burne);
                    burne.FloatLine(12021, triggerer);
                    burne.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8060)))
            {
                var morgan = Utilities.find_npc_near(triggerer, 8060);
                if ((morgan != null))
                {
                    triggerer.RemoveFollower(morgan);
                    morgan.FloatLine(12021, triggerer);
                    morgan.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8069)))
            {
                var pishella = Utilities.find_npc_near(triggerer, 8069);
                if ((pishella != null))
                {
                    triggerer.RemoveFollower(pishella);
                    pishella.FloatLine(12021, triggerer);
                    pishella.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8071)))
            {
                var rufus = Utilities.find_npc_near(triggerer, 8071);
                if ((rufus != null))
                {
                    triggerer.RemoveFollower(rufus);
                    rufus.FloatLine(12021, triggerer);
                    rufus.Attack(triggerer);
                }

            }

            if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8714)))
            {
                var holly = Utilities.find_npc_near(triggerer, 8714);
                if ((holly != null))
                {
                    triggerer.RemoveFollower(holly);
                    holly.FloatLine(1000, triggerer);
                    holly.Attack(triggerer);
                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            foreach (var npc in PartyLeader.GetPartyMembers())
            {
                if ((npc.GetNameId() == 8072 && npc.GetLeader() != null && !GetGlobalFlag(819)))
                {
                    var curr = npc.GetStat(Stat.hp_current);
                    if ((curr >= 1))
                    {
                        foreach (var target in PartyLeader.GetPartyMembers())
                        {
                            if ((target.GetNameId() != 8072 && npc.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
                            {
                                npc.TurnTowards(target);
                                target.BeginDialog(npc, 1000);
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(817, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(724) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()) && !PartyAlignment.IsGood())
            {
                attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(724) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()) && !PartyAlignment.IsGood())
            {
                attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(724, GetGlobalVar(724) + 1);
            return RunDefault;
        }
        public static bool create_terjon(GameObject attachee, GameObject triggerer)
        {
            var terjonLoc = attachee.GetLocation();
            terjonLoc.locx -= 4;
            var terjon = GameSystems.MapObject.CreateObject(14007, terjonLoc);
            terjon.TurnTowards(attachee);
            AttachParticles("sp-Dimension Door", terjon);
            return RunDefault;
        }
        public static bool switch_to_terjon(GameObject npc, GameObject pc)
        {
            var terjon = Utilities.find_npc_near(npc, 20003);
            foreach (var target in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((terjon.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
                {
                    terjon.TurnTowards(target);
                    target.BeginDialog(terjon, 2020);
                    return SkipDefault;
                }

            }

            terjon.FloatLine(2020, pc);
            terjon.Attack(pc);
            return RunDefault;
        }
        public static bool look_spugnoir(GameObject attachee, GameObject triggerer)
        {
            foreach (var npc in PartyLeader.GetPartyMembers())
            {
                if ((npc.GetNameId() == 8072 && npc.GetLeader() != null && !GetGlobalFlag(819)))
                {
                    var curr = npc.GetStat(Stat.hp_current);
                    if ((curr >= 1))
                    {
                        foreach (var target in PartyLeader.GetPartyMembers())
                        {
                            if ((npc.DistanceTo(target) <= 20 && target.type == ObjectType.pc && npc.GetNameId() != 8072))
                            {
                                npc.TurnTowards(target);
                                target.BeginDialog(npc, 1000);
                                return SkipDefault;
                            }

                        }

                    }

                }

            }

            attachee.Attack(triggerer);
            return RunDefault;
        }
        public static bool beggar_cavanaugh(GameObject attachee, GameObject triggerer)
        {
            StartTimer(86400000, () => beggar_now(attachee, triggerer));
            return RunDefault;
        }
        public static bool beggar_now(GameObject attachee, GameObject triggerer)
        {
            SetGlobalFlag(205, true);
            SetGlobalVar(24, GetGlobalVar(24) + 1);
            if ((!triggerer.HasReputation(5)))
            {
                triggerer.AddReputation(5);
            }

            if ((GetGlobalVar(24) >= 3 && !triggerer.HasReputation(6)))
            {
                triggerer.AddReputation(6);
            }

            return RunDefault;
        }
        public static void respawn(GameObject attachee)
        {
            var box = Utilities.find_container_near(attachee, 1001);
            InventoryRespawn.RespawnInventory(box);
            StartTimer(86400000, () => respawn(attachee)); // 86400000ms is 24 hours
            return;
        }

    }
}
