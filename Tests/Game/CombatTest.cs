using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Game
{
    public class CombatTest : HeadlessGameTest
    {
        private GameObjectBody _player;

        [OneTimeSetUp]
        public void SetupScenario()
        {
            GameSystems.Map.OpenMap(5111, false, true, true).Should().BeTrue();
            GameSystems.Location.CenterOn(500, 477);
        }

        [SetUp]
        public void SetupParty()
        {
            // Always Setup a player
            _player = GameSystems.MapObject.CreateObject(TestProtos.HumanMalePlayer, new locXY(500, 477));
            GameSystems.Critter.GenerateHp(_player);
            GameSystems.Party.AddToPCGroup(_player);
        }

        [TearDown]
        public void ClearArena()
        {
            // Delete everything
            var toDelete = GameSystems.Object.GameObjects.ToList();
            toDelete.ForEach(GameSystems.MapObject.RemoveMapObj);
            // Wait for combat to stop
            Game.RunUntil(() => !GameSystems.Combat.IsCombatActive());
        }

        [Test]
        [TakeFailureScreenshot]
        public void CombatShouldNotStartWithoutEnemies()
        {
            GameSystems.Combat.IsCombatActive().Should().BeFalse();
        }

        [Test]
        [TakeFailureScreenshot]
        public void CombatShouldAutomaticallyStartWhenEnemyIsInRange()
        {
            var zombie = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 483));
            GameSystems.Critter.GenerateHp(zombie);

            Game.RunUntil(() => GameSystems.Combat.IsCombatActive());

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
            GameSystems.D20.Initiative.Should().BeEquivalentTo(
                _player,
                zombie
            );
        }

        [Test]
        [RecordFailureVideo(always:true)]
        public void CombatShouldWorkWithTwoOpponentsAndConcurrentTurns()
        {
            Globals.Config.ConcurrentTurnsEnabled = true;
            SetupScenarioForTwoZombies(out var zombie1, out var zombie2);

            // The zombies should perform simultaneously
            GameSystems.D20.Actions.isSimultPerformer(zombie1).Should().BeTrue();
            GameSystems.D20.Actions.isSimultPerformer(zombie2).Should().BeTrue();

            Game.RunUntil(() => GameSystems.D20.Initiative.CurrentActor == _player, 6000);
        }

        [Test]
        [RecordFailureVideo(always:true)]
        public void CombatShouldWorkWithTwoOpponentsAndSequentialTurns()
        {
            Globals.Config.ConcurrentTurnsEnabled = false;
            SetupScenarioForTwoZombies(out var zombie1, out var zombie2);

            // The zombies should not perform simultaneously
            GameSystems.D20.Actions.isSimultPerformer(zombie1).Should().BeFalse();
            GameSystems.D20.Actions.isSimultPerformer(zombie2).Should().BeFalse();

            Game.RunUntil(() => GameSystems.D20.Initiative.CurrentActor == _player, 6000);
        }

        private void SetupScenarioForTwoZombies(out GameObjectBody zombie1, out GameObjectBody zombie2)
        {
            // Ensure the player goes first by bumping the initiative bonus past 20
            _player.SetBaseStat(Stat.dexterity, 100);

            zombie1 = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 483));
            GameSystems.Critter.GenerateHp(zombie1);
            zombie2 = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 484));
            GameSystems.Critter.GenerateHp(zombie2);

            Game.RunUntil(() => GameSystems.Combat.IsCombatActive());

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
            GameSystems.D20.Initiative.CurrentActor.Should().Be(_player);
            GameSystems.D20.Initiative.Should().BeEquivalentTo(
                _player,
                zombie1,
                zombie2
            );

            // End turn for the player
            GameSystems.Combat.AdvanceTurn(_player);

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
            GameSystems.D20.Initiative.CurrentActor.Should().NotBe(_player);
        }
    }
}