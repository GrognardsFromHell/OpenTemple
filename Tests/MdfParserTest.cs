using OpenTemple.Core.MaterialDefinitions;
using NUnit.Framework;

namespace OpenTemple.Tests;

public class MdfParserTest
{
    [Test]
    public void CanParseSimpleTexturedMaterial()
    {
        var parser = new MdfParser("dummy", @"Textured
Texture ""folder/filename.tga""
");
        var material = parser.Parse();
        Assert.AreEqual(MdfType.Textured, material.Type);
        Assert.AreEqual(@"folder/filename.tga", material.Samplers[0].Filename);
    }

    /// <summary>
    /// In Textured material files, the paths are NOT escaped.
    /// </summary>
    [Test]
    public void CanParseBackslashesInSamplerPathOfTexturedMaterial()
    {
        var parser = new MdfParser("dummy", @"Textured
Texture ""folder\filename.tga""
");
        var material = parser.Parse();
        Assert.AreEqual(MdfType.Textured, material.Type);
        Assert.AreEqual(@"folder\filename.tga", material.Samplers[0].Filename);
    }

    /// <summary>
    /// In "General" materials, the paths are actually escaped...
    /// </summary>
    [Test]
    public void CanParseEscapedBackslashesInSamplerPathOfGeneralMaterial()
    {
        var parser = new MdfParser("dummy", @"General
Texture 0 ""folder\\filename.tga""
");
        var material = parser.Parse();
        Assert.AreEqual(MdfType.General, material.Type);
        Assert.AreEqual(@"folder\filename.tga", material.Samplers[0].Filename);
    }
}