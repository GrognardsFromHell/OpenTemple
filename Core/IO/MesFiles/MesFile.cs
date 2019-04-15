using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SpicyTemple.Core.IO.MesFiles
{
    public static class MesFile
    {

        public static Dictionary<int, string> Read(string path)
        {
            var data = File.ReadAllBytes(path);
            return Read(Path.GetFileName(path), data);
        }

        public static Dictionary<int, string> Read(string filename, ReadOnlySpan<byte> content)
        {

            var result = new Dictionary<int, string>();

            var lexer = new MesLexer(filename, content);

            while (lexer.ReadNextToken(out var keyToken))
            {
                if (!int.TryParse(keyToken, out var key))
                {
                    Debug.Print("Invalid numeric key @ {0}:{1}", filename, lexer.Line);
                }

                if (!lexer.ReadNextToken(out var valueToken))
                {
                    Debug.Print("Key without value @ {0}:{1}", filename, lexer.Line);
                    break;
                }

                result[key] = valueToken;
            }

            return result;

        }

    }

}
