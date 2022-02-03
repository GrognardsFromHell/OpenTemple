using System;
using System.Globalization;
using System.Text;
using OpenTemple.Core.Particles.Parser;
using NUnit.Framework;

namespace OpenTemple.Tests.Particles;

public class ParserKeyframesTest
{
    private static string Parse(string spec, float lifetime)
    {
        var parsed = ParserKeyframes.Parse(Encoding.Default.GetBytes(spec), lifetime);
        Assert.NotNull(parsed);

        var result = "";
        foreach (var frame in parsed.GetFrames())
        {
            if (result != "")
            {
                result += " -> ";
            }

            result += $"{frame.value}@{frame.start.ToString(CultureInfo.InvariantCulture)}";
        }

        return result;
    }

    /*
        This test checks our implementation of the primary parsing bug that
        the ToEE particle systems parser has. If the first frame is offset
        from the actual start of the animation, the system needs to insert
        a frame with the same value as the actual first frame at time code 0.
        This somewhat works, but while it modifies the time code of the first
        specified frame, it also skips it too early. This leads to the entire
        animation being shifted towards the front of the animation and
        an incorrect frame being inserted at the end.

        This particular example comes from the Brasier Main Fire emitter.
        The actual expected result i'd guess would be:
        255@0 -> 255@2/30 -> 255@3/30 -> 197@0.5
    */
    [Test]
    public void TestOldBugOnlyPreFrame()
    {
        var actual = Parse("255(2),255(3),197", 0.5f);
        var expected = "255@0 -> 255@0.1 -> 197@0.33333334 -> 197@0.5";
        Assert.AreEqual(expected, actual);
    }

    /*
        This tests our implementation of the ToEE frame parsing bug
        when the last frame is offset from the end of the animation.
        This particular (pointless since the value is constant, by the way)
        example comes from sp-Burning Hands
    */
    [Test]
    public void TestOldBugWithPostFrame()
    {
        var actual = Parse("255(6),255(10)", 1);
        var expected = "255@0 -> 255@0.33333334 -> 255@0.6666667 -> 255@1";
        Assert.AreEqual(expected, actual);
    }

    /// <summary>
    /// "sp-See Invisibility" has an emitter with a vertical tab inside the keyframe list.
    /// </summary>
    [Test]
    public void TestParsingWithVerticalTabs()
    {
        var actual = Parse("255,\v5", 1);
        var expected = "255@0 -> 5@1";
        Assert.AreEqual(expected, actual);
    }

    /*
        This tests that keyframe specs that are actually unsupported by the old
        engine but still parsed correctly are supported in the same way by TemplePlus.
        In particular this is the mixing of random and keyframe parameters. In reality,
        the sscanf operation used by ToEE will just read the 200 of "200?300" when the
        frame is parsed by the keyframe parser.
        This particular example comes from the Brasier Smoke emitter.
    */
    [Test]
    public void TestOldUnsupportedBehaviour()
    {
        var actual = Parse("200?300,-100", 1.5f);
        var expected = "200@0 -> -100@1.5";
        Assert.AreEqual(expected, actual);
    }

    [Test]
    public void TestUnterminatedList()
    {
        var actual = Parse("40,35,43,37,39,42,40,35,43,37,39,42,40,35,43,37,39,42,", 1.0f);
        var expected =
            "40@0 -> 35@0.05882353 -> 43@0.11764706 -> 37@0.1764706 -> 39@0.23529412 -> 42@0.29411766 -> 40@0.3529412 -> 35@0.4117647 -> 43@0.47058824 -> 37@0.5294118 -> 39@0.5882353 -> 42@0.64705884 -> 40@0.7058824 -> 35@0.7647059 -> 43@0.8235294 -> 37@0.88235295 -> 39@0.9411765 -> 42@1";
        Assert.AreEqual(expected, actual);
    }
}