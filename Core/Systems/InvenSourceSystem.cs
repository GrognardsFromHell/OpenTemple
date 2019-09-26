using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
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
                    ParseLine(kvp.Key, kvp.Value);
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
                var nextSep = line.IndexOf(',', currentIndex);
                var spec = line.AsSpan(currentIndex, nextSep - currentIndex);
                Console.WriteLine(new string(spec));

                if (nextSep == -1)
                {
                    break;
                }

                currentIndex = nextSep + 1;
            }


            return result;
        }

        public void Dispose()
        {
        }
    }

    public struct InventorySourceItem
    {
        public int PercentChance;
        public int ProtoId;
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
        public int GemsMin { get; set; }
        public int GemsMax { get; set; }
        public int JewelryMin { get; set; }
        public int JewelryMax { get; set; }
        public int BuyListId { get; set; }

        public List<InventorySourceItem> Items { get; } = new List<InventorySourceItem>();

        public List<List<InventorySourceItem>> OneOfLists { get; } = new List<List<InventorySourceItem>>();

        public InventorySource(int id)
        {
            Id = id;
        }
    }

}