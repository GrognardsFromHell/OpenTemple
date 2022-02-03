
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
    [ObjectScript(305)]
    public class Wizard : BaseObjectScript
    {
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
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            // if (attachee.leader_get() == OBJ_HANDLE_NULL and not game.combat_is_active()):
            SetGlobalVar(717, 0);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(717) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.MageArmor, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(717) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.SeeInvisibility, attachee);
                attachee.PendingSpellsToMemorized();
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.GetNameId() == 14425 && attachee.GetMap() == 5065))
                    {
                        obj.CastSpell(WellKnownSpells.EndureElements, obj);
                        obj.PendingSpellsToMemorized();
                    }

                }

            }

            if ((GetGlobalVar(717) == 8 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.GetNameId() == 14425 && attachee.GetMap() == 5065))
                    {
                        obj.CastSpell(WellKnownSpells.CatsGrace, obj);
                        obj.PendingSpellsToMemorized();
                    }

                }

                attachee.CastSpell(WellKnownSpells.ProtectionFromArrows, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(717) == 12 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.EaglesSplendor, attachee);
                attachee.PendingSpellsToMemorized();
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.GetNameId() == 14425 && attachee.GetMap() == 5065))
                    {
                        obj.CastSpell(WellKnownSpells.OwlsWisdom, obj);
                        obj.PendingSpellsToMemorized();
                    }

                }

            }

            SetGlobalVar(717, GetGlobalVar(717) + 1);
            return RunDefault;
        }

    }
}
