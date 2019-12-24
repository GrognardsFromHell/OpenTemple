using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using OpenTemple.Core.Systems.FogOfWar;
using Xunit;

namespace OpenTemple.Tests.FogOfWar
{
    public class LineOfSightTest
    {
        /// <summary>
        /// This is the fog-buffer of party member 0 exported from Vanilla's memory.
        /// It is captured after calling ComputeLineOfSight (0x100326d0).
        /// </summary>
        private readonly byte[] _vanillaLos0;

        /// <summary>
        /// This is the fog-buffer of party member 0 exported from Vanilla's memory.
        /// It is captured after calling ExtendLineOfSight (0x100317e0).
        /// </summary>
        private readonly byte[] _vanillaLos1a;
        private readonly byte[] _vanillaLos1b;

        public LineOfSightTest()
        {
            _vanillaLos0 = File.ReadAllBytes("FogOfWar/los_buffer_0.bin");
            _vanillaLos1a = File.ReadAllBytes("FogOfWar/los_buffer_1a.bin");
            _vanillaLos1b = File.ReadAllBytes("FogOfWar/los_buffer_1b.bin");
        }

        // Copy just the blocking-bit from source to dest
        private static void CopyBlockingBit(ReadOnlySpan<byte> source, Span<byte> dest)
        {
            Assert.Equal(source.Length, dest.Length);
            for (var i = 0; i < source.Length; i++)
            {
                int current = dest[i];
                current &= ~LineOfSightBuffer.BLOCKING;
                current |= source[i] & LineOfSightBuffer.BLOCKING;
                dest[i] = (byte) current;
            }
        }

        /// <summary>
        /// Save a comparison / visualization of two line of sight buffers to a PNG file. Very useful for
        /// visually comparing the actual line of sight buffer to reference data.
        /// </summary>
        private static void DumpLineOfSight(
            ReadOnlySpan<byte> referenceBuffer,
            ReadOnlySpan<byte> actualBuffer,
            [CallerMemberName]
            string testName = "LineOfSight")
        {
            using var image = new Image<Rgb24>(
                Configuration.Default,
                LineOfSightBuffer.Dimension,
                LineOfSightBuffer.Dimension
            );

            Trace.Assert(referenceBuffer.Length == actualBuffer.Length);
            Trace.Assert(referenceBuffer.Length == image.Width * image.Height);

            var red = new Rgb24(255, 0, 0);
            var yellow = new Rgb24(255, 255, 0);
            var white = new Rgb24(255, 255, 255);

            for (var y = 0; y < LineOfSightBuffer.Dimension; y++)
            {
                for (var x = 0; x < LineOfSightBuffer.Dimension; x++)
                {
                    var index = y * LineOfSightBuffer.Dimension + x;
                    var refFlag = referenceBuffer[index] & LineOfSightBuffer.LINE_OF_SIGHT;
                    var actualFlag = actualBuffer[index] & LineOfSightBuffer.LINE_OF_SIGHT;

                    if (refFlag != actualFlag)
                    {
                        if (refFlag != 0)
                        {
                            image[x, y] = red;
                        }
                        else
                        {
                            image[x, y] = yellow;
                        }
                    }
                    else if (refFlag != 0)
                    {
                        image[x, y] = white;
                    }
                }
            }

            image.Save(testName + ".png");
        }

        /// <summary>
        /// The initially computed line of sight is checked against what Vanilla would do.
        /// The data is based around the initial player position on the tutorial map.
        /// This does not include the "extended" line of sight which is performed after the initial raycasting.
        /// </summary>
        [Fact]
        public void ComputedLineOfSightMatchesVanillaForTutorialMap()
        {
            var buffer = new LineOfSightBuffer();

            // Pre-Fill the data with blocking data from the vanilla buffer
            var losBuffer = buffer.Buffer;
            CopyBlockingBit(_vanillaLos0, losBuffer);

            buffer.ComputeLineOfSight(60);

            // Save comparison of line of sight to a PNG file so it can be visually inspected
            DumpLineOfSight(_vanillaLos0, losBuffer);

            // Check that the line of sight bit (2) is set as it is in vanilla
            for (var i = 0; i < losBuffer.Length; i++)
            {
                Assert.Equal(_vanillaLos0[i] & LineOfSightBuffer.LINE_OF_SIGHT, losBuffer[i] & LineOfSightBuffer.LINE_OF_SIGHT);
            }
        }

        /// <summary>
        /// Checks line of sight against vanilla after it has been extended by the sector visibility flags.
        /// (BASE, END, EXTEND, ARCHWAY)
        /// </summary>
        [Fact]
        public void ExtendedLineOfSightMatchesVanillaForTutorialMap()
        {
            var buffer = new LineOfSightBuffer();

            // Pre-Fill the data with blocking data from the vanilla buffer
            var losBuffer = buffer.Buffer;
            _vanillaLos1a.CopyTo(losBuffer);

            buffer.ExtendLineOfSight();

            // Save comparison of line of sight to a PNG file so it can be visually inspected
            DumpLineOfSight(_vanillaLos1b, losBuffer);

            // Check that the line of sight bit (2) is set as it is in vanilla
            for (var i = 0; i < losBuffer.Length; i++)
            {
                Assert.Equal(_vanillaLos1b[i] & LineOfSightBuffer.LINE_OF_SIGHT, losBuffer[i] & LineOfSightBuffer.LINE_OF_SIGHT);
            }
        }

    }

}