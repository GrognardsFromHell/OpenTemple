
using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace Scripts
{
    [ObjectScript(291)]
    public class AtropalScion : BaseObjectScript
    {
        public override bool OnDying(GameObjectBody attachee, GameObjectBody triggerer)
        {
            if (CombatStandardRoutines.should_modify_CR(attachee))
            {
                CombatStandardRoutines.modify_CR(attachee, CombatStandardRoutines.get_av_level());
            }

            return RunDefault;
        }
        public override bool OnEnterCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            AttachParticles("DesecrateEarth", attachee);
            foreach (var obj in PartyLeader.GetPartyMembers())
            {
                if (obj.DistanceTo(attachee) <= 60 || attachee.HasLineOfSight(obj))
                {
                    if (!obj.HasCondition(SpellEffects.SpellProtectionFromAlignment) && !obj.IsMonsterCategory(MonsterCategory.undead))
                    {
                        var alignment = obj.GetAlignment();
                        if (((alignment.IsEvil())))
                        {
                            AttachParticles("Barbarian Rage-end", obj);
                            Utilities.create_item_in_inventory(12752, obj);
                            Utilities.create_item_in_inventory(12752, obj);
                            obj.FloatMesFileLine("mes/combat.mes", 6016);
                        }
                        else if (((alignment.IsGood())))
                        {
                            AttachParticles("Barbarian Rage-end", obj);
                            Utilities.create_item_in_inventory(12750, obj);
                            Utilities.create_item_in_inventory(12750, obj);
                            obj.FloatMesFileLine("mes/combat.mes", 6016);
                        }
                        else if (((alignment.IsLawful())))
                        {
                            AttachParticles("Barbarian Rage-end", obj);
                            Utilities.create_item_in_inventory(12749, obj);
                            Utilities.create_item_in_inventory(12749, obj);
                            obj.FloatMesFileLine("mes/combat.mes", 6016);
                        }
                        else if (((alignment.IsChaotic())))
                        {
                            AttachParticles("Barbarian Rage-end", obj);
                            Utilities.create_item_in_inventory(12751, obj);
                            Utilities.create_item_in_inventory(12751, obj);
                            obj.FloatMesFileLine("mes/combat.mes", 6016);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnStartCombat(GameObjectBody attachee, GameObjectBody triggerer)
        {
            foreach (var blech in ObjList.ListVicinity(attachee.GetLocation(), ObjectListFilter.OLC_NPC))
            {
                // game.particles( 'sp-Bless Water', blech )
                if ((blech.DistanceTo(attachee) <= 60))
                {
                    // game.particles( 'DesecrateEarth', attachee )
                    if (blech.IsMonsterCategory(MonsterCategory.undead))
                    {
                        var dice = Dice.Parse("5d1");
                        blech.Heal(attachee, dice);
                        if (blech.GetStat(Stat.hp_current) > 0)
                        {
                            AttachParticles("sp-Curse Water", blech);
                        }

                    }

                }

            }

            return RunDefault;
        }
        public override bool OnInsertItem(GameObjectBody attachee, GameObjectBody triggerer)
        {
            StartTimer(20000, () => make_it_gone(attachee));
            return RunDefault;
        }
        public static void make_it_gone(GameObjectBody attachee)
        {
            attachee.Destroy();
            // game.particles( "sp-summon monster I", game.party[0] )
            return;
        }

    }
}
