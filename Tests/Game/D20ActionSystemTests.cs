using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
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
            GameSystems.Critter.GenerateHp(obj);
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

            GameSystems.D20.Actions.D20ActionTriggersAoO(action,tbStatus).Should().BeFalse();
        }
    }
}