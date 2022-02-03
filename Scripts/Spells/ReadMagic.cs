
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

namespace Scripts.Spells
{
    [SpellScript(385)]
    public class ReadMagic : BaseSpellScript
    {
        public override void OnBeginSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Read Magic OnBeginSpellCast");
            Logger.Info("spell.target_list={0}", spell.Targets);
            Logger.Info("spell.caster={0} caster.level= {1}", spell.caster, spell.casterLevel);
        }
        // game.particles( "sp-divination-conjure", spell.caster )

        public override void OnSpellEffect(SpellPacketBody spell)
        {
            Logger.Info("Read Magic OnSpellEffect");
            spell.duration = 0;
            var target_item = spell.Targets[0];
            AttachParticles("sp-Read Magic", spell.caster);
            target_item.Object.SetItemFlag(ItemFlag.IDENTIFIED);
            spell.RemoveTarget(target_item.Object);
            spell.EndSpell();
            // NEW! Identifies all scrolls and potions when cast in safe area
            var idCount = 0;
            if (((GameSystems.Party.PartyMembers).Contains(spell.caster)))
            {
                var sleepStatus = GameSystems.RandomEncounter.SleepStatus;
                if ((sleepStatus == SleepStatus.Safe || sleepStatus == SleepStatus.PassTimeOnly))
                {
                    foreach (var dude in GameSystems.Party.PartyMembers)
                    {
                        Logger.Info("{0}", "IDing potions and scrolls for " + dude.ToString());
                        for (var q = 0; q < 24; q++)
                        {
                            var item = dude.GetInventoryItem(q);
                            if ((item != null))
                            {
                                Logger.Info("{0}", "Item: " + item.ToString());
                                if ((item.type == ObjectType.food || item.type == ObjectType.scroll))
                                {
                                    item.SetItemFlag(ItemFlag.IDENTIFIED);
                                    idCount = idCount + 1;
                                }

                            }

                        }

                    }

                }

            }

            Logger.Info("{0}", "Finished IDing, item count: " + idCount.ToString());
        }
        public override void OnBeginRound(SpellPacketBody spell)
        {
            Logger.Info("Read Magic OnBeginRound");
        }
        public override void OnEndSpellCast(SpellPacketBody spell)
        {
            Logger.Info("Read Magic OnEndSpellCast");
        }

    }
}
