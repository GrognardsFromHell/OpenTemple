using System.Drawing;
using System.Numerics;
using FluentAssertions;
using NUnit.Framework;
using OpenTemple.Core.GFX;

namespace OpenTemple.Tests.Core.GFX;

public class WorldCameraTest
{
    [Test]
    public void CenterOn()
    {
        var camera = new WorldCamera();
        camera.ViewportSize = new SizeF(1024, 768);

        // Some location in the first room of the tutorial area
        for (var xoff = -15f; xoff <= 15f; xoff += 0.1f)
        {
            for (var zoff = -15f; zoff <= 15f; zoff += 0.1f)
            {
                var center = new Vector3(14396.694f + xoff, 0, 12925.912f + zoff);
                camera.CenterOn(center);
                var distance = (center - camera.CenteredOn).Length();
                distance.Should().BeLessThan(0.1f);
            }
        }
    }
}