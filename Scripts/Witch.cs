
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
    [ObjectScript(306)]
    public class Witch : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                SetGlobalVar(729, 0);
            }

            return RunDefault;
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
            if (((!GetGlobalFlag(833) && attachee.GetMap() == 5065 && !GetGlobalFlag(835) && !GetGlobalFlag(849)) || (GetGlobalFlag(839) && attachee.GetMap() == 5065 && !GetGlobalFlag(840) && !GetGlobalFlag(849))))
            {
                return SkipDefault;
            }

            if ((GetGlobalVar(711) == 0 && attachee.GetMap() == 5065))
            {
                Utilities.create_item_in_inventory(8906, attachee);
                SetGlobalVar(711, 1);
            }

            foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
            {
                var curr = obj.GetStat(Stat.hp_current);
                if ((curr <= -10 && obj.DistanceTo(attachee) <= 10 && obj.GetLeader() == null && (obj.IsMonsterCategory(MonsterCategory.humanoid) || obj.IsMonsterCategory(MonsterCategory.fey) || obj.IsMonsterCategory(MonsterCategory.giant) || obj.IsMonsterCategory(MonsterCategory.monstrous_humanoid))))
                {
                    Utilities.create_item_in_inventory(8907, attachee);
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(729) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() != 5065))
            {
                attachee.CastSpell(WellKnownSpells.Endurance, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(729) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() != 5065))
            {
                attachee.CastSpell(WellKnownSpells.EndureElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(729) == 8 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive() && attachee.GetMap() != 5065))
            {
                attachee.CastSpell(WellKnownSpells.OwlsWisdom, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(729, GetGlobalVar(729) + 1);
            return RunDefault;
        }
        public override bool OnWillKos(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(840) && attachee.GetMap() == 5065))
            {
                return SkipDefault;
            }

            return RunDefault;
        }

    }
}
