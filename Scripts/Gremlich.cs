
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
    [ObjectScript(368)]
    public class Gremlich : BaseObjectScript
    {
        public override bool OnFirstHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(929)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            else if ((!Utilities.is_daytime()))
            {
                if ((GetGlobalVar(927) == 4))
                {
                    attachee.ClearObjectFlag(ObjectFlag.OFF);
                    if ((attachee.GetMap() == 5001))
                    {
                        if ((attachee.GetNameId() == 14752))
                        {
                            Sound(4126, 1);
                        }
                        else if ((attachee.GetNameId() == 14699))
                        {
                            Sound(4128, 1);
                        }

                    }
                    else if ((attachee.GetMap() == 5051))
                    {
                        Sound(4127, 1);
                    }
                    else if ((attachee.GetMap() == 5121))
                    {
                        Sound(4129, 1);
                    }

                }

            }

            return RunDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalVar(926) >= 3))
            {
                if (CombatStandardRoutines.should_modify_CR(attachee))
                {
                    CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
                }

                SetGlobalFlag(929, true);
                AttachParticles("hit-FIRE-medium", attachee);
                AttachParticles("ef-MinoCloud", attachee);
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }
            // return RUN_DEFAULT
            else if ((GetGlobalVar(926) <= 2))
            {
                SetGlobalVar(926, 0);
                attachee.SetObjectFlag(ObjectFlag.OFF);
                AttachParticles("ef-MinoCloud", attachee);
                if ((attachee.GetNameId() == 14752))
                {
                    var grem2 = GameSystems.MapObject.CreateObject(14699, attachee.GetLocation());
                    AttachParticles("Trap-Fire", grem2);
                    grem2.TurnTowards(PartyLeader);
                    Sound(4125, 1);
                    grem2.Attack(triggerer);
                }
                else if ((attachee.GetNameId() == 14699))
                {
                    var grem1 = GameSystems.MapObject.CreateObject(14752, attachee.GetLocation());
                    AttachParticles("Trap-PoisonGas", grem1);
                    grem1.TurnTowards(PartyLeader);
                    Sound(4129, 1);
                    grem1.Attack(triggerer);
                }

                return SkipDefault;
            }

            // dice = dice_new("1d10+1000")
            // attachee.heal( OBJ_HANDLE_NULL, dice )
            return RunDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(925, GetGlobalVar(925) + 1);
            if ((Utilities.obj_percent_hp(attachee) < 40))
            {
                if ((GetGlobalVar(926) <= 2))
                {
                    SetGlobalVar(926, 0);
                    attachee.SetObjectFlag(ObjectFlag.OFF);
                    AttachParticles("ef-MinoCloud", attachee);
                    if ((attachee.GetNameId() == 14752))
                    {
                        var grem2 = GameSystems.MapObject.CreateObject(14699, attachee.GetLocation());
                        AttachParticles("Trap-Fire", grem2);
                        grem2.TurnTowards(PartyLeader);
                        Sound(4125, 1);
                        grem2.Attack(triggerer);
                    }
                    else if ((attachee.GetNameId() == 14699))
                    {
                        var grem1 = GameSystems.MapObject.CreateObject(14752, attachee.GetLocation());
                        AttachParticles("Trap-PoisonGas", grem1);
                        grem1.TurnTowards(PartyLeader);
                        Sound(4129, 1);
                        grem1.Attack(triggerer);
                    }

                    return SkipDefault;
                }

            }
            else if ((GetGlobalVar(925) == 0))
            {
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 1))
            {
                attachee.SetInt(obj_f.critter_strategy, 426);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 2))
            {
                attachee.SetInt(obj_f.critter_strategy, 427);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 3))
            {
                attachee.SetInt(obj_f.critter_strategy, 428);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 4))
            {
                attachee.SetInt(obj_f.critter_strategy, 429);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 5))
            {
                attachee.SetInt(obj_f.critter_strategy, 430);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 6))
            {
                attachee.SetInt(obj_f.critter_strategy, 431);
                return RunDefault;
            }
            else if ((GetGlobalVar(925) == 7))
            {
                SetGlobalVar(925, 0);
                return RunDefault;
            }
            return RunDefault;
        }
        public override bool OnExitCombat(GameObject attachee, GameObject triggerer)
        {
            if ((attachee.GetMap() == 5001 || attachee.GetMap() == 5051 || attachee.GetMap() == 5121))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
                if ((!GetGlobalFlag(929)))
                {
                    SetGlobalVar(927, 5);
                    StartTimer(432000000, () => reset_gremlich()); // 432000000ms is 5 days
                    ScriptDaemon.record_time_stamp("s_gremlich_2");
                }

            }

            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(929)))
            {
                attachee.SetObjectFlag(ObjectFlag.OFF);
            }

            return RunDefault;
        }
        public override bool OnSpellCast(GameObject attachee, GameObject triggerer, SpellPacketBody spell)
        {
            if ((spell.spellEnum == WellKnownSpells.Flare || spell.spellEnum == WellKnownSpells.SearingLight || spell.spellEnum == WellKnownSpells.FaerieFire))
            {
                SetGlobalVar(926, GetGlobalVar(926) + 1);
                AttachParticles("hit-FIRE-medium", attachee);
                AttachParticles("ef-MinoCloud", attachee);
                Sound(4127, 1);
            }

            return RunDefault;
        }
        public static bool reset_gremlich()
        {
            QueueRandomEncounter(3440);
            ScriptDaemon.set_f("s_gremlich_2_scheduled");
            return RunDefault;
        }

    }
}
