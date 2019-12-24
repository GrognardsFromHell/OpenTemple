using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using OpenTemple.Core.IO;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems
{
    public class InvenSourceSystem : IGameSystem
    {
        private readonly Dictionary<int, InventorySource> _sources = new Dictionary<int, InventorySource>();

        [TempleDllLocation(0x10053220)]
        public InvenSourceSystem()
        {
            var invenSourceLines = Tig.FS.ReadMesFile("rules/InvenSource.mes");
            var invenSourceBuyLines = Tig.FS.ReadMesFile("rules/InvenSourceBuy.mes");

            foreach (var kvp in invenSourceLines)
            {
                try
                {
                    _sources[kvp.Key] = ParseLine(kvp.Key, kvp.Value);
                }
                catch (Exception e)
                {
                    throw new InvalidOperationException(
                        $"Failed to parse inventory source line {kvp.Key}: {e.Message}"
                    );
                }
            }
        }

        [TempleDllLocation(0x10052620)]
        public bool TryGetById(int id, out InventorySource source)
        {
            return _sources.TryGetValue(id, out source);
        }

        [TempleDllLocation(0x10052a90)]
        private InventorySource ParseLine(int id, string line)
        {
            var result = new InventorySource(id);

            var colonIdx = line.IndexOf(':');
            if (colonIdx == -1)
            {
                throw new ArgumentException($"Missing colon.");
            }

            result.Name = line.Substring(0, colonIdx);
            int currentIndex = colonIdx + 1;

            while (currentIndex < line.Length)
            {
                var nextSep = line.IndexOf(' ', currentIndex);
                ReadOnlySpan<char> entryText;
                if (nextSep == -1)
                {
                    entryText = line.AsSpan(currentIndex);
                }
                else
                {
                    entryText = line.AsSpan(currentIndex, nextSep - currentIndex);
                }

                // Skip leading whitespace
                while (!entryText.IsEmpty && entryText[0] == ' ')
                {
                    entryText = entryText.Slice(1);
                }

                ParseEntry(entryText, result);

                if (nextSep == -1)
                {
                    break;
                }

                currentIndex = nextSep + 1;
            }


            return result;
        }

        private void ParseEntry(ReadOnlySpan<char> entry, InventorySource result)
        {
            if (entry.IsEmpty)
            {
                return;
            }

            var nextSep = entry.IndexOf(',');
            var firstToken = ReadOnlySpan<char>.Empty;
            var secondToken = ReadOnlySpan<char>.Empty;
            if (nextSep != -1)
            {
                firstToken = entry.Slice(0, nextSep);
                secondToken = entry.Slice(nextSep + 1);
            }

            if (firstToken.Equals("copper", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.CopperMin = minValue;
                result.CopperMax = maxValue;
            }
            else if (firstToken.Equals("silver", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.SilverMin = minValue;
                result.SilverMax = maxValue;
            }
            else if (firstToken.Equals("gold", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.GoldMin = minValue;
                result.GoldMax = maxValue;
            }
            else if (firstToken.Equals("platinum", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.PlatinumMin = minValue;
                result.PlatinumMax = maxValue;
            }
            else if (firstToken.Equals("gems", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.GemsMinValue = minValue;
                result.GemsMaxValue = maxValue;
            }
            else if (firstToken.Equals("jewelry", StringComparison.OrdinalIgnoreCase))
            {
                ParseRange(secondToken, out var minValue, out var maxValue);
                result.JewelryMinValue = minValue;
                result.JewelryMaxValue = maxValue;
            }
            else if (firstToken.Equals("buy_list_num", StringComparison.OrdinalIgnoreCase))
            {
                if (!int.TryParse(secondToken, out var buylistId))
                {
                    throw new ArgumentException($"Invalid buy list id: '{new string(secondToken)}'");
                }

                result.BuyListId = buylistId;
            }
            else if (entry.StartsWith("("))
            {
                // Parse a one-off list
                if (!entry.EndsWith(")"))
                {
                    throw new ArgumentException($"Unterminated one-off list: '{new string(entry)}'");
                }

                var list = new string(entry.Slice(1, entry.Length - 2));
                result.OneOfLists.Add(list.Split(',').Select(int.Parse).ToList());
            }
            else
            {
                // This is a proto entry with a percentage chance
                if (!int.TryParse(firstToken, out var percentage))
                {
                    throw new ArgumentException($"Couldn't parse percentage in entry: '{new string(entry)}'");
                }

                if (!int.TryParse(secondToken, out var protoId))
                {
                    throw new ArgumentException($"Couldn't parse protoId in entry: '{new string(entry)}'");
                }

                result.Items.Add(new InventorySourceItem(percentage, protoId));
            }
        }

        private void ParseRange(ReadOnlySpan<char> text, out int minValue, out int maxValue)
        {
            var sep = text.IndexOf('-');
            if (sep == -1)
            {
                if (!int.TryParse(text, out minValue))
                {
                    throw new ArgumentException($"Invalid amount: '{new string(text)}'");
                }

                maxValue = minValue;
            }
            else
            {
                var fromText = text.Slice(0, sep);
                var toText = text.Slice(sep + 1);

                if (!int.TryParse(fromText, out minValue)
                    || !int.TryParse(toText, out maxValue))
                {
                    throw new ArgumentException($"Invalid amount range: '{new string(text)}'");
                }
            }
        }

        public void Dispose()
        {
        }
    }

    public readonly struct InventorySourceItem
    {
        public readonly int PercentChance;
        public readonly int ProtoId;

        public InventorySourceItem(int percentChance, int protoId)
        {
            PercentChance = percentChance;
            ProtoId = protoId;
        }
    }

    public class InventorySource
    {
        public int Id { get; }

        public string Name { get; set; }

        public int CopperMin { get; set; }
        public int CopperMax { get; set; }
        public int SilverMin { get; set; }
        public int SilverMax { get; set; }
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int PlatinumMin { get; set; }
        public int PlatinumMax { get; set; }
        public int GemsMinValue { get; set; }
        public int GemsMaxValue { get; set; }
        public int JewelryMinValue { get; set; }
        public int JewelryMaxValue { get; set; }
        public int BuyListId { get; set; }

        public List<InventorySourceItem> Items { get; } = new List<InventorySourceItem>();

        public List<List<int>> OneOfLists { get; } = new List<List<int>>();

        public InventorySource(int id)
        {
            Id = id;
        }
    }
}