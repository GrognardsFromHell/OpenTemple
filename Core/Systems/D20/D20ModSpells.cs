using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.D20
{
    public static class D20ModSpells
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x100d3410)]
        [TempleDllLocation(0x100c3810)]
        public static bool CheckSpellResistance(GameObjectBody target, SpellPacketBody spellPkt)
        {
            // check spell immunity
            DispIoImmunity dispIo = DispIoImmunity.Default;
            dispIo.flag = 1;
            dispIo.spellPkt = spellPkt;
            if (!GameSystems.Spell.TryGetSpellEntry(spellPkt.spellEnum, out dispIo.spellEntry))
            {
                return false;
            }

            if (target.Dispatch64ImmunityCheck(dispIo))
            {
                return true;
            }

            // does spell allow saving?
            if (dispIo.spellEntry.spellResistanceCode != SpellResistanceType.Yes)
            {
                return false;
            }

            // obtain bonuses

            // Defender bonus
            int srMod = target.Dispatch45SpellResistanceMod(dispIo.spellEntry);
            if (srMod <= 0)
            {
                return false;
            }

            var bonlist = BonusList.Create();
            var caster = spellPkt.caster;

            var casterLvlMod = caster.Dispatch35CasterLevelModify(spellPkt);
            bonlist.AddBonus(casterLvlMod, 0, 203);

            // TODO: Why are these feats not handled by subdispatchers for SpellResistanceCasterLevelCheck??? in the feat conditions
            if (caster.HasFeat(FeatId.SPELL_PENETRATION))
            {
                bonlist.AddBonusFromFeat(2, 0, 114, FeatId.SPELL_PENETRATION);
            }

            if (caster.HasFeat(FeatId.GREATER_SPELL_PENETRATION))
            {
                bonlist.AddBonusFromFeat(2, 0, 114, FeatId.GREATER_SPELL_PENETRATION);
            }

            // New Spell resistance mod
            caster.DispatchSpellResistanceCasterLevelCheck(target, bonlist, spellPkt);

            // do the roll and log the result to the D20 window
            var spellResistanceText = GameSystems.D20.Combat.GetCombatMesLine(5048);
            var dispelSpellResistanceResult = GameSystems.Spell.DispelRoll(spellPkt.caster, bonlist, 0, srMod,
                spellResistanceText, out var rollHistId);
            string outcomeText1, outcomeText2;
            if (dispelSpellResistanceResult < 0)
            {
                // fixed bug - was <= instead of <
                var spellName = GameSystems.Spell.GetSpellName(spellPkt.spellEnum);
                Logger.Info("CheckSpellResistance: Spell {0} cast by {1} resisted by target {2}.", spellName,
                    GameSystems.MapObject.GetDisplayName(spellPkt.caster),
                    GameSystems.MapObject.GetDisplayName(target));
                GameSystems.Spell.FloatSpellLine(target, 30008, TextFloaterColor.White);
                GameSystems.Spell.PlayFizzle(target);
                outcomeText1 = GameSystems.D20.Combat.GetCombatMesLine(119); // Spell ~fails~[ROLL_
                outcomeText2 = GameSystems.D20.Combat.GetCombatMesLine(120); // ] to overcome Spell Resistance
            }
            else
            {
                GameSystems.Spell.FloatSpellLine(target, 30009, TextFloaterColor.Red);
                outcomeText1 = GameSystems.D20.Combat.GetCombatMesLine(121); // Spell ~overcomes~[ROLL_
                outcomeText2 = GameSystems.D20.Combat.GetCombatMesLine(122); // ] Spell Resistance
            }

            var histText = $"{outcomeText1}{rollHistId}{outcomeText2}\n\n";
            GameSystems.RollHistory.CreateFromFreeText(histText);

            return dispelSpellResistanceResult <= 0;
        }
    }
}