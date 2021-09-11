using System;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
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
            ClearGameObjects();
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
        [RecordFailureVideo(always: true)]
        public void CombatShouldWorkWithTwoOpponentsAndConcurrentTurns()
        {
            Globals.Config.ConcurrentTurnsEnabled = true;
            SetupScenarioForTwoZombies(out var zombie1, out var zombie2);

            // The zombies should perform simultaneously
            GameSystems.D20.Actions.isSimultPerformer(zombie1).Should().BeTrue();
            GameSystems.D20.Actions.isSimultPerformer(zombie2).Should().BeTrue();

            Game.RunUntil(() => GameSystems.D20.Initiative.CurrentActor == _player, 10000);
        }

        [Test]
        [RecordFailureVideo(always: true)]
        public void CombatShouldWorkWithTwoOpponentsAndSequentialTurns()
        {
            Globals.Config.ConcurrentTurnsEnabled = false;
            SetupScenarioForTwoZombies(out var zombie1, out var zombie2);

            // The zombies should not perform simultaneously
            GameSystems.D20.Actions.isSimultPerformer(zombie1).Should().BeFalse();
            GameSystems.D20.Actions.isSimultPerformer(zombie2).Should().BeFalse();

            // It should be zombie1's turn, run the game until it's zombie2's turn and check what zombie1 did
            GameSystems.D20.Initiative.CurrentActor.Should().Be(zombie1);
            Game.RunUntil(() => GameSystems.D20.Initiative.CurrentActor == zombie2, 10000);
            CompletedActions.Should()
                .HaveCount(1)
                .And.Contain(action => action.d20APerformer == zombie1
                                       && action.d20ATarget == _player
                                       && action.d20ActType == D20ActionType.MOVE);
            ActionLog.Clear();

            // Now it's zombie2's turn, run until it's the player's turn and check what zombie2 did
            Game.RunUntil(() => GameSystems.D20.Initiative.CurrentActor == _player, 10000);
            CompletedActions.Should()
                .HaveCount(1)
                .And.Contain(action => action.d20APerformer == zombie2
                                       && action.d20ATarget == _player
                                       && action.d20ActType == D20ActionType.MOVE);

            // Since both zombies just moved, there should be no combat entries
            CombatLog.Should().BeEmpty();
        }

        private void SetupScenarioForTwoZombies(out GameObjectBody zombie1, out GameObjectBody zombie2)
        {
            // Initially, the logs should be empty.
            CombatLog.Should().BeEmpty();
            ActionLog.Should().BeEmpty();

            zombie1 = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 483));
            GameSystems.Critter.GenerateHp(zombie1);
            zombie2 = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 484));
            GameSystems.Critter.GenerateHp(zombie2);

            // Ensure a specific turn order
            SetInitiative(_player, 20);
            SetInitiative(zombie1, 10);
            SetInitiative(zombie2, 0);

            Game.RunUntil(() => GameSystems.Combat.IsCombatActive());

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
            GameSystems.D20.Initiative.CurrentActor.Should().Be(_player);
            GameSystems.D20.Initiative.Should().Equal(
                _player,
                zombie1,
                zombie2
            );

            // Without advancing time, no combat history entries should exist
            CombatLog.Should().BeEmpty();
            ActionLog.Should().BeEmpty();

            // End turn for the player
            GameSystems.Combat.AdvanceTurn(_player);

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
        }
    }
}