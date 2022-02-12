using System;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.TestTools;

/// <summary>
/// Tries leveling up a character in a specific character class with required choices being made
/// automatically.
/// </summary>
public static class AutoLevelUp
{
    public static void LevelUp(GameObject critter, Stat classStat)
    {
        if (!D20ClassSystem.Classes.ContainsKey(classStat))
        {
            throw new ArgumentException($"{classStat} is not a valid class");
        }

        var packet = new LevelupPacket
        {
            classCode = classStat
        };

        var classCode = SpellSystem.GetSpellClass(classStat);
        foreach (var spell in GameSystems.Spell.AllSpells)
        {
            if (spell.SpellLevelForSpellClass(classCode) != -1)
            {
                packet.spellEnums.Add(spell.spellEnum);
            }
        }

        GameSystems.Level.LevelUpApply(critter, packet);
        packet.spellEnums.Clear();

        // Memorize some spells and make it so they aren't used
        foreach (var spell in GameSystems.Spell.AllSpells)
        {
            if (GameSystems.Spell.IsSpellKnown(critter, spell.spellEnum))
            {
                MemorizeSpell(critter, spell.spellEnum, classStat);
            }
        }
    }

    private static void MemorizeSpell(GameObject critter, int spellEnum, Stat classStat)
    {
        if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry))
        {
            return;
        }

        var classCode = SpellSystem.GetSpellClass(classStat);
        var level = spellEntry.SpellLevelForSpellClass(classCode);

        var spellsPerDay = GameSystems.Spell.GetSpellsPerDay(critter).Find(spd => spd.ClassCode == classCode);
        if (spellsPerDay == null)
        {
            return;
        }

        // Any free slots???
        if (level >= 0 && level < spellsPerDay.Levels.Length &&
            spellsPerDay.Levels[level].Slots.Any(slot => !slot.HasSpell))
        {
            GameSystems.Spell.SpellMemorizedAdd(critter, spellEnum, classCode, level, default, default);
        }
    }
}