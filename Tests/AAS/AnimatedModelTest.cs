using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.AAS;
using OpenTemple.Core.GFX;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.AAS
{
    public class AnimatedModelTest : RealGameFiles
    {
        private AnimatedModelFactory _amf;

        [SetUp]
        public void SetUp()
        {
            var meshTable = new Dictionary<int, string>()
            {
                { 1, "PCs/PC_Human_Male/PC_Human_Male" }
            };
            _amf = new AnimatedModelFactory(
                Tig.FS,
                meshTable,
                s => s
            );
        }

        [TearDown]
        public void TearDown()
        {
            _amf.Dispose();
        }

        [Test]
        public void TestSimplePlayback()
        {
            var model = _amf.FromIds(1, 1, new EncodedAnimId(WeaponAnim.LeftAttack), AnimatedModelParams.Default);

            var frames = 0;
            while (true)
            {
                var evt = model.Advance(1 / 30.0f, 0, 0, AnimatedModelParams.Default);
                frames += 1;
                if (evt.IsEnd)
                {
                    break;
                }
            }

            frames.Should().Be(20);
        }
    }
}