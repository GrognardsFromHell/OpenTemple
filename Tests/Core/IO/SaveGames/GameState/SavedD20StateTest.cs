using System.IO;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Utils;

namespace OpenTemple.Tests.Core.IO.SaveGames.GameState;

public class SavedD20StateTest
{
    [Test]
    public void TestEmptyHotkeyRoundtrip()
    {
        TestRoundtrip(new SavedHotkeys());
    }

    [Test]
    public void TestHotkeyRoundtrip()
    {
        var savedHotkeys = new SavedHotkeys();
        // An entry without spell data
        savedHotkeys.Hotkeys.Add(new SavedHotkey
        {
            Id = "user_3",
            Text = "Radial Menu Text",
            ActionData = 123,
            ActionType = D20ActionType.UNSPECIFIED_MOVE,
            SpellData = null,
            TextHash = ElfHash.Hash("Radial Menu Text")
        });
        // An entry with spell data
        savedHotkeys.Hotkeys.Add(new SavedHotkey
        {
            Id = "user_9",
            Text = "Other Radialmenu Text",
            ActionData = 321,
            ActionType = D20ActionType.UNSPECIFIED_ATTACK,
            SpellData = new D20SpellData(123, 2, 3),
            TextHash = ElfHash.Hash("Other Radialmenu Text")
        });

        TestRoundtrip(savedHotkeys);
    }

    private static void TestRoundtrip(SavedHotkeys savedHotkeys)
    {
        var stream = new MemoryStream();
        var writer = new BinaryWriter(stream);
        savedHotkeys.Save(writer);

        var writtenBytes = stream.Position;

        stream.Position = 0;
        var reader = new BinaryReader(stream);
        var loadedHotkeys = SavedHotkeys.Load(reader);

        // Make sure the reader did read all bytes
        stream.Position.Should().Be(writtenBytes);

        loadedHotkeys.Should().BeEquivalentTo(savedHotkeys);
    }
}