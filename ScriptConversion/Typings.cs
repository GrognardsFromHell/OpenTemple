using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptConversion;

internal class Typings
{
    private List<TypingRecord> _records = new List<TypingRecord>();

    public void Load(string path)
    {
        Console.WriteLine("Loading typing info from " + path);
        var lineNum = 0;
        foreach (var line in File.ReadLines(path))
        {
            lineNum++;
            if (line.IndexOf(';') == -1)
            {
                continue;
            }

            try
            {
                var cols = line.Split(';');
                _records.Add(new TypingRecord
                {
                    Module = cols[0],
                    Function = cols[1],
                    ReturnType = Enum.Parse<GuessedType>(cols[2]),
                    ParameterTypes = cols.Skip(3).Select(Enum.Parse<GuessedType>).ToArray()
                });
            }
            catch (Exception)
            {
                Console.WriteLine("Error on line " + lineNum + " of " + path);
                throw;
            }
        }
    }

    public bool TryGetSignature(string moduleName, string functionName, out GuessedType forcedReturnType,
        out GuessedType[] parameterTypes)
    {
        foreach (var typingRecord in _records)
        {
            if (typingRecord.Module != "*" && typingRecord.Module != moduleName)
            {
                continue;
            }

            if (typingRecord.Function != functionName)
            {
                continue;
            }

            forcedReturnType = typingRecord.ReturnType;
            parameterTypes = typingRecord.ParameterTypes;
            return true;
        }

        forcedReturnType = GuessedType.Unknown;
        parameterTypes = null;
        return false;
    }
}

internal struct TypingRecord
{
    public string Module { get; set; }
    public string Function { get; set; }
    public GuessedType ReturnType { get; set; }
    public GuessedType[] ParameterTypes { get; set; }
}