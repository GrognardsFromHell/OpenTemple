using System;
using System.Dynamic;
using System.Linq;
using OpenTemple.Core;
using OpenTemple.Core.Config;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.DynamicScripting;

/// <summary>
/// Any of this is exposed in the global namespace for scripts.
/// </summary>
public class ReplGlobals
{
    public dynamic Vars = new ExpandoObject();

    public GameConfig Config => Globals.Config;

    public GameObject PartyLeader => GameSystems.Party.GetLeader();

    public GameObject FindByName(string namePart)
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