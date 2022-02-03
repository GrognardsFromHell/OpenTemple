using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;

namespace OpenTemple.Core.IO.SaveGames.GameState;

public class SavedPartyState
{
    public ObjectId[] PartyMembers { get; set; }

    public ObjectId[] Selected { get; set; }

    public ObjectId[] PCs { get; set; }

    public ObjectId[] ControlledFollowers { get; set; }

    public ObjectId[] UncontrolledFollowers { get; set; }

    public ObjectId[] D20Registry { get; set; }

    public Alignment Alignment { get; set; }

    // Party money (TODO: double check)
    public int PlatinumCoins { get; set; }
    public int GoldCoins { get; set; }
    public int SilverCoins { get; set; }
    public int CopperCoins { get; set; }

    // Enable audible confirmations when giving commands
    public bool IsVoiceConfirmEnabled { get; set; }

    [TempleDllLocation(0x1002ad80)]
    public static SavedPartyState Read(BinaryReader reader)
    {
        var result = new SavedPartyState();
        result.PartyMembers = LoadGroup(reader);
        result.Selected = LoadGroup(reader);
        result.PCs = LoadGroup(reader);
        result.ControlledFollowers = LoadGroup(reader);
        result.UncontrolledFollowers = LoadGroup(reader);
        result.D20Registry = LoadGroup(reader);

        result.Alignment = (Alignment) reader.ReadInt32(); // TODO: Extension method might be better
        result.PlatinumCoins = reader.ReadInt32();
        result.GoldCoins = reader.ReadInt32();
        result.SilverCoins = reader.ReadInt32();
        result.CopperCoins = reader.ReadInt32();
        result.IsVoiceConfirmEnabled = reader.ReadInt32() != 0;

        return result;
    }

    [TempleDllLocation(0x1002ac70)]
    public void Write(BinaryWriter writer)
    {
        SaveGroup(writer, PartyMembers);
        SaveGroup(writer, Selected);
        SaveGroup(writer, PCs);
        SaveGroup(writer, ControlledFollowers);
        SaveGroup(writer, UncontrolledFollowers);
        SaveGroup(writer, D20Registry);

        writer.WriteInt32((int) Alignment); // TODO: Extension method might be better
        writer.WriteInt32(PlatinumCoins);
        writer.WriteInt32(GoldCoins);
        writer.WriteInt32(SilverCoins);
        writer.WriteInt32(CopperCoins);
        writer.WriteInt32(IsVoiceConfirmEnabled ? 1 : 0);
    }

    private static ObjectId[] LoadGroup(BinaryReader reader)
    {
        var count = reader.ReadInt32();
        var result = new ObjectId[count];
        for (var i = 0; i < result.Length; i++)
        {
            result[i] = reader.ReadObjectId();
        }

        return result;
    }

    private static void SaveGroup(BinaryWriter writer, ICollection<ObjectId> group)
    {
        writer.WriteInt32(group.Count);
        foreach (var objectId in group)
        {
            writer.WriteObjectId(objectId);
        }
    }
}