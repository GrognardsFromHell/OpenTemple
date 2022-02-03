
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
    [ObjectScript(314)]
    public class ElectricalTrap : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            if ((GetGlobalFlag(820)))
            {
                return SkipDefault;
            }
            else if ((triggerer.GetPartyMembers().Any(o => o.HasItemByName(6114))))
            {
                triggerer.BeginDialog(attachee, 1);
            }
            else
            {
                triggerer.BeginDialog(attachee, 1000);
            }

            return SkipDefault;
        }
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
        {
            var leader = PartyLeader;
            Co8.StopCombat(attachee, 0);
            leader.BeginDialog(attachee, 4000);
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            SetGlobalVar(707, GetGlobalVar(707) + 1);
            if ((GetGlobalVar(707) >= 3 && !GetGlobalFlag(820)))
            {
                SetGlobalVar(707, 0);
                AttachParticles("sp-Call Lightning", attachee);
                Sound(4019, 1);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(attachee) <= 12 && obj.GetNameId() != attachee.GetNameId() && obj.GetNameId() != 14605))
                    {
                        AttachParticles("sp-Shocking Grasp", obj);
                        Sound(4030, 1);
                        if ((obj.GetStat(Stat.hp_current) >= -9))
                        {
                            // for chest in game.obj_list_vicinity(attachee.location,OLC_CONTAINER):
                            // if (chest.name == 1055):
                            // if (obj.has_los(chest)):
                            // obj.turn_towards(chest)
                            var damage_dice = Dice.Parse("15d4");
                            if ((obj.GetNameId() == 8035))
                            {
                                damage_dice = Dice.Parse("5d4");
                            }

                            if (obj.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE, 0))
                            {
                                obj.FloatMesFileLine("mes/spell.mes", 30001);
                            }
                            else
                            {
                                obj.FloatMesFileLine("mes/spell.mes", 30002);
                            }

                        }

                    }

                }

            }

            return RunDefault;
        }
        public static bool zap(GameObject attachee, GameObject triggerer)
        {
            var damage_dice = Dice.Parse("5d4");
            AttachParticles("sp-Shocking Grasp", triggerer);
            Sound(4030, 1);
            if (triggerer.ReflexSaveAndDamage(null, 20, D20SavingThrowReduction.Half, D20SavingThrowFlag.NONE, damage_dice, DamageType.Electricity, D20AttackPower.UNSPECIFIED, D20ActionType.UNSPECIFIED_MOVE, 0))
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30001);
            }
            else
            {
                triggerer.FloatMesFileLine("mes/spell.mes", 30002);
            }

            return RunDefault;
        }

    }
}
