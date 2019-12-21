
using SpicyTemple.Core.Systems.Spells;

var leader = GameSystems.Party.GetLeader();

var packet = new LevelupPacket();
packet.classCode = Stat.level_wizard;

packet.spellEnums.Clear();

for (var i = 0; i < 700; i++) {
    if (GameSystems.Spell.TryGetSpellEntry(i, out _)) {
        packet.spellEnums.Add(i);
    }
}
GameSystems.Level.LevelUpApply(leader, packet);
packet.spellEnums.Clear();

GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);
GameSystems.Level.LevelUpApply(leader, packet);

// Memorize some spells and make it so they aren't used
void MemorizeSpell(int spellEnum) {
    if (!GameSystems.Spell.TryGetSpellEntry(spellEnum, out var spellEntry)) {
        return;
    }

    var classCode = GameSystems.Spell.GetSpellClass(Stat.level_wizard);
    var level = spellEntry.SpellLevelForSpellClass(classCode);

    var spellsPerDay = GameSystems.Spell.GetSpellsPerDay(leader).Find(spd => spd.ClassCode == classCode);

    // Any free slots???
    if (level >= 0 && level < spellsPerDay.Levels.Length && spellsPerDay.Levels[level].Slots.Any(slot => !slot.HasSpell)) {
        GameSystems.Spell.SpellMemorizedAdd(leader, spellEnum, classCode, level, default, default);
    }
}
for (var i = 0; i < 700; i++) {
    if (GameSystems.Spell.IsSpellKnown(leader, i)) {
        MemorizeSpell(i);
    }
}
