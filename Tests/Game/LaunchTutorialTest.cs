using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Game
{
    public class LaunchTutorialTest : HeadlessGameTest
    {
        [OneTimeSetUp]
        public static void LaunchTutorial()
        {
            UiSystems.MainMenu.LaunchTutorial();
            Game.RunFor(1000);
        }

        [Test]
        public void TutorialMapIsLoaded()
        {
            // The tutorial map
            GameSystems.Map.GetCurrentMapId().Should().Be(5116);
        }

        [Test]
        public void TutorialMapObjectsAreLoaded()
        {
            var objectsOnMap = GameSystems.Object.GameObjects
                // The party itself should be DYNAMIC, hence we exclude it here
                .Where(o => !o.HasFlag(ObjectFlag.DYNAMIC))
                .Select(obj => obj.ProtoId);

            objectsOnMap.Should().BeEquivalentTo(
                1048, // Tutorial Chest A (container)
                14433, // Giant Rat (npc)
                14434, // Tutorial Room1 (npc)
                14437, // Tutorial Room4 (npc)
                6067, // Wooden orcish shield (armor)
                4040, // Longsword (weapon)
                14440, // Passage Icon (npc)
                10021, // Small gold Key (key)
                14436, // Tutorial Room3 (npc)
                14435, // Tutorial Room2 (npc)
                105, // Door (portal)
                105, // Door (portal)
                2012 // Ladder Down Icon (scenery)
            );
        }

        [Test]
        public void PlayerCharacterIsOnMap()
        {
            var partyObjects = GameSystems.Object.GameObjects
                .Where(o => o.type == ObjectType.pc)
                .ToList();
            partyObjects.Should().HaveCount(1);

            var playerCharacter = partyObjects[0];
            playerCharacter.ProtoId.Should().Be(13105);
            GameSystems.Party.PartyMembers.Should().BeEquivalentTo(playerCharacter);
        }

        [Test]
        public void PlayerCharacterHasEquipment()
        {
            var pc = GameSystems.Party.PartyMembers.First();
            pc.EnumerateEquipment()
                .Select(kvp => (kvp.Key, kvp.Value.ProtoId))
                .Should().BeEquivalentTo(
                    (EquipSlot.Helmet, 6032),
                    (EquipSlot.Gloves, 6012),
                    (EquipSlot.Armor, 6047),
                    (EquipSlot.Boots, 6011)
                );
        }
    }
}