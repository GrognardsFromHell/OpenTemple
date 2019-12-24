
using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
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
    [ObjectScript(125)]
    public class Belsornig : BaseObjectScript
    {
        public override bool OnDialog(GameObjectBody attachee, GameObjectBody triggerer)
        {
            ScriptDaemon.record_time_stamp(515);
            if ((GetGlobalFlag(132)))
            {
                attachee.Attack(triggerer);
            }
            else if ((!attachee.HasMet(triggerer)))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 370);
            }

            return SkipDefault;
        }
        public override bool OnFirstHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalFlag(372)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else
            {
                if ((attachee.GetLeader() == null))
                {
                    SetGlobalVar(714, 0);
                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalFlag(105, true);
            ScriptDaemon.record_time_stamp(457);
            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(345, false);
            if ((!attachee.HasEquippedByName(4071) || !attachee.HasEquippedByName(4124)))
            {
                attachee.WieldBestInAllSlots();
            }

            foreach (var statue in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_SCENERY))
            {
                if ((statue.GetNameId() == 1618))
                {
                    var loc = statue.GetLocation();
                    var rot = statue.Rotation;
                    statue.Destroy();
                    foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
                    {
                        if (obj.GetNameId() == 14244)
                        {
                            return RunDefault;
                        }

                    }

                    var juggernaut = GameSystems.MapObject.CreateObject(14244, loc);
                    juggernaut.Rotation = rot;
                    AttachParticles("ef-MinoCloud", juggernaut);
                    return RunDefault;
                }

            }

            return RunDefault;
        }
        public override bool OnResurrect(GameObjectBody attachee, GameObjectBody triggerer)
        {
            SetGlobalFlag(105, false);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((GetGlobalVar(714) == 16 && !attachee.HasEquippedByName(4071) || !attachee.HasEquippedByName(4124)))
            {
                attachee.WieldBestInAllSlots();
                attachee.WieldBestInAllSlots();
            }

            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((!attachee.HasMet(obj)))
                    {
                        if ((Utilities.is_safe_to_talk(attachee, obj)))
                        {
                            ScriptDaemon.record_time_stamp(515);
                            if (((GetGlobalFlag(104)) || (GetGlobalFlag(106)) || (GetGlobalFlag(107))))
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 600);
                            }
                            else
                            {
                                obj.TurnTowards(attachee); // added by Livonya
                                attachee.TurnTowards(obj); // added by Livonya
                                obj.BeginDialog(attachee, 1);
                            }

                        }

                    }

                }

            }

            // game.new_sid = 0			## removed by Livonya
            if ((GetGlobalVar(714) == 0 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.ProtectionFromElements, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(714) == 4 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.FreedomOfMovement, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(714) == 8 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                attachee.CastSpell(WellKnownSpells.Endurance, attachee);
                attachee.PendingSpellsToMemorized();
            }

            if ((GetGlobalVar(714) == 12 && attachee.GetLeader() == null && !GameSystems.Combat.IsCombatActive()))
            {
                // attachee.cast_spell(spell_fog_cloud, attachee)
                attachee.CastSpell(WellKnownSpells.ShieldOfFaith, attachee);
                attachee.PendingSpellsToMemorized();
            }

            SetGlobalVar(714, GetGlobalVar(714) + 1);
            return RunDefault;
        }
        public override bool OnWillKos(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if ((!GetGlobalFlag(132)))
            {
                return SkipDefault;
            }
            else
            {
                return RunDefault;
            }

        }

    }
}
