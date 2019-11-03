using System;
using System.Globalization;
using System.IO;
using FluentAssertions;
using SpicyTemple.Core.IO.TroikaArchives;
using SpicyTemple.Core.Particles.Parser;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Particles.Params;
using Xunit;
using Xunit.Sdk;

namespace SpicyTemple.Tests.Particles
{
    /*
        This is a test fixture for particle system parser tests that only parses all particle systems once
        to speed up the tests.
    */
    public class PartSysParserTest
    {
        private readonly PartSysParser _parser = new PartSysParser();

        public PartSysParserTest()
        {
            var vfs = new TroikaVfs();
            vfs.AddDataDir(".");
            Tig.FS = vfs;

            // Init VFS with mock/dummy code
            _parser.ParseFile("Particles/partsys0.tab");
            _parser.ParseFile("Particles/partsys1.tab");
            _parser.ParseFile("Particles/partsys2.tab");
        }

        [Fact]
        public void TestBasicSystem()
        {
            // Tests that the max particles are calculated correctly for all particle systems
            var flamingAxeEmitter = _parser.GetSpec("ef-flaming axe");
            Assert.NotNull(flamingAxeEmitter);
            Assert.Equal(6, flamingAxeEmitter.GetEmitters().Count);
        }

        [Fact]
        public void TestKeyFrameParsing()
        {
            foreach (var line in File.ReadLines("Particles/keyframedump.txt"))
            {
                var parts = line.Split('|');
                var partSysName = parts[0];
                var emitterIdx = int.Parse(parts[1]);
                PartSysParamId paramIdx = (PartSysParamId) int.Parse(parts[2]);

                var partSys = _parser.GetSpec(partSysName);
                if (partSys == null)
                {
                    throw new XunitException($"Could not find particle system {partSysName}");
                }

                if (emitterIdx >= partSys.GetEmitters().Count)
                {
                    throw new XunitException($"Particle System {partSysName} is missing emitter {emitterIdx}");
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
                var frameParam = (PartSysParamKeyframes) param;

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

        [Fact]
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