using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using JetBrains.Annotations;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.TabFiles;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems;

[Flags]
public enum TrapSpecFlag
{
    InStone = 1,
    PlacedByParty = 2,
    SpottedByParty = 4,
    Magical = 8
}

public readonly record struct TrapDamage(
    DamageType Type,
    Dice Dice
);

public record Trap(
    // last column in trap.tab seems to be the numeric trap id, stored in "counters" field of san_trap
    int Id,
    string Name, // first col

    // Which script event triggers it?
    ObjScriptEvent Trigger,
    TrapSpecFlag Flags,
    string ParticleSystemId,
    int SearchDC,
    int DisarmDC,

    // If the obj is not a "real" trap, the trap script will be replaced by this trap after triggering (by name)
    string AfterTriggerName,
    int ChallengeRating, // second to last col
    IImmutableList<TrapDamage> Damage
);

public class TrapSystem : IGameSystem
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    [TempleDllLocation(0x10aa32a0)] private readonly Dictionary<int, string> _translations;

    [TempleDllLocation(0x10aa329c)] private readonly ImmutableDictionary<int, Trap> _traps;

    private readonly ImmutableDictionary<string, Trap> _trapsByName;

    [TempleDllLocation(0x10050da0)]
    public TrapSystem()
    {
        _translations = Tig.FS.ReadMesFile("mes/trap.mes");

        var parser = new TrapFileParser();
        parser.Parse("rules/traps.tab");
        _traps = parser.Traps.ToImmutableDictionary();
        _trapsByName = _traps.Values.ToImmutableDictionary(t => t.Name, t => t);
    }

    [TempleDllLocation(0x10050940)]
    public void Dispose()
    {
    }

    [TempleDllLocation(0x100514c0)]
    public void AttemptDisarm(GameObject disarmer, GameObject trappedObj, out bool success)
    {
        if (!TryGetTrapFromObject(trappedObj, out var trap) || !KnowsAboutDangerousTrap(disarmer, trappedObj))
        {
            success = false;
            return;
        }

        success = GameSystems.Skill.SkillRoll(disarmer, SkillId.disable_device, trap.DisarmDC, out var dcDelta,
            SkillCheckFlags.UnderDuress);
        if (success)
        {
            SetNextTrapAfterTrigger(trappedObj);
            GameSystems.TextFloater.FloatLine(trappedObj, TextFloaterCategory.Generic, TextFloaterColor.Green,
                _translations[4]);
            GameSystems.D20.Combat.AwardExperienceForChallengeRating(trap.ChallengeRating);
            GameUiBridge.RecordTrapDisarmed(disarmer);
        }
        else
        {
            GameSystems.TextFloater.FloatLine(trappedObj, TextFloaterCategory.Generic, TextFloaterColor.Green,
                _translations[5]);
            if (dcDelta <= -4)
            {
                SetOffTrap(disarmer, trappedObj);
            }
        }
    }

    [TempleDllLocation(0x10050f40)]
    private void SetNextTrapAfterTrigger(GameObject trappedObject)
    {
        if (!TryGetTrapFromObject(trappedObject, out var trap))
        {
            return;
        }

        if (trappedObject.type == ObjectType.trap)
        {
            GameSystems.Object.Destroy(trappedObject);
        }
        else
        {
            if (trap.AfterTriggerName == null)
            {
                var trapScript = GetTrapScript(trappedObject);
                trapScript.Counter1 = 0;
                trapScript.scriptId = 0;
                SetTrapScript(trappedObject, trapScript);
            }
            else if (_trapsByName.TryGetValue(trap.AfterTriggerName, out var nextTrap))
            {
                var trapScript = GetTrapScript(trappedObject);
                trapScript.Counter1 = (byte) nextTrap.Id;
                SetTrapScript(trappedObject, trapScript);
            }
            else
            {
                Logger.Warn("Trap {0} has unknown trap '{1}' set as next trap after triggering.",
                    trap.Name, trap.AfterTriggerName);
            }
        }
    }

    [TempleDllLocation(0x10051190)]
    private void SetOffTrap(GameObject triggerer, GameObject trappedObj)
    {
        if (!TryGetTrapFromObject(trappedObj, out var trap))
        {
            return;
        }

        ObjScriptInvocation scriptInvoc = default;
        scriptInvoc.script = GetTrapScript(trappedObj);
        scriptInvoc.triggerer = triggerer;
        scriptInvoc.attachee = trappedObj;
        scriptInvoc.eventId = ObjScriptEvent.Trap;
        scriptInvoc.trapEvent = new TrapSprungEvent(trappedObj, trap);
        GameSystems.Script.Invoke(ref scriptInvoc);

        GameSystems.D20.Combat.AwardExperienceForChallengeRating(trap.ChallengeRating);
        GameUiBridge.RecordTrapSetOff(triggerer);
    }

    // Traps of difficulty <= 20 are detectable even without the find trap feat/class-feature
    private static bool HasAbilityToFindTrap(GameObject triggerer, int searchDc)
    {
        return searchDc <= 20 || GameSystems.D20.D20Query(triggerer, D20DispatcherKey.QUE_Critter_Can_Find_Traps);
    }

    [TempleDllLocation(0x10050d20)]
    [CanBeNull]
    private Trap GetTrapById(int trapId)
    {
        return _traps.GetValueOrDefault(trapId, null);
    }

    private bool TryGetTrapFromObject(GameObject trappedObj, out Trap trap)
    {
        var script = GetTrapScript(trappedObj);
        if (script.scriptId == 0)
        {
            trap = null;
            return false;
        }

        var trapId = (int) (script.counters & 0xFF);
        if (trapId <= 0)
        {
            trap = null;
            return false;
        }

        trap = GetTrapById(trapId);
        return trap != null;
    }

    [TempleDllLocation(0x10050e30)]
    public bool WillTrigger(GameObject trappedObj, ObjScriptEvent? triggerEvent = null)
    {
        if (!TryGetTrapFromObject(trappedObj, out var trap))
        {
            return false;
        }

        return !triggerEvent.HasValue || trap.Trigger == triggerEvent.Value;
    }

    /// <summary>
    /// Does the critter know about the trap and would it trigger for them?
    /// </summary>
    [TempleDllLocation(0x10050ea0)]
    public bool KnowsAboutDangerousTrap(GameObject triggerer, GameObject trappedObj)
    {
        if (triggerer == null || !triggerer.IsCritter() || !WillTrigger(trappedObj))
        {
            return false;
        }

        var script = GetTrapScript(trappedObj);

        var trapFlags = (TrapSpecFlag) script.unk1;
        if (triggerer.IsPC() || GameSystems.Critter.GetLeaderRecursive(triggerer) != null)
        {
            // PCs and members of the party know about traps set by the party, and traps they spotted 
            return (trapFlags & (TrapSpecFlag.SpottedByParty|TrapSpecFlag.PlacedByParty)) != 0;
        }
        else
        {
            // Non-PCs / Monsters always know about non-PC traps
            return (trapFlags & TrapSpecFlag.PlacedByParty) == 0;
        }
    }

    private static ObjectScript GetTrapScript(GameObject trappedObj)
    {
        return trappedObj.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Trap);
    }

    private static void SetTrapScript(GameObject trappedObj, in ObjectScript script)
    {
        trappedObj.SetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Trap, in script);
    }

    [TempleDllLocation(0x10051350)]
    public bool TryToDetectWithBonus(GameObject critter, GameObject trappedObj, BonusList searchBonus)
    {
        if (GameSystems.D20.D20Query(critter, D20DispatcherKey.QUE_CannotUseIntSkill))
        {
            return false;
        }

        if (KnowsAboutDangerousTrap(critter, trappedObj))
        {
            return false;
        }

        if (!TryGetTrapFromObject(trappedObj, out var trap))
        {
            return false;
        }
        
        var dc = trap.SearchDC;
        if (!HasAbilityToFindTrap(critter, dc))
        {
            return false;
        }

        var bonus = critter.dispatch1ESkillLevel(SkillId.search, ref searchBonus, trappedObj,
            SkillCheckFlags.SearchForTraps);
        var roll = Dice.D20.Roll();

        var rollHistId = GameSystems.RollHistory.AddSkillCheck(
            critter, null, SkillId.search, Dice.D20, roll, dc, searchBonus
        );
        if (roll + bonus >= dc)
        {
            GameSystems.RollHistory.CreateRollHistoryString(rollHistId);
            TrapSpotted(critter, trappedObj, false);
            return true;
        }

        return false;
    }

    // TODO: Could be rolled in with TrapSpotted if SkillRoll would allow an optional bonus list to be passed
    [TempleDllLocation(0x10051250)]
    public bool TryToDetect(GameObject critter, GameObject trappedObject)
    {
        if (critter == null || !critter.IsCritter())
        {
            return false;
        }

        if (!TryGetTrapFromObject(trappedObject, out var trap))
        {
            return false;
        }

        if (KnowsAboutDangerousTrap(critter, trappedObject))
        {
            return true; // Already knows about this
        }

        var searchDc = trap.SearchDC;
        if (HasAbilityToFindTrap(critter, searchDc)
            && GameSystems.Skill.SkillRoll(critter, SkillId.search, searchDc, out _, SkillCheckFlags.SearchForTraps))
        {
            TrapSpotted(critter, trappedObject, false);
            return true;
        }

        return false;
    }

    [TempleDllLocation(0x100509c0)]
    private void TrapSpotted(GameObject triggerer, GameObject trappedObj, bool suppressMessage)
    {
        if (triggerer == null)
        {
            return;
        }

        if (triggerer.IsPC() && !GameSystems.Party.IsInParty(triggerer))
        {
            return;
        }

        if (!triggerer.IsNPC() || GameSystems.Critter.GetLeaderRecursive(triggerer) == null)
        {
            return;
        }

        var script = GetTrapScript(trappedObj);
        script.unk1 |= (int) TrapSpecFlag.SpottedByParty;
        if (trappedObj.type == ObjectType.trap)
        {
            GameSystems.MapObject.ClearFlags(trappedObj, ObjectFlag.DONTDRAW);
        }

        if (!suppressMessage)
        {
            var text = _translations[0];
            GameSystems.TextFloater.FloatLine(triggerer, TextFloaterCategory.Generic, TextFloaterColor.Green, text);
        }

        SetTrapScript(trappedObj, in script);
    }

    public bool OnBeforeScriptInvoked(ref ObjScriptInvocation invocation)
    {
        if (!TryGetTrapFromObject(invocation.attachee, out var trap))
        {
            return false;
        }

        if (invocation.eventId == ObjScriptEvent.Trap)
        {
            // If the triggerer doesn't know about the trap yet, give them a chance to detect it and
            // cancel the action before setting it off. Otherwise they set it off.
            if (!KnowsAboutDangerousTrap(invocation.triggerer, invocation.attachee)
                && TryToDetect(invocation.triggerer, invocation.attachee))
            {
                return true;
            }
        }
        else if (WillTrigger(invocation.attachee, invocation.eventId))
        {
            GameUiBridge.RecordTrapSetOff(invocation.triggerer);

            var trapInvocation = invocation;
            trapInvocation.eventId = ObjScriptEvent.Trap;
            trapInvocation.trapEvent = new TrapSprungEvent(invocation.attachee, trap);

            if (!GameSystems.Script.Invoke(ref trapInvocation))
            {
                return true;
            }
        }

        return false;
    }

    public void OnAfterScriptInvoked(ref ObjScriptInvocation invocation)
    {
        // Traps who lose their script after the invocation are destroyed
        if (invocation.attachee.type == ObjectType.trap && !TryGetTrapFromObject(invocation.attachee, out _))
        {
            GameSystems.Object.Destroy(invocation.attachee);
        }

        var trap = invocation.trapEvent.Type;
        if (invocation.eventId == ObjScriptEvent.Trap && trap != null)
        {
            GameSystems.D20.Combat.AwardExperienceForChallengeRating(trap.ChallengeRating);
            SetNextTrapAfterTrigger(invocation.attachee);
        }
    }
}

public class TrapFileParser
{
    private const string NoTrapName = "TRAP_NONE";

    public Dictionary<int, Trap> Traps { get; } = new();

    public void Parse(string path)
    {
        TabFile.ParseFile(path, ParseTrapRecord);
    }

    private void ParseTrapRecord(in TabFileRecord record)
    {
        var name = record[0].AsString();
        if (name == NoTrapName)
        {
            return;
        }

        var triggerEvent = ParseTriggerEvent(in record);

        var id = record[13].GetInt();

        var particleSystemId = record[3].AsString();
        if (particleSystemId.Length == 0)
        {
            particleSystemId = null;
        }

        // Name of trap to associate with the object once it triggers. TRAP_NONE is included as a trap in vanilla
        // but we translate it to null.
        var afterTriggerName = record[6].AsString();
        if (afterTriggerName == NoTrapName)
        {
            afterTriggerName = null;
        }

        Traps[id] = new Trap(
            id,
            name,
            triggerEvent,
            ParseFlags(record),
            particleSystemId,
            record[4].GetInt(),
            record[5].GetInt(),
            afterTriggerName,
            record[12].GetInt(),
            ParseDamage(record)
        );
    }

    private static TrapSpecFlag ParseFlags(in TabFileRecord record)
    {
        TrapSpecFlag flags = default;

        // In the Vanilla traps.tab there's never more than one flag.
        if (!record[2].IsEmpty)
        {
            var flagNames = record[2].AsString().Split(",");
            foreach (var flagName in flagNames)
            {
                flags |= flagName.ToUpperInvariant() switch
                {
                    "TRAP_F_IN_STONE" => TrapSpecFlag.InStone,
                    "TRAP_F_PC" => TrapSpecFlag.PlacedByParty,
                    "TRAP_F_SPOTTED" => TrapSpecFlag.SpottedByParty,
                    "TRAP_F_MAGICAL" => TrapSpecFlag.Magical,
                    _ => throw new InvalidDataException($"Unknown Trap flag: {flagName}")
                };
            }
        }

        return flags;
    }

    private static IImmutableList<TrapDamage> ParseDamage(in TabFileRecord record)
    {
        var result = new List<TrapDamage>(5);
        for (var i = 0; i < 5; i++)
        {
            var tabFileColumn = record[7 + i];
            if (!tabFileColumn.IsEmpty)
            {
                var damageParts = tabFileColumn.AsString().Split(" ");
                var damageType = DamageTypes.GetDamageTypeByName(damageParts[0]);
                var dice = Dice.Parse(damageParts[1]);
                result.Add(new TrapDamage(damageType, dice));
            }
        }

        return result.ToImmutableList();
    }

    private static ObjScriptEvent ParseTriggerEvent(in TabFileRecord record)
    {
        var triggerEventName = record[1];
        if (triggerEventName.EqualsIgnoreCase("san_use"))
        {
            return ObjScriptEvent.Use;
        }

        if (triggerEventName.EqualsIgnoreCase("san_unlock"))
        {
            return ObjScriptEvent.Unlock;
        }

        throw new InvalidDataException("Unsupported trigger script event: " + triggerEventName.AsString());
    }
}