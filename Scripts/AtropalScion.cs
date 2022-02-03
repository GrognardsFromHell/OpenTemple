
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
    [ObjectScript(291)]
    public class AtropalScion : BaseObjectScript
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
        public override bool OnStartCombat(GameObject attachee, GameObject triggerer)
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
        public override bool OnInsertItem(GameObject attachee, GameObject triggerer)
        {
            StartTimer(20000, () => make_it_gone(attachee));
            return RunDefault;
        }
        public static void make_it_gone(GameObject attachee)
        {
            attachee.Destroy();
            // game.particles( "sp-summon monster I", game.party[0] )
            return;
        }

    }
}
