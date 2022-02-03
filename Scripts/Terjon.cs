
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

[ObjectScript(23)]
public class Terjon : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(28) && !triggerer.HasReputation(2)))
        {
            triggerer.AddReputation(2);
        }

        if ((attachee.GetMap() == 5013))
        {
            triggerer.BeginDialog(attachee, 1500);
        }
        else if ((attachee.HasMet(triggerer)))
        {
            if ((GetGlobalVar(5) >= 9))
            {
                triggerer.BeginDialog(attachee, 50);
            }
            else if ((PartyAlignment == Alignment.NEUTRAL_EVIL))
            {
                if ((GetGlobalVar(501) == 2))
                {
                    triggerer.BeginDialog(attachee, 990);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 1590);
                }

            }
            else if ((GetGlobalVar(5) <= 4))
            {
                if ((GetGlobalVar(501) == 2))
                {
                    triggerer.BeginDialog(attachee, 1000);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 30);
                }

            }
            else
            {
                if ((GetGlobalVar(501) == 2))
                {
                    triggerer.BeginDialog(attachee, 1010);
                }
                else
                {
                    triggerer.BeginDialog(attachee, 40);
                }

            }

        }
        else if ((PartyAlignment == Alignment.NEUTRAL_GOOD || PartyAlignment == Alignment.LAWFUL_GOOD || PartyAlignment == Alignment.LAWFUL_NEUTRAL))
        {
            SetGlobalVar(5, 3);
            triggerer.BeginDialog(attachee, 1);
        }
        else if ((PartyAlignment == Alignment.NEUTRAL_EVIL || PartyAlignment == Alignment.CHAOTIC_EVIL || PartyAlignment == Alignment.CHAOTIC_NEUTRAL))
        {
            SetGlobalVar(5, 5);
            triggerer.BeginDialog(attachee, 20);
        }
        else
        {
            SetGlobalVar(5, 4);
            triggerer.BeginDialog(attachee, 10);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((attachee.GetMap() == 5011))
        {
            if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6 || GetGlobalVar(510) == 2))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((GetGlobalFlag(816)))
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return RunDefault;
                }

                if ((GetGlobalFlag(21)))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                }
                else
                {
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    return RunDefault;
                }

                if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
                {
                    SetGlobalVar(723, 0);
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

        SetGlobalFlag(816, true);
        SetGlobalFlag(299, true);
        if (GetGlobalFlag(296))
        {
            PartyLeader.AddReputation(24);
            return RunDefault;
        }

        SetGlobalVar(23, GetGlobalVar(23) + 1);
        if (GetGlobalVar(23) >= 2)
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

        if ((!GetGlobalFlag(817) && !GetGlobalFlag(818)))
        {
            SetGlobalFlag(818, true);
            foreach (var target in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
            {
                if ((target.GetNameId() != 8072 && attachee.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
                {
                    attachee.TurnTowards(target);
                    target.BeginDialog(attachee, 2000);
                    return SkipDefault;
                }

            }

            attachee.FloatLine(2010, triggerer);
            var calmert = GameSystems.MapObject.CreateObject(14011, attachee.GetLocation().OffsetTiles(-4, 0));
            calmert.TurnTowards(attachee);
            AttachParticles("sp-Dimension Door", calmert);
            var terjon = attachee.GetInitiative();
            calmert.AddToInitiative();
            calmert.SetInitiative(terjon);
            UiSystems.Combat.Initiative.UpdateIfNeeded();
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
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(875) && !GetGlobalFlag(876) && GetQuestState(99) != QuestState.Completed && !triggerer.GetPartyMembers().Any(o => o.HasItemByName(12900))))
        {
            SetGlobalFlag(876, true);
            StartTimer(140000000, () => amii_dies());
        }

        if ((GetGlobalVar(723) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()) && !PartyAlignment.IsGood())
        {
            attachee.CastSpell(WellKnownSpells.DeathWard, attachee);
            attachee.PendingSpellsToMemorized();
        }

        if ((GetGlobalVar(723) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()) && !PartyAlignment.IsGood())
        {
            attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
            attachee.PendingSpellsToMemorized();
        }

        SetGlobalVar(723, GetGlobalVar(723) + 1);
        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(816, false);
        SetGlobalFlag(299, false);
        return RunDefault;
    }
    public static bool amii_dies()
    {
        SetQuestState(99, QuestState.Botched);
        SetGlobalFlag(862, true);
        return RunDefault;
    }
    public static bool create_calmert(GameObject attachee, GameObject triggerer)
    {
        var calmert = GameSystems.MapObject.CreateObject(14011, attachee.GetLocation().OffsetTiles(-4, 0));
        calmert.TurnTowards(attachee);
        AttachParticles("sp-Dimension Door", calmert);
        return RunDefault;
    }
    public static bool switch_to_calmert(GameObject npc, GameObject pc)
    {
        var calmert = Utilities.find_npc_near(npc, 20006);
        foreach (var target in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_PC))
        {
            if ((calmert.DistanceTo(target) <= 20 && target.type == ObjectType.pc))
            {
                calmert.TurnTowards(target);
                target.BeginDialog(calmert, 2020);
                return SkipDefault;
            }

        }

        calmert.FloatLine(2020, pc);
        calmert.Attack(pc);
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
    public static bool Change_Brooch(GameObject attachee, GameObject triggerer)
    {
        foreach (var pc in ObjList.ListVicinity(attachee, ObjectListFilter.OLC_NPC))
        {
            if ((pc.FindItemByName(3003) != null))
            {
                pc.BeginDialog(attachee, 1220);
                return SkipDefault;
            }

        }

        triggerer.BeginDialog(attachee, 1210);
        return SkipDefault;
    }

}