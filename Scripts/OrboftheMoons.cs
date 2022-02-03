
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
    [ObjectScript(284)]
    public class OrboftheMoons : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        // this is for oil, acid, Alchemist's fire etc

        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
        {
            // def san_insert_item( attachee, triggerer ):	## this is for oil, acid, Alchemist's fire etc
            var cap1 = triggerer.GetNameId();
            var cap2 = attachee.GetInt(obj_f.item_pad_i_1);
            // first check if the user is just moving it around internally (eg equipping it)
            if (cap2 == cap1)
            {
                return RunDefault;
            }
            // next check if there was no previous user (getting from a box or off ground)
            else if (cap2 == 0)
            {
                if ((triggerer.type == ObjectType.pc || triggerer.type == ObjectType.npc))
                {
                    attachee.SetInt(obj_f.item_pad_i_1, cap1);
                }

                return RunDefault;
            }
            // next check if it is being inserted INTO box or dropped onto ground
            // hmmm... this doesn't work, PC remains triggerer... I wonder...
            else if ((triggerer.type != ObjectType.pc && triggerer.type != ObjectType.npc))
            {
                // game.particles( "sp-summon monster I", game.party[0] )
                attachee.SetInt(obj_f.item_pad_i_1, 0);
                return RunDefault;
            }
            // now the money shot
            // if this takes place in combat, then it's going from one combatant to another
            // add splash effect object
            else if (GameSystems.Combat.IsCombatActive())
            {
                var cap3 = attachee.GetNameId();
                attachee.Destroy();
            }

            // next section only for KotB, requires certain strategy changes
            // and deals with items like Alchemist's Fire not in ToEE
            // if cap3 == 4643 or cap3 == 4644 or cap3 == 4645:
            // splash_effect = game.obj_create(12833, triggerer.location)
            // triggerer.item_get(splash_effect)
            // splash_effect.obj_set_int( obj_f_item_pad_i_1, cap3 )
            // ## set original weapon name on residue items
            // ## so residue effect can be individually scripted
            // game.timeevent_add( get_rid_of_it, ( splash_effect, triggerer ), 1500 )
            // outside combat we don't have to worry, it's just party members moving it around
            return RunDefault;
        }
        public override bool OnRemoveItem(GameObject attachee, GameObject triggerer)
        {
            attachee.Destroy();
            return RunDefault;
        }
        public static void get_rid_of_it(GameObject obj, GameObject victim)
        {
            var eff1 = obj.GetInt(obj_f.item_pad_i_1);
            if (eff1 == 0 || eff1 == null)
            {
                return;
            }

            var eff2 = obj.GetNameId();
            SetGlobalVar(902, obj.GetInt(obj_f.item_pad_i_1));
            Dice dam;
            DamageType dtype;
            if (eff1 == 4643) // alchemist's fire
            {
                dam = Dice.D6;
                dtype = DamageType.Fire;
                AttachParticles("hit-FIRE-medium", victim);
            }
            else if (eff1 == 4644) // alchemist's spark
            {
                dam = Dice.D8;
                dtype = DamageType.Electricity;
                AttachParticles("hit-SHOCK-medium", victim);
            }
            else if (eff1 == 4645) // alchemist's frost
            {
                dam = Dice.D8;
                dtype = DamageType.Cold;
                AttachParticles("hit-COLD-Burst", victim);
            }
            else
            {
                obj.Destroy();
                return;
            }

            victim.Damage(null, dtype, dam);
            victim.FloatMesFileLine("mes/combat.mes", 6500);

            obj.Destroy();
            return;
        }

    }
}
