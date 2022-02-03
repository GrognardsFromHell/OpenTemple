
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
    [ObjectScript(205)]
    public class NodeVrock : BaseObjectScript
    {
        public override bool OnDialog(GameObject attachee, GameObject triggerer)
        {
            triggerer.BeginDialog(attachee, 1);
            return SkipDefault;
        }
        public override bool OnDying(GameObject attachee, GameObject triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            SetGlobalVar(30, GetGlobalVar(30) + 1);
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
            // if (attachee.name == 14361 or attachee.name == 14258):
            // if (game.global_vars[762] == 0):
            // damage_dice = dice_new( '1d8' )
            // game.particles( 'Mon-Vrock-Spores', attachee)
            // for obj in game.obj_list_vicinity(attachee.location,OLC_CRITTERS):
            // if (obj.distance_to(attachee) <= 10 and obj.name != 14258 and obj.name != 14259 and obj.name != 14263 and obj.name != 14286 and obj.name != 14358 and obj.name != 14359 and obj.name != 14360 and obj.name != 14361 and obj.name != 14110):
            // obj.spell_damage( attachee, D20DT_POISON, damage_dice, D20DAP_UNSPECIFIED, D20A_CAST_SPELL, 261 )
            // obj.condition_add_with_args( 'sp-Vrock Spores', 273, 10, 0)
            // game.particles( 'Mon-Vrock-Spores-Hit', obj )
            // game.global_vars[762] = game.global_vars[762] + 1
            // if (game.global_vars[762] == 3):
            // game.global_vars[762] = 0
            return RunDefault;
        }
        public override bool OnHeartbeat(GameObject attachee, GameObject triggerer)
        {
            if ((!GameSystems.Combat.IsCombatActive()))
            {
                foreach (var obj in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_PC))
                {
                    if ((Utilities.is_safe_to_talk(attachee, obj)))
                    {
                        attachee.TurnTowards(obj);
                        obj.BeginDialog(attachee, 1);
                        DetachScript();
                        return RunDefault;
                    }

                }

            }

            // if (game.global_vars[712] == 0 and attachee.leader_get() == OBJ_HANDLE_NULL and attachee.distance_to(party_closest(attachee) ) <= 30  ):
            // attachee.spells_pending_to_memorized()
            // attachee.cast_spell(spell_heroism, attachee)
            // game.global_vars[712] = 1
            // The motherfucker doesn't want to cast spells out of combat. Hmpf!
            return RunDefault;
        }

    }
}
