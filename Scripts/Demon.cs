
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

[ObjectScript(309)]
public class Demon : BaseObjectScript
{
    public override bool OnDying(GameObject attachee, GameObject triggerer)
    {
        if (CombatStandardRoutines.should_modify_CR(attachee))
        {
            CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
        }

        if ((attachee.GetNameId() == 14342 && attachee.FindItemByName(4083) == null)) // Lamia
        {
            Utilities.create_item_in_inventory(4083, attachee);
        }

        return RunDefault;
    }
    public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
    {
        while ((attachee.FindItemByName(8903) != null))
        {
            attachee.FindItemByName(8903).Destroy();
        }

        // if (attachee.d20_query(Q_Is_BreakFree_Possible)): # workaround no longer necessary!
        // create_item_in_inventory( 8903, attachee )
        if ((attachee.GetNameId() == 14328 && RandomRange(1, 100) <= 40)) // Bodak Death Gaze
        {
            attachee.PendingSpellsToMemorized();
        }

        if ((attachee.GetNameId() == 14309 && RandomRange(1, 100) <= 25)) // Gorgon Breath Attack
        {
            attachee.PendingSpellsToMemorized();
        }

        if ((attachee.GetNameId() == 14109 && RandomRange(1, 100) <= 25)) // Ice Lizard Breath Attack
        {
            attachee.PendingSpellsToMemorized();
        }

        if ((attachee.GetNameId() == 14342 && attachee.HasEquippedByName(4083))) // Lamia
        {
            attachee.FindItemByName(4083).Destroy();
            return RunDefault;
        }

        if ((attachee.GetNameId() == 14342 && attachee.FindItemByName(4083) == null && RandomRange(1, 100) <= 50))
        {
            Utilities.create_item_in_inventory(4083, attachee);
            attachee.WieldBestInAllSlots();
        }

        if ((attachee.GetNameId() == 14295 && !attachee.D20Query(D20DispatcherKey.QUE_Critter_Is_Blinded))) // Basilisk
        {
            attachee.PendingSpellsToMemorized();
        }

        if ((attachee.GetNameId() == 14258)) // Guardian Vrock
        {
            SetGlobalVar(762, GetGlobalVar(762) + 1);
            if ((GetGlobalVar(762) >= 3))
            {
                var damage_dice = Dice.D8;
                AttachParticles("Mon-Vrock-Spores", attachee);
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_CRITTERS))
                {
                    if ((obj.DistanceTo(attachee) <= 10 && obj.GetNameId() != 14258 && obj.GetNameId() != 14361))
                    {
                        obj.DealSpellDamage(attachee, DamageType.Poison, damage_dice, D20AttackPower.UNSPECIFIED, D20ActionType.CAST_SPELL, 261);
                        AttachParticles("Mon-Vrock-Spores-Hit", obj);
                    }

                }

            }

        }

        // damage_dice = dice_new( '1d8' )
        // game.particles( 'Mon-Vrock-Spores', attachee)
        // for obj in game.obj_list_vicinity(attachee.location,OLC_CRITTERS):
        // if (obj.distance_to(attachee) <= 10 and obj.name != 14258):
        // obj.damage( OBJ_HANDLE_NULL, D20DT_POISON, damage_dice, D20DAP_NORMAL)
        // obj.spell_damage( attachee, D20DT_POISON, damage_dice, D20DAP_UNSPECIFIED, D20A_CAST_SPELL, 261 )
        // obj.condition_add_with_args( "Poisoned", 273 , 10)
        // obj.condition_add_with_args( 'sp-Vrock Spores', 273, 10, 0)
        // game.particles( 'Mon-Vrock-Spores-Hit', obj )
        // if ((attachee.name == 14259 or attachee.name == 14360) and game.random_range(1,3) <= 3):
        // create_item_in_inventory( 8909, attachee )
        // Spiritual Weapon Shenanigens	#
        CombatStandardRoutines.Spiritual_Weapon_Begone(attachee);
        return RunDefault;
    }

}