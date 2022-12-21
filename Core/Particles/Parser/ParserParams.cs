using System;
using System.Buffers.Text;
using System.Text;
using OpenTemple.Particles.Params;

namespace OpenTemple.Core.Particles.Parser;

public static class ParserParams
{
    public static IPartSysParam? Parse(PartSysParamId id, ReadOnlySpan<byte> value, float emitterLifespan,
        float particleLifespan, out bool success)
    {
        // Look up the default value
        var defaultValue = PartSysParamDefaultValues.GetDefaultValue(id);

        // Do we have to use the particle or emitter lifespan as reference for keyframes?
        var lifespan = (id >= PartSysParamId.part_accel_X) ? particleLifespan : emitterLifespan;

        return Parse(value, defaultValue, lifespan, out success);
    }

    public static IPartSysParam? Parse(ReadOnlySpan<byte> value, float defaultValue, float parentLifespan,
        out bool success)
    {
        success = false;

        SkipWhitespace(ref value);

        if (parentLifespan == 0)
        {
            parentLifespan = 1.0f;
        }

        if (value.Contains((byte) ','))
        {
            var result = ParseKeyframes(value, parentLifespan);
            success = result != null;
            return result;
        }

        if (value.Contains((byte) '?'))
        {
            var result = ParseRandom(value);
            success = result != null;
            return result;
        }

        if (value.Contains((byte) '#'))
        {
            var result = ParseSpecial(value);
            success = result != null;
            return result;
        }

        return ParseConstant(value, defaultValue, out success);
    }

    public static PartSysParamKeyframes? ParseKeyframes(ReadOnlySpan<byte> value, float parentLifespan)
    {
        return ParserKeyframes.Parse(value, parentLifespan);
    }

    public static PartSysParamRandom? ParseRandom(ReadOnlySpan<byte> value)
    {
        if (!Utf8Parser.TryParse(value, out float lower, out var bytesConsumed))
        {
            return null;
        }

        if (bytesConsumed >= value.Length || value[bytesConsumed] != '?')
        {
            return null;
        }

        if (!Utf8Parser.TryParse(value.Slice(bytesConsumed + 1), out float upper, out _))
        {
            return null;
        }

        var variance = upper - lower;
        return new PartSysParamRandom(lower, variance);
    }

    private static void SkipWhitespace(ref ReadOnlySpan<byte> value)
    {
        for (var count = 0; count < value.Length; count++)
        {
            if (value[count] != ' ' && value[count] != 11 /* Vertical tab */)
            {
                if (count > 0)
                {
                    value = value.Slice(count);
                }

                break;
            }
        }
    }

    private static readonly byte[] SpecialValueRadius = Encoding.ASCII.GetBytes("#radius");

    public static PartSysParamSpecial? ParseSpecial(ReadOnlySpan<byte> value)
    {
        Span<byte> valueLower = stackalloc byte[value.Length];
        ToStringLower(value, valueLower);

        if (value.SequenceEqual(SpecialValueRadius))
        {
            return new PartSysParamSpecial(PartSysParamSpecialType.PSPST_RADIUS);
        }

        return null;
    }

    private static void ToStringLower(ReadOnlySpan<byte> value, Span<byte> valueLower)
    {
        for (var i = 0; i < value.Length; i++)
        {
            valueLower[i] = (byte) char.ToLowerInvariant((char) value[i]);
        }
    }

    public static PartSysParamConstant? ParseConstant(ReadOnlySpan<byte> value, float defaultValue, out bool success)
    {
        // Try to parse it as a floating point constant
        if (!Utf8Parser.TryParse(value, out float floatValue, out _))
        {
            success = false;
            return null;
        }

        success = true; // At this point it's a valid floating point number

        // Save some memory by not allocating a param if we're using the default value anyway
        if (MathF.Abs(floatValue - defaultValue) < 0.000001f)
        {
            return null;
        }

        return new PartSysParamConstant(floatValue);
    }
}