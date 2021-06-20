using System;
using System.Text;

namespace Core.StyleSystemGenerator
{
    public static class ScriptFunctions
    {
        public static string GetFlagEnumBaseType(int count)
        {
            return count switch
            {
                > 64 => throw new InvalidOperationException("Enum cannot have more than 64 literals"),
                > 32 => "ulong",
                > 16 => "uint",
                > 8 => "ushort",
                _ => "byte"
            };
        }

        public static string Indent(string text, int amount)
        {
            var whitespace = "";
            for (var i = 0; i < amount; i++)
            {
                whitespace += " ";
            }

            var result = new StringBuilder(text);
            result.Insert(0, whitespace);

            for (var i = result.Length - 2; i >= 0; i--)
            {
                if (result[i] == '\n')
                {
                    result.Insert(i + 1, whitespace);
                }
            }

            return result.ToString();
        }

    }
}