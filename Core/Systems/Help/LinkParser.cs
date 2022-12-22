using System;
using JetBrains.Annotations;

namespace OpenTemple.Core.Systems.Help;

public static class LinkParser
{
    private const byte LinkTextDelimiter = (byte) '~';

    private const byte LinkTargetStart = (byte) '[';
    private const byte LinkTargetEnd = (byte) ']';

    public static bool ParseLink(ReadOnlySpan<byte> text,
        out ReadOnlySpan<byte> linkText,
        out ReadOnlySpan<byte> linkTarget,
        out int linkLength)
    {
        ReadOnlySpan<byte> parseText = text;

        // Text has the form ~<linktext>~[link_target]
        if (!ParseDelimitedToken(ref parseText, LinkTextDelimiter, LinkTextDelimiter, out linkText))
        {
            linkTarget = default;
            linkLength = 0;
            return false;
        }

        if (!ParseDelimitedToken(ref parseText, LinkTargetStart, LinkTargetEnd, out linkTarget))
        {
            linkLength = 0;
            return false;
        }

        linkLength = text.Length - parseText.Length;
        return true;
    }

    private static bool ParseDelimitedToken(scoped ref ReadOnlySpan<byte> text,
        byte tokenStart,
        byte tokenEnd,
        out ReadOnlySpan<byte> tokenText)
    {
        if (text.IsEmpty || text[0] != tokenStart)
        {
            tokenText = default;
            return false;
        }

        var end = text[1..].IndexOf(tokenEnd);
        if (end == -1)
        {
            tokenText = default;
            return false;
        }

        end++;

        tokenText = text.Slice(1, end - 1);
        text = text[(end + 1)..];
        return true;
    }
}