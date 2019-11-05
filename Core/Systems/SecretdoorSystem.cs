using System;
using System.Collections.Generic;
using System.Diagnostics;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.Protos;
using SpicyTemple.Core.Systems.TimeEvents;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public class SecretdoorSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

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
        }

        [TempleDllLocation(0x100463b0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10046400)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10046b70)]
        public void AfterTeleportStuff(locXY loc)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10046ea0)]
        public void QueueSearchTimer(GameObjectBody obj)
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
        private readonly List<int> _nameListSeen = new List<int>();

        [TempleDllLocation(0x10046550)]
        public void MarkUsed(GameObjectBody target)
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

        private bool IsSecretDoor(GameObjectBody obj)
        {
            return obj != null && obj.GetSecretDoorFlags().HasFlag(SecretDoorFlag.SECRET_DOOR);
        }

        [TempleDllLocation(0x100464a0)]
        private bool IsSecretDoorDetected(GameObjectBody obj)
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
        public int GetSecretDoorDc(GameObjectBody obj)
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
        public bool TryFindSecretDoor(GameObjectBody door, GameObjectBody seeker, BonusList searchBonus)
        {
            if (!IsSecretDoor(door) || GameSystems.D20.D20Query(seeker, D20DispatcherKey.QUE_CannotUseIntSkill) )
            {
                return false;
            }

            var dc = GetSecretDoorDc(door);

            var bonus = seeker.dispatch1ESkillLevel(SkillId.search, ref searchBonus, door, SkillCheckFlags.SearchForSecretDoors);

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
        public bool SecretDoorSpotted(GameObjectBody door, GameObjectBody viewer)
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
}