
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

[ObjectScript(12)]
public class Jaroo : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalVar(2) >= 100 && !triggerer.HasReputation(3)))
        {
            triggerer.AddReputation(3);
        }

        attachee.TurnTowards(triggerer);
        if ((GetGlobalVar(501) == 4 || GetGlobalVar(501) == 5 || GetGlobalVar(501) == 6))
        {
            triggerer.BeginDialog(attachee, 630);
        }
        else if ((GetGlobalVar(501) == 2))
        {
            triggerer.BeginDialog(attachee, 580);
        }
        else
        {
            triggerer.BeginDialog(attachee, 1);
        }

        return SkipDefault;
    }
    public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
    {
        return RunDefault;
        if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
        {
            SetGlobalVar(722, 0);
        }

        if ((GetGlobalVar(510) == 2))
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

        SetGlobalFlag(337, true);
        SetGlobalVar(23, GetGlobalVar(23) + 1);
        if ((GetGlobalVar(23) >= 2))
        {
            PartyLeader.AddReputation(92);
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        // if (not attachee.has_wielded(4047) or not attachee.has_wielded(4111)):
        if ((!attachee.HasEquippedByName(4111))) // 4111 (Rod of the Python) is a two-handed quarterstaff since v6.1
        {
            attachee.WieldBestInAllSlots();
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

        if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8072)))
        {
            var spugnoir = Utilities.find_npc_near(triggerer, 8072);
            if ((spugnoir != null))
            {
                triggerer.RemoveFollower(spugnoir);
                spugnoir.FloatLine(12021, triggerer);
                spugnoir.Attack(triggerer);
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

        if (triggerer.GetPartyMembers().Any(o => o.HasFollowerByName(8730)))
        {
            var ronald = Utilities.find_npc_near(triggerer, 8730);
            if ((ronald != null))
            {
                triggerer.RemoveFollower(ronald);
                ronald.FloatLine(12021, triggerer);
                ronald.Attack(triggerer);
            }

        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        Logger.Info("Jaroo start combat");
        if ((!attachee.HasEquippedByName(4047) || !attachee.HasEquippedByName(4111)))
        {
            attachee.WieldBestInAllSlots();
        }

        return RunDefault;
    }
    public override bool OnResurrect(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(337, false);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(875) && !GetGlobalFlag(876) && GetQuestState(99) != QuestState.Completed && !triggerer.GetPartyMembers().Any(o => o.HasItemByName(12900))))
        {
            SetGlobalFlag(876, true);
            StartTimer(140000000, () => amii_dies());
        }

        SetGlobalVar(722, GetGlobalVar(722) + 1);
        if ((GameSystems.Combat.IsCombatActive()) || (PartyAlignment.IsGood()))
        {
            return RunDefault;
        }

        if ((GetGlobalVar(722) == 1))
        {
            attachee.CastSpell(WellKnownSpells.Stoneskin, attachee);
            attachee.PendingSpellsToMemorized();
        }

        // if (game.global_vars[722] >= 5 and (not attachee.has_wielded(4047) or not attachee.has_wielded(4111))):
        if ((GetGlobalVar(722) >= 5 && !attachee.HasEquippedByName(4111))) // 4111 (Rod of the Python) is a two-handed quarterstaff since v6.1
        {
            attachee.WieldBestInAllSlots();
        }

        // attachee.item_wield_best_all()
        // if (not attachee.has_spell_effects()):
        // game.global_vars[722] = 0
        return RunDefault;
    }
    public static bool amii_dies_due_to_dont_give_a_damn_dialog(GameObject attachee, GameObject triggerer)
    {
        SetGlobalFlag(876, true);
        StartTimer(140000000, () => amii_dies());
        return RunDefault;
    }
    public static bool amii_dies()
    {
        SetQuestState(99, QuestState.Botched);
        SetGlobalFlag(862, true);
        return RunDefault;
    }
    // Function: should_clear_spell_on( obj )
    // Author  : Livonya
    // Returns : 0 if obj is dead, else 1
    // Purpose : to fix only characters that need it

    public static int should_clear_spell_on(GameObject obj)
    {
        if ((obj.GetStat(Stat.hp_current) <= -10))
        {
            return 0;
        }

        return 1;
    }
    public static bool kill_picked(GameObject obj, GameObject jaroo)
    {
        var tempp = GetGlobalVar(23);
        var damage_dice = Dice.Parse("100d20");
        obj.Damage(null, DamageType.Bludgeoning, damage_dice);
        jaroo.CastSpell(WellKnownSpells.Reincarnation, obj);
        obj.SetBaseStat(Stat.experience, GetGlobalVar(753));
        jaroo.PendingSpellsToMemorized();
        if (tempp <= 1)
        {
            StartTimer(500, () => de_butcherize(tempp), true);
        }

        return RunDefault;
    }
    public static void de_butcherize(int tempp)
    {
        SelectedPartyLeader.RemoveReputation(1);
        SetGlobalVar(23, tempp);
    }
    public static bool run_off(GameObject npc, GameObject pc)
    {
        SetQuestState(99, QuestState.Completed);
        foreach (var obj in ObjList.ListVicinity(npc.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if (obj.GetNameId() == 8090) // Amii
            {
                obj.RunOff();
            }

        }

        return RunDefault;
    }

}