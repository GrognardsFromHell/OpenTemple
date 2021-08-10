using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Tests.TestUtils;
using SixLabors.ImageSharp;

namespace OpenTemple.Tests.Game
{
    public class CombatTest : HeadlessGameTest
    {
        private GameObjectBody _player;
        private GameObjectBody _npc;

        [OneTimeSetUp]
        public void SetupScenario()
        {
            GameSystems.Map.OpenMap(5111, false, true, true).Should().BeTrue();
            GameSystems.Location.CenterOn(500, 477);

            _player = GameSystems.MapObject.CreateObject(TestProtos.HumanMalePlayer, new locXY(500, 477));
            GameSystems.Critter.GenerateHp(_player);
            GameSystems.Party.AddToPCGroup(_player);
            _npc = GameSystems.MapObject.CreateObject(TestProtos.Zombie, new locXY(500, 483));
            _npc.SetNpcFlag(NpcFlag.KOS);
            GameSystems.Critter.GenerateHp(_npc);
        }

        [Test]
        public void CombatHasInitiallyNotStarted()
        {
            GameSystems.Combat.IsCombatActive().Should().BeFalse();
            Game.RenderFrame();
            Game.TakeScreenshot().SaveAsPng("combat.png");
        }

        [Test]
        public void CombatShouldAutomaticallyStartWhenTimeIsAdvanced()
        {
            Game.RunFor(1000);
            Game.TakeScreenshot().SaveAsPng("combat.png");

            GameSystems.Combat.IsCombatActive().Should().BeTrue();
        }
    }
}