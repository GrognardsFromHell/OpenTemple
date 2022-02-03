using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpenTemple.Core.GameObjects;

namespace OpenTemple.Core.IO.SaveGames.Co8State
{
    public static class PythonValueParser
    {
        public static object ParseValue(ReadOnlySpan<char> text)
        {
            var i = 0;
            return ParseValue(text, ref i);
        }

        public static ObjectId ParseObjectId(object value)
        {
            // ToEE encodes object handles as this crazy nested tupel bullshit
            if (value is (int idType, (int dword1, int short1, int short2, (int b1, int b2, int b3, int b4, int b5, int
                b6, int b7, int b8))))
            {
                if (idType != 2)
                {
                    throw new ArgumentException($"Only persistent ids (type=2) are supported. but found:" + idType);
                }

                return ObjectId.CreatePermanent(
                    new Guid(
                        dword1,
                        (short) short1,
                        (short) short2,
                        (byte) b1,
                        (byte) b2,
                        (byte) b3,
                        (byte) b4,
                        (byte) b5,
                        (byte) b6,
                        (byte) b7,
                        (byte) b8
                    )
                );
            }

            throw new ArgumentException($"Is not a serialized object id: {value}");
        }

        public static (int, ObjectId) ParseActiveTargetListEntry(object value)
        {
            if (value is object[] spellIdTargetListPair)
            {
                var spellId = (int) spellIdTargetListPair[0];
                var target = ParseObjectId(spellIdTargetListPair[1]);
                return (spellId, target);
            }

            throw new ArgumentException($"Value is not an active target list: {value}");
        }

        private static object ParseValue(ReadOnlySpan<char> text, ref int i)
        {
            char ch = text[i];
            if (ch == '(')
            {
                // This is the start of a value tuple. We support value tuples of up to 8 values.
                return ParseTuple(text, ref i);
            }
            else if (ch == '[')
            {
                return ParseList(text, ref i);
            }
            else if (ch >= '0' && ch <= '9' || ch == '-')
            {
                return ParseNumber(text, ref i);
            }
            else
            {
                throw new ArgumentException($"Unexpected character {ch}");
            }

            return null;
        }

        private static object ParseNumber(ReadOnlySpan<char> text, ref int start)
        {
            // seek until the end or a non-digit
            var i = start;
            if (text[i] == '-')
            {
                i++;
            }

            while (i < text.Length)
            {
                if (!char.IsDigit(text[i]))
                {
                    break;
                }

                i++;
            }

            var slice = text.Slice(start, i - start);
            if (int.TryParse(slice, NumberStyles.Integer, CultureInfo.InvariantCulture, out var number))
            {
                start = i;
                return number;
            }

            throw new InvalidOperationException("Failed to parse number: " + new string(text));
        }

        private static object ParseTuple(ReadOnlySpan<char> text, ref int start)
        {
            var items = ParseCommaSeparatedValues(text, ref start, '(', ')');

            var factoryMethods = typeof(ValueTuple).GetMethods(BindingFlags.Static | BindingFlags.Public);

            var factoryMethodTemplate = factoryMethods.First(m => m.GetParameters().Length == items.Count);
            var factoryMethod = factoryMethodTemplate
                .MakeGenericMethod(items.Select(i => i.GetType()).ToArray());

            return factoryMethod.Invoke(null, items.ToArray());
        }

        private static object ParseList(ReadOnlySpan<char> text, ref int start)
        {
            return ParseCommaSeparatedValues(text, ref start, '[', ']').ToArray();
        }

        private static List<object> ParseCommaSeparatedValues(ReadOnlySpan<char> text, ref int start,
            char startChar, char endChar)
        {
            if (text[start] != startChar)
            {
                throw new ArgumentException();
            }

            start++;

            var result = new List<object>();

            while (text[start] != endChar)
            {
                result.Add(ParseValue(text, ref start));
                SkipWhitespace(text, ref start);
                if (text[start] == ',')
                {
                    start++;
                    SkipWhitespace(text, ref start);
                }
            }

            start++;

            return result;
        }

        private static void SkipWhitespace(ReadOnlySpan<char> text, ref int i)
        {
            while (text[i] == ' ')
            {
                i++;
            }
        }
    }
}