using System;
using System.Collections.Generic;
using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.Protos;
using OpenTemple.Core.Systems.TimeEvents;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems;

public class SecretdoorSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x109dda10)]
    private bool zuggtmoyFound = false;

    [TempleDllLocation(0x10046370)]
    public SecretdoorSystem()
    {
    }

    public void Dispose()
    {
    }

    [TempleDllLocation(0x10046390)]
    public void Reset()
    {
        GameSystems.TimeEvent.RemoveAll(TimeEventType.Search);
        zuggtmoyFound = false;

        // ToEE forgot to reset this:
        _nameListSeen.Clear();
    }

    [TempleDllLocation(0x100463b0)]
    public void SaveGame(SavedGameState savedGameState)
    {
        savedGameState.SecretDoorState = new SavedSecretDoorState
        {
            SeenSceneryNames = new List<int>(_nameListSeen)
        };
    }

    [TempleDllLocation(0x10046400)]
    public void LoadGame(SavedGameState savedGameState)
    {
        _nameListSeen.Clear();
        _nameListSeen.AddRange(savedGameState.SecretDoorState.SeenSceneryNames);
    }

    [TempleDllLocation(0x10046b70)]
    public void AfterTeleportStuff(locXY loc)
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10046ea0)]
    public void QueueSearchTimer(GameObject obj)
    {
        GameSystems.TimeEvent.Remove(TimeEventType.Search, evt => evt.arg1.handle == obj);

        var newEvt = new TimeEvent(TimeEventType.Search);
        newEvt.arg1.handle = obj;
        newEvt.arg2.int32 = 1;
        GameSystems.TimeEvent.ScheduleNow(newEvt);
    }

    // I think this is used for making both ends of a secret passage known to the player
    // after they have detected one end of it.
    // TODO: Validate this. They should actually only get to know the other end once they've used the secret passage
    [TempleDllLocation(0x109DD880)]
    private readonly List<int> _nameListSeen = new();

    [TempleDllLocation(0x10046550)]
    public void MarkUsed(GameObject target)
    {
        var nameId = target.GetInt32(obj_f.name);
        var typeOeName = ProtoSystem.GetOeNameIdForType(target.type);
        if (nameId != typeOeName)
        {
            if (!_nameListSeen.Contains(nameId))
            {
                _nameListSeen.Add(nameId);
            }
        }
    }

    private bool IsSecretDoor(GameObject obj)
    {
        return obj != null && obj.GetSecretDoorFlags().HasFlag(SecretDoorFlag.SECRET_DOOR);
    }

    [TempleDllLocation(0x100464a0)]
    private bool IsSecretDoorDetected(GameObject obj)
    {
        if (!IsSecretDoor(obj))
        {
            return false;
        }

        if (obj.GetSecretDoorFlags().HasFlag(SecretDoorFlag.SECRET_DOOR_FOUND))
        {
            return true;
        }

        var nameId = obj.GetInt32(obj_f.name);
        return _nameListSeen.Contains(nameId);
    }

    [TempleDllLocation(0x10046510)]
    public int GetSecretDoorDc(GameObject obj)
    {
        if (IsSecretDoor(obj))
        {
            return obj.GetInt32(obj_f.secretdoor_dc) & 0x7F;
        }
        else
        {
            return 0;
        }
    }

    [TempleDllLocation(0x10046db0)]
    public bool TryFindSecretDoor(GameObject door, GameObject seeker, BonusList searchBonus)
    {
        if (!IsSecretDoor(door) || GameSystems.D20.D20Query(seeker, D20DispatcherKey.QUE_CannotUseIntSkill))
        {
            return false;
        }

        var dc = GetSecretDoorDc(door);

        var bonus = seeker.dispatch1ESkillLevel(SkillId.search, ref searchBonus, door,
            SkillCheckFlags.SearchForSecretDoors);

        // Seems to assume take 20 on search for secret doors... ok?!
        var dice = Dice.Constant(20 + bonus);
        var roll = dice.Roll();
        GameSystems.RollHistory.AddSkillCheck(seeker, null, SkillId.search, dice, roll, dc, searchBonus);

        if (roll >= dc)
        {
            return SecretDoorSpotted(door, seeker);
        }

        return false;
    }

    [TempleDllLocation(0x10046920)]
    public bool SecretDoorSpotted(GameObject door, GameObject viewer)
    {
        if (IsSecretDoor(door) || IsSecretDoorDetected(door) || !viewer.IsPC())
        {
            return false;
        }

        door.SetSecretDoorFlags(door.GetSecretDoorFlags() | SecretDoorFlag.SECRET_DOOR_FOUND);
        MarkUsed(door);

        // Fade out existing objects of the secret door effect.
        var effectName = door.GetInt32(obj_f.secretdoor_effectname);
        using var listResult = ObjList.ListVicinity(door, ObjectListFilter.OLC_ALL);
        foreach (var closeObj in listResult)
        {
            if (closeObj.GetInt32(obj_f.name) == effectName)
            {
                GameSystems.ObjFade.FadeTo(closeObj, 0, 10, 2, 0);
            }
        }

        var animatedModel = door.GetOrCreateAnimHandle();
        if (animatedModel == null)
        {
            Logger.Info("Cannot start secret door reveal effect, no animation exists for {0}", door);
            return false;
        }

        for (var i = 0; i < 30; i++)
        {
            // The bones on the object are named the same as the particle systems, which is weird...
            var particleSysName = $"effect-secretdoor-{i:00}";
            if (animatedModel.HasBone(particleSysName))
            {
                GameSystems.ParticleSys.CreateAtObj(particleSysName, door);
            }
        }

        if (door.type != ObjectType.portal)
        {
            GameSystems.MapObject.SetTransparency(door, 0);
            GameSystems.ObjFade.FadeTo(door, 255, 10, 2, 0);
        }

        var text = GameSystems.Skill.GetSkillUiMessage(1000);
        GameSystems.TextFloater.FloatLine(door, TextFloaterCategory.Generic, TextFloaterColor.Blue, text);
        return true;
    }
}