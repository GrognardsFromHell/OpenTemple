using System;

namespace OpenTemple.Core.Systems.Help;

public static class UnicodeLinkParser
{
    private const char LinkTextDelimiter = '~';

    private const char LinkTargetStart = '[';
    private const char LinkTargetEnd = ']';

    public static bool ParseLink(ReadOnlySpan<char> text,
        out ReadOnlySpan<char> linkText,
        out ReadOnlySpan<char> linkTarget,
        out int linkLength)
    {
        ReadOnlySpan<char> parseText = text;

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

    private static bool ParseDelimitedToken(ref ReadOnlySpan<char> text,
        char tokenStart,
        char tokenEnd,
        out ReadOnlySpan<char> tokenText)
    {
        if (text.IsEmpty || text[0] != tokenStart)
        {
            tokenText = default;
            return false;
        }

        var end = text.Slice(1).IndexOf(tokenEnd);
        if (end == -1)
        {
            tokenText = default;
            return false;
        }

        end++;

        tokenText = text.Slice(1, end - 1);
        text = text.Slice(end + 1);
        return true;
    }
}