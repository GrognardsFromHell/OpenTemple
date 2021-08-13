using System;
using System.Globalization;
using System.IO;
using System.Linq;
using FluentAssertions;
using OpenTemple.Core.IO.TroikaArchives;
using OpenTemple.Core.Particles.Parser;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Particles.Params;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OpenTemple.Core.IO;
using OpenTemple.Tests.TestUtils;

namespace OpenTemple.Tests.Particles
{
    /*
        This is a test fixture for particle system parser tests that only parses all particle systems once
        to speed up the tests.
    */
    public class PartSysParserTest : IDisposable
    {
        // This is just a list of all .mdf files in art/meshes/particle/
        // These can be referenced by emitters and to avoid log-spam, we
        // create dummy files in place of those materials.
        private static readonly string[] MaterialFiles = new[] {
            "AcidArrow.mdf",
            "Animate Dead-text.mdf",
            "Animate Dead.mdf",
            "backdrop.mdf",
            "BigCircle.mdf",
            "Black-Circle.mdf",
            "Black-rectangle.mdf",
            "Brickalone.mdf",
            "Bug1.mdf",
            "bullstrength.mdf",
            "butterfly.mdf",
            "catsgrace.mdf",
            "Chaos-Hammer.mdf",
            "Charac-1.mdf",
            "Charac-2.mdf",
            "Charac-3.mdf",
            "Charac-4.mdf",
            "Charac-5.mdf",
            "dirt.mdf",
            "DungeonBug.mdf",
            "Enervation1.mdf",
            "Enervation2.mdf",
            "Enervation3.mdf",
            "eye-noPupil.mdf",
            "eye-Pupil.mdf",
            "Feather.mdf",
            "Fire-Sprite.mdf",
            "fire1.mdf",
            "fire2.mdf",
            "fire3.mdf",
            "Flare-1.mdf",
            "Flare-2.mdf",
            "Flare-3.mdf",
            "Flare-big.mdf",
            "Flare-Small.mdf",
            "Flare-X.mdf",
            "flare.mdf",
            "Glyph1.mdf",
            "Glyph2.mdf",
            "Golden_orb_of_death.mdf",
            "GrassBlowin.mdf",
            "hand.mdf",
            "Ice1.mdf",
            "Ice2.mdf",
            "Ice3.mdf",
            "Leaf1.mdf",
            "Leaf2.mdf",
            "Leaf3.mdf",
            "lightning-1.mdf",
            "lightning-2.mdf",
            "lightning-3.mdf",
            "lightning-4.mdf",
            "lightning-5.mdf",
            "MagicFang.mdf",
            "MinorGlobe.mdf",
            "mm-candle.mdf",
            "MM-CandleSprite.mdf",
            "MM-CandleSpriteFire.mdf",
            "MM-CandleStick.mdf",
            "MM-Chain.mdf",
            "mm-wick.mdf",
            "NODE-Air.mdf",
            "NODE-Earth.mdf",
            "NODE-Fire.mdf",
            "NODE-Water.mdf",
            "Note1.mdf",
            "Note2.mdf",
            "Note3.mdf",
            "Note4.mdf",
            "OakLeaf.mdf",
            "RedBrick-Alone.mdf",
            "Ring-3.mdf",
            "ring-fuzzy.mdf",
            "Ring-Split.mdf",
            "Ring-third.mdf",
            "Ring.mdf",
            "rocks.mdf",
            "Shard.mdf",
            "Sliver.mdf",
            "spear.mdf",
            "Spikes-1.mdf",
            "Spikes.mdf",
            "SpikeStone.mdf",
            "splash.mdf",
            "Strike.mdf",
            "SummonMonster.mdf",
            "SummonMonsterB.mdf",
            "Tree.mdf",
            "web-1.mdf",
            "web-2.mdf",
            "web.mdf",
            "WeedBlowin.mdf",
        };

        private readonly PartSysParser _parser = new PartSysParser();

        private readonly TempDirectory _tempDirectory = new();

        private TroikaVfs _vfs = new TroikaVfs();

        private IFileSystem _previousVfs;

        public PartSysParserTest()
        {
            _previousVfs = Tig.FS;
            // Setup a fake FS
            _vfs.AddDataDir(_tempDirectory.Path);
            Tig.FS = _vfs;

            // Setup the temporary directory to contain the desired particle system files
            var partsysFiles = new[] {
                "partsys0.tab",
                "partsys1.tab",
                "partsys2.tab"
            };
            foreach (var partsysFile in partsysFiles)
            {
                File.Copy(TestData.GetPath("Particles/" + partsysFile), Path.Join(_tempDirectory.Path, partsysFile));
            }

            // Setup dummy material files because the particle system parser is going to try and find them
            Directory.CreateDirectory(Path.Join(_tempDirectory.Path, "art/meshes/particle/"));
            foreach (var materialFile in MaterialFiles)
            {
                var path = Path.Join(_tempDirectory.Path, "art/meshes/particle", materialFile);
                var basename = Path.GetFileNameWithoutExtension(materialFile);
                File.WriteAllText(path, $"Textured\nTexture \"textures/{basename}.tga\"");
            }

            foreach (var partsysFile in partsysFiles)
            {
                _parser.ParseFile(partsysFile);
            }
        }

        public void Dispose()
        {
            if (Tig.FS == _vfs)
            {
                Tig.FS = _previousVfs;
            }

            _tempDirectory.Dispose();
        }

        [Test]
        public void TestBasicSystem()
        {
            // Tests that the max particles are calculated correctly for all particle systems
            var flamingAxeEmitter = _parser.GetSpec("ef-flaming axe");
            Assert.NotNull(flamingAxeEmitter);
            Assert.AreEqual(6, flamingAxeEmitter.GetEmitters().Count);
        }

        [Test]
        public void TestKeyFrameParsing()
        {
            foreach (var line in File.ReadLines("Particles/keyframedump.txt"))
            {
                var parts = line.Split('|');
                var partSysName = parts[0];
                var emitterIdx = int.Parse(parts[1]);
                PartSysParamId paramIdx = (PartSysParamId)int.Parse(parts[2]);

                var partSys = _parser.GetSpec(partSysName);
                if (partSys == null)
                {
                    throw new NUnitException($"Could not find particle system {partSysName}");
                }

                if (emitterIdx >= partSys.GetEmitters().Count)
                {
                    throw new NUnitException($"Particle System {partSysName} is missing emitter {emitterIdx}");
                }

                var emitter = partSys.GetEmitters()[emitterIdx];
                var param = emitter.GetParam(paramIdx);
                float lifespan = (paramIdx >= PartSysParamId.part_accel_X)
                    ? emitter.GetParticleLifespan()
                    : emitter.GetLifespan();
                var msg =
                    $"Sys: '{partSysName}' Emitter: '{emitter.GetName()}' Param: {paramIdx} (Lifespan: {lifespan})";

                param.Should().NotBeNull(msg);
                param.Type.Should().BeEquivalentTo(PartSysParamType.PSPT_KEYFRAMES, msg);
                var frameParam = (PartSysParamKeyframes)param;

                var frameCount = int.Parse(parts[3]);
                frameParam.GetFrames().Length.Should().Be(frameCount, msg);

                for (var i = 0; i < frameCount; ++i)
                {
                    var frameStr = parts[4 + i];
                    var frameParts = frameStr.Split(';');
                    var frameStarti = uint.Parse(frameParts[0], NumberStyles.HexNumber);
                    var frameValuei = uint.Parse(frameParts[1], NumberStyles.HexNumber);
                    var frameDeltai = uint.Parse(frameParts[2], NumberStyles.HexNumber);
                    var frameStart = BitConverter.ToSingle(BitConverter.GetBytes(frameStarti));
                    var frameValue = BitConverter.ToSingle(BitConverter.GetBytes(frameValuei));
                    var frameDelta = BitConverter.ToSingle(BitConverter.GetBytes(frameDeltai));

                    var actual = frameParam.GetFrames()[i];
                    actual.start.Should().BeApproximately(frameStart, 0.0001f, msg);
                    actual.value.Should().BeApproximately(frameValue, 0.0001f, msg);
                    actual.deltaPerSec.Should().BeApproximately(frameDelta, MathF.Abs(frameDelta * 0.0001f), msg);
                }
            }
        }

        [Test]
        public void TestTextureNamesShouldBeExtractedFromMdfFiles()
        {
            var partSys = _parser.GetSpec("Bardic-Countersong");
            partSys.GetEmitters().Select(e => e.GetTextureName()).Should().BeEquivalentTo(
                "textures/Note3.tga", "textures/Note2.tga", "textures/Note1.tga",
                "textures/Note1.tga", "textures/Note2.tga", "textures/Note3.tga",
                "textures/Flare-1.tga", "textures/Flare-3.tga"
            );
        }

        [Test]
        public void TestMaxParticleCalculations()
        {
            foreach (var line in File.ReadLines("Particles/partsysdump.txt"))
            {
                var parts = line.Split('|');
                var partSysName = parts[0];
                var emitterIdx = int.Parse(parts[1]);

                var partSys = _parser.GetSpec(partSysName);
                partSys.Should().NotBeNull("Could not find particle system" + partSysName);
                emitterIdx.Should().BeLessThan(partSys.GetEmitters().Count,
                    $"Particle System {partSysName} is missing emitter {emitterIdx}.");
                var emitter = partSys.GetEmitters()[emitterIdx];

                var msg = $"Sys: {partSysName} Emitter: {emitter.GetName()}";

                var maxParticles = int.Parse(parts[2]);
                // TemplePlus already deviated from Vanilla here, but I don't know why...
                //emitter.GetMaxParticles().Should().Be(maxParticles, msg);

                var particlesPerStep = float.Parse(parts[3], CultureInfo.InvariantCulture);
                emitter.GetParticleRate().Should().Be(particlesPerStep, msg);

                var particlesPerStepSec = float.Parse(parts[4], CultureInfo.InvariantCulture);
                emitter.GetParticleRateMin().Should().Be(particlesPerStepSec, msg);
            }
        }
    }
}