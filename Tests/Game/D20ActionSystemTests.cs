using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Ui;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Game
{
    public class D20ActionSystemTests : HeadlessGameTest
    {
        [OneTimeSetUp]
        public void SetupScenario()
        {
            GameSystems.Map.OpenMap(5111, false, true, true).Should().BeTrue();
        }

        [TearDown]
        public void ClearArena()
        {
            ClearGameObjects();
        }

        private GameObjectBody Create(int protoId)
        {
            var obj = GameSystems.MapObject.CreateObject(protoId, new locXY(500, 483));
            if (obj.IsCritter())
            {
                GameSystems.Critter.GenerateHp(obj);
            }

            return obj;
        }

        /// <summary>
        /// Regression test for GH issue #42, where having natural weapons was not correctly considered
        /// in determining whether an attack triggers AoOs
        /// </summary>
        [Test]
        public void GiantRatCanAttackWithoutProvokingAoOs()
        {
            var giantRat = Create(TestProtos.GiantRat);
            giantRat.HasNaturalAttacks().Should().BeTrue();

            GameSystems.D20.Actions.AssignSeq(giantRat);

            var action = new D20Action(D20ActionType.STANDARD_ATTACK, giantRat);
            var tbStatus = new TurnBasedStatus();

            GameSystems.D20.Actions.D20ActionTriggersAoO(action, tbStatus).Should().BeFalse();
        }

        /// <summary>
        /// Tests that the lockpicking action is performed after moving into range, when clicking on the locked
        /// chest from afar.
        /// </summary>
        [Test]
        [RecordFailureVideo]
        public void ActionsArePerformedAfterMovingIntoRange()
        {
            // Spawn a player and a chest (at a distance), and click on the chest
            var player = CreatePlayer();
            var lockedChest = Create(TestProtos.LockedChest);
            ClickOn(lockedChest);

            // This should make the party leader perform the default action,
            // which is to open the container.
            Game.RunUntil(() => GameSystems.D20.Actions.IsCurrentlyPerforming(player));

            var sequence = GameSystems.D20.Actions.CurrentSequence ?? throw new NullReferenceException();
            sequence.performer.Should().Be(player);
            sequence.targetObj.Should().Be(lockedChest);

            // Run until the sequence has concluded
            Game.RunUntil(() => !GameSystems.D20.Actions.IsCurrentlyPerforming(player), 5000);

            // Since the player was not in reach, there should be a move action performed,
            // then the opening action
            CompletedActions.Should().SatisfyRespectively(
                first =>
                {
                    first.d20APerformer.Should().Be(player);
                    first.d20ActType.Should().Be(D20ActionType.MOVE);
                    // The target location should bring the player into reach of the container
                    first.destLoc.DistanceTo(lockedChest.GetLocationFull()).Should().BeLessOrEqualTo(
                        player.GetRadius()
                        + lockedChest.GetRadius()
                        // Reach returns feet
                        + player.GetReach(D20ActionType.OPEN_CONTAINER) * locXY.INCH_PER_FEET
                    );
                },
                second =>
                {
                    second.d20ActType.Should().Be(D20ActionType.OPEN_CONTAINER);
                    second.d20APerformer.Should().Be(player);
                    second.d20ATarget.Should().Be(lockedChest);
                }
            );
        }

        private GameObjectBody CreatePlayer(int x = 500, int y = 477)
        {
            var player = GameSystems.MapObject.CreateObject(TestProtos.HumanMalePlayer, new locXY(x, y));
            GameSystems.Party.AddToPCGroup(player);
            Game.RenderFrame(); // Have to render once to uncover fog of war
            return player;
        }
    }
}