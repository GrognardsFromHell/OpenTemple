
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

[ObjectScript(302)]
public class Brigand : BaseObjectScript
{
    public override bool OnDialog(GameObject attachee, GameObject triggerer)
    {
        DetachScript();
        return SkipDefault;
    }
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        return RunDefault;
    }
    public override bool OnEnterCombat(GameObject attachee, GameObject triggerer)
    {
        if ((!GetGlobalFlag(833) && attachee.GetMap() == 5065))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.type == ObjectType.pc))
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.GetNameId() == 8002 && obj.GetLeader() != null))
                {
                    var leader = obj.GetLeader();
                    leader.BeginDialog(obj, 266);
                }

            }

            if ((attachee.GetNameId() == 14310 && GetGlobalFlag(37) && !GetGlobalFlag(835))) // Lareth dead
            {
                var leader = PartyLeader;
                if ((leader.GetSkillLevel(attachee, SkillId.bluff) >= 10))
                {
                    leader.BeginDialog(attachee, 266);
                }
                else if ((leader.GetSkillLevel(attachee, SkillId.bluff) >= 5))
                {
                    leader.BeginDialog(attachee, 66);
                }
                else
                {
                    leader.BeginDialog(attachee, 166);
                }

            }

            if ((attachee.GetNameId() == 14310 && GetGlobalFlag(834) && !GetGlobalFlag(37) && !GetGlobalFlag(835))) // Lareth alive
            {
                var leader = PartyLeader;
                leader.BeginDialog(attachee, 366);
            }

            if ((attachee.GetNameId() == 14310 && !GetGlobalFlag(834) && !GetGlobalFlag(37) && !GetGlobalFlag(835))) // Lareth not met
            {
                var leader = PartyLeader;
                leader.BeginDialog(attachee, 466);
            }

            return SkipDefault;
        }

        if ((GetGlobalFlag(839) && attachee.GetMap() == 5065 && !GetGlobalFlag(840)))
        {
            foreach (var pc in GameSystems.Party.PartyMembers)
            {
                if ((pc.type == ObjectType.pc))
                {
                    attachee.AIRemoveFromShitlist(pc);
                }

            }

            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                if ((obj.GetNameId() == 14614 && obj.GetLeader() != null))
                {
                    var leader = obj.GetLeader();
                    leader.BeginDialog(obj, 400);
                    return SkipDefault;
                }

            }

            if ((GetGlobalFlag(847)))
            {
                var target = GameSystems.MapObject.CreateObject(14617, new locXY(479, 489));
                SetGlobalFlag(841, false);
                SetGlobalFlag(847, false);
                target.TurnTowards(PartyLeader);
                PartyLeader.BeginDialog(target, 350);
            }

            return SkipDefault;
        }

        if ((GetGlobalFlag(840) && attachee.GetMap() == 5065))
        {
            // game.global_flags[840] = 0
            SetGlobalFlag(849, true);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetMap() == 5002) // moathouse courtyard
        {
            foreach (var npc in ObjList.ListVicinity(new locXY(477, 463), ObjectListFilter.OLC_NPC))
            {
                if (npc.GetNameId() == 14070 && npc.GetLeader() == null && !npc.IsUnconscious())
                {
                    npc.Attack(SelectedPartyLeader);
                }

            }

        }

        if (((!GetGlobalFlag(833) && attachee.GetMap() == 5065 && !GetGlobalFlag(835) && !GetGlobalFlag(849)) || (GetGlobalFlag(839) && attachee.GetMap() == 5065 && !GetGlobalFlag(840) && !GetGlobalFlag(849))))
        {
            return SkipDefault;
        }

        // THIS IS USED FOR BREAK FREE
        // found_nearby = 0
        // for obj in game.party[0].group_list():
        // if (obj.distance_to(attachee) <= 3 and obj.stat_level_get(stat_hp_current) >= -9):
        // found_nearby = 1
        // if found_nearby == 0:
        // while(attachee.item_find(8903) != OBJ_HANDLE_NULL):
        // attachee.item_find(8903).destroy()
        // #if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // #	create_item_in_inventory( 8903, attachee )
        // attachee.d20_send_signal(S_BreakFree)
        // Spiritual Weapon Shenanigens	#
        CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
        return RunDefault;
    }
    public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
    {
        if (attachee.GetMap() == 5002) // moathouse courtyard brigands
        {
            attachee.SetScriptId(ObjScriptEvent.StartCombat, 302);
        }

        if ((GetGlobalFlag(841) && attachee.GetMap() == 5065 && attachee.GetNameId() == 14310))
        {
            var target = Utilities.find_npc_near(attachee, 14614);
            if ((target == null))
            {
                target = GameSystems.MapObject.CreateObject(14617, new locXY(479, 489));
                SetGlobalFlag(841, false);
                return RunDefault;
            }

        }

        return RunDefault;
    }
    public override bool OnWillKos(GameObject attachee, GameObject triggerer)
    {
        if ((GetGlobalFlag(840) && attachee.GetMap() == 5065))
        {
            return SkipDefault;
        }

        if (attachee.GetMap() == 5091)
        {
            return SkipDefault;
        }

        return RunDefault;
    }
    public static bool buff_npc(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14424 && obj.GetLeader() == null))
            {
                obj.TurnTowards(attachee);
                obj.CastSpell(WellKnownSpells.MageArmor, obj);
            }

            if ((obj.GetNameId() == 14425 && obj.GetLeader() == null))
            {
                obj.TurnTowards(attachee);
                obj.CastSpell(WellKnownSpells.ShieldOfFaith, obj);
            }

        }

        return RunDefault;
    }
    public static bool buff_npc_two(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14424 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.MirrorImage, obj);
            }

            if ((obj.GetNameId() == 14425 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.OwlsWisdom, obj);
            }

        }

        return RunDefault;
    }
    public static bool buff_npc_three(GameObject attachee, GameObject triggerer)
    {
        foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
        {
            if ((obj.GetNameId() == 14424 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.ProtectionFromArrows, obj);
            }

            if ((obj.GetNameId() == 14425 && obj.GetLeader() == null))
            {
                obj.CastSpell(WellKnownSpells.EndureElements, obj);
            }

        }

        return RunDefault;
    }

}