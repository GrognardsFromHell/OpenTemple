using OpenTemple.Core.MaterialDefinitions;
using Xunit;

namespace OpenTemple.Tests
{
    public class MdfParserTest
    {
        [Fact]
        public void CanParseSimpleTexturedMaterial()
        {
            var parser = new MdfParser("dummy", @"Textured
Texture ""folder/filename.tga""
");
            var material = parser.Parse();
            Assert.Equal(MdfType.Textured, material.type);
            Assert.Equal(@"folder/filename.tga", material.samplers[0].filename);
        }

        /// <summary>
        /// In Textured material files, the paths are NOT escaped.
        /// </summary>
        [Fact]
        public void CanParseBackslashesInSamplerPathOfTexturedMaterial()
        {
            var parser = new MdfParser("dummy", @"Textured
Texture ""folder\filename.tga""
");
            var material = parser.Parse();
            Assert.Equal(MdfType.Textured, material.type);
            Assert.Equal(@"folder\filename.tga", material.samplers[0].filename);
        }

        /// <summary>
        /// In "General" materials, the paths are actually escaped...
        /// </summary>
        [Fact]
        public void CanParseEscapedBackslashesInSamplerPathOfGeneralMaterial()
        {
            var parser = new MdfParser("dummy", @"General
Texture 0 ""folder\\filename.tga""
");
            var material = parser.Parse();
            Assert.Equal(MdfType.General, material.type);
            Assert.Equal(@"folder\filename.tga", material.samplers[0].filename);
        }
    }
}