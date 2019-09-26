using System;
using System.Dynamic;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.DynamicScripting
{
    /// <summary>
    /// Any of this is exposed in the global namespace for scripts.
    /// </summary>
    public class ReplGlobals
    {
        public dynamic vars = new ExpandoObject();

        public GameObjectBody PartyLeader => GameSystems.Party.GetLeader();

        public GameObjectBody FindByName(string namePart)
        {
            return GameSystems.Object
                .EnumerateNonProtos()
                .FirstOrDefault(o => o.ToString().Contains(namePart, StringComparison.InvariantCultureIgnoreCase));
        }

        public void Print(object obj)
        {
            Tig.Console.Append(obj?.ToString() ?? "null");
        }
    }
}