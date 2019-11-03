using System.Text;
using SpicyTemple.Core.Particles.Parser;
using SpicyTemple.Particles.Params;
using Xunit;

namespace SpicyTemple.Tests.Particles
{
    public class ParserParamsTest
    {
        /// <summary>
        /// Check the default value calculation of PartSysParam for a few examples.
        /// </summary>
        [Fact]
        public void GetDefaultValue()
        {
            Assert.Equal(0, PartSysParamDefaultValues.GetDefaultValue(PartSysParamId.part_accel_X));
            Assert.Equal(255, PartSysParamDefaultValues.GetDefaultValue(PartSysParamId.emit_init_alpha));
            Assert.Equal(1, PartSysParamDefaultValues.GetDefaultValue(PartSysParamId.emit_scale_X));
        }

        [Fact]
        public void ParseConstant()
        {
            var value = Encoding.Default.GetBytes("1.5");
            var param = (PartSysParamConstant)
                ParserParams.Parse(PartSysParamId.emit_accel_X, value, 0, 0, out var success);

            Assert.NotNull(param);
            Assert.True(success);
            Assert.Equal(PartSysParamType.PSPT_CONSTANT, param.Type);
            Assert.Equal(1.5f, param.GetValue());
        }

        /// <summary>
        /// For constants with a default value, the parser will return null, but
        /// set success to true.
        /// </summary>
        [Fact]
        public void ParseConstantDefault()
        {
            var value = Encoding.Default.GetBytes("0");
            var param = ParserParams.Parse(PartSysParamId.emit_accel_X, value, 0, 0, out var success);

            Assert.True(param == null, "No parameter should have been returned for a default value.");
            Assert.True(success, "The parser should still indicate success, however.");

            // Try it for a param with another default value than 1

            value = Encoding.Default.GetBytes("255");
            param = ParserParams.Parse(PartSysParamId.emit_init_alpha, value, 0, 0, out success);

            Assert.True(param == null, "No parameter should have been returned for a default value.");
            Assert.True(success, "The parser should still indicate success, however.");
        }
    }
}