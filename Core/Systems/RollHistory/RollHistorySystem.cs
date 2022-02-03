using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Help;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Time;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.RollHistory;

public class RollHistorySystem : IGameSystem, IResetAwareSystem, ISaveGameAwareGameSystem
{
    [TempleDllLocation(0x109DDA20)]
    private readonly List<HistoryEntry> _historyArray = new List<HistoryEntry>();

    [TempleDllLocation(0x102b016c)]
    private int rollSerialNumber;

    [TempleDllLocation(0x109dda18)]
    private int lastHistoryId;

    [TempleDllLocation(0x11868f80)]
    public event Action<D20RollHistoryLine> OnHistoryLineAdded;

    public event Action OnHistoryCleared;

    public event Action<HistoryEntry> OnHistoryEvent;

    private readonly Dictionary<int, string> _translations;

    private readonly Dictionary<int, string> _messageTranslations;

    [TempleDllLocation(0x100475d0)]
    public RollHistorySystem()
    {
        Reset();
        _translations = Tig.FS.ReadMesFile("mes/roll_ui.mes");
        _messageTranslations = Tig.FS.ReadMesFile("mes/history.mes");
    }

    [TempleDllLocation(0x10047160)]
    public void Reset()
    {
        _historyArray.Clear();
        rollSerialNumber = 0;
        lastHistoryId = 0;

        OnHistoryCleared?.Invoke();
    }

    [TempleDllLocation(0x100471a0)]
    public void SaveGame(SavedGameState savedGameState)
    {
        savedGameState.D20RollsState = new SavedD20RollsState
        {
            NextRollId = rollSerialNumber
        };
    }

    [TempleDllLocation(0x100471e0)]
    public void LoadGame(SavedGameState savedGameState)
    {
        rollSerialNumber = savedGameState.D20RollsState.NextRollId;
    }

    [TempleDllLocation(0x100dffc0)]
    public void CreateFromFreeText(string text)
    {
        // TODO: This actually belongs to another subsystem (the roll console)
        Stub.TODO();
    }

    [TempleDllLocation(0x10047430)]
    private int AddHistoryEntry(HistoryEntry entry)
    {
        // Assign a history entry serial number and time of recording
        entry.histId = ++rollSerialNumber;
        entry.recorded = TimePoint.Now;
        _historyArray.Insert(0, entry);

        if (entry.obj != null)
        {
            entry.objId = GameSystems.Object.GetPersistableId(entry.obj);
            entry.objDescr = GameSystems.MapObject.GetDisplayNameForParty(entry.obj);
        }
        else
        {
            entry.objId = ObjectId.CreateNull();
            entry.objDescr = "";
        }

        if (entry.obj2 != null)
        {
            entry.obj2Id = GameSystems.Object.GetPersistableId(entry.obj2);
            entry.obj2Descr = GameSystems.MapObject.GetDisplayNameForParty(entry.obj2);
        }
        else
        {
            entry.obj2Id = ObjectId.CreateNull();
            entry.obj2Descr = "";
        }

        OnHistoryEvent?.Invoke(entry);

        return entry.histId;
    }

    [TempleDllLocation(0x100dfff0)]
    public void CreateRollHistoryString(int histId)
    {
        // TODO: This actually belongs to another subsystem (the roll console)
        if (histId != -1)
        {
            var builder = new StringBuilder();
            FormatHistoryEntry(histId, builder);
            if (builder.Length > 0)
            {
                OnHistoryLineAdded?.Invoke(D20RollHistoryLine.Create(builder.ToString()));
            }
        }
    }

    [TempleDllLocation(0x10047bd0)]
    public int AddAttackRoll(int attackRoll, int criticalConfirmRoll, GameObject attacker,
        GameObject defender,
        BonusList attackerBonus, BonusList defenderBonus, D20CAF flags)
    {
        var entry = new HistoryAttackRoll
        {
            rollRes = attackRoll,
            critRollRes = criticalConfirmRoll,
            obj = attacker,
            obj2 = defender,
            bonlist = attackerBonus,
            defenderRollId = AddMiscBonus(defender, defenderBonus, 33, 0),
            d20Caf = flags,
            defenderOverallBonus = defenderBonus.OverallBonus
        };
        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047C80)]
    public int AddDamageRoll(GameObject attacker, GameObject victim, DamagePacket damPkt)
    {
        var entry = new HistoryDamageRoll(damPkt); // TODO: We should actually make a *copy* of the packet here
        entry.obj = attacker;
        entry.obj2 = victim;

        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047CF0)]
    public int AddSkillCheck(GameObject objHnd, GameObject objHnd2, SkillId skillIdx, Dice dice,
        int rollResult,
        int dc,
        in BonusList bonlist)
    {
        var entry = new HistorySkillCheck();
        entry.obj = objHnd;
        entry.obj2 = objHnd2;
        entry.dice = dice;
        entry.rollResult = rollResult;
        entry.skillIdx = skillIdx;
        entry.dc = dc;
        entry.bonlist = bonlist;

        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047D90)]
    public int AddSavingThrow(GameObject obj, int dc, SavingThrowType saveType, D20SavingThrowFlag flags,
        Dice dice, int rollResult, in BonusList bonListIn)
    {
        var entry = new HistorySavingThrow();
        entry.obj = obj;
        entry.dc = dc;
        entry.saveType = saveType;
        entry.saveFlags = flags;
        entry.dicePacked = dice;
        entry.rollResult = rollResult;
        entry.bonlist = bonListIn;
        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047e30)]
    public int AddMiscCheck(GameObject obj, int dc, string historyText, Dice dice, int rollResult,
        BonusList bonusList)
    {
        var entry = new HistoryMiscCheck();
        entry.obj = obj;
        entry.dicePacked = dice;
        entry.rollResult = rollResult;
        entry.dc = dc;
        entry.bonlist = bonusList;
        entry.text = historyText;
        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047ec0)]
    public int AddPercentageCheck(GameObject obj, GameObject tgt, int failChance,
        int combatMesFailureReason, int rollResult, int combatMesResult, int combatMesTitle)
    {
        var entry = new HistoryPercentageCheck();
        entry.obj = obj;
        entry.obj2 = tgt;
        entry.rollResult = rollResult;
        entry.failureChance = failChance;
        entry.combatMesFailureReason = combatMesFailureReason;
        entry.combatMesResult = combatMesResult;
        entry.combatMesTitle = combatMesTitle;
        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x10047F70)]
    public int AddOpposedCheck(GameObject performer, GameObject opponent,
        int roll, int opposingRoll,
        in BonusList bonus, in BonusList opposingBonus,
        int combatMesLineTitle, D20CombatMessage combatMesLineResult, int flag)
    {
        var entry = new HistoryOpposedChecks();
        entry.obj = performer;
        entry.obj2 = opponent;
        entry.flags = flag;
        entry.bonusList = bonus;
        entry.roll = roll;
        entry.opposingBonus = opposingRoll;
        entry.combatMesTitleLine = combatMesLineTitle;
        entry.combatMesResultLine = combatMesLineResult;
        entry.opposingBonus = opposingBonus.OverallBonus;

        var opposingEntry = new HistoryOpposedChecks();
        opposingEntry.obj = opponent;
        opposingEntry.obj2 = performer;
        opposingEntry.flags = flag | 2;
        opposingEntry.bonusList = opposingBonus;
        opposingEntry.roll = opposingRoll;
        opposingEntry.opposingBonus = roll;
        opposingEntry.combatMesTitleLine = combatMesLineTitle;
        opposingEntry.combatMesResultLine = combatMesLineResult;
        opposingEntry.opposingBonus = bonus.OverallBonus;

        // Link the two entries
        opposingEntry.opposingHistoryEntry = entry;
        entry.opposingHistoryEntry = opposingEntry;

        AddHistoryEntry(opposingEntry);

        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x100475f0)]
    public int AddMiscBonus(GameObject critter, BonusList bonList, int line, int rollResult)
    {
        var entry = new HistoryBonusDetail(bonList, line, rollResult)
        {
            obj = critter
        };
        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x100480c0)]
    public int AddTrapAttack(int attackRoll, int criticalConfirmRoll, int attackBonus, GameObject victim,
        BonusList acBonus, D20CAF caf)
    {
        var entry = new HistoryTrap();
        entry.obj2 = victim;
        entry.attackRoll = attackRoll;
        entry.criticalConfirmRoll = criticalConfirmRoll;

        entry.attackBonus = BonusList.Create();
        entry.attackBonus.AddBonus(attackBonus, 0, 118); // Trap attack bonus

        entry.armorClassDetails = FindEntry(AddMiscBonus(victim, acBonus, 33, 0));
        entry.armorClass = acBonus.OverallBonus;
        entry.caf = caf;

        return AddHistoryEntry(entry);
    }

    [TempleDllLocation(0x100e02a0)]
    public void AddSpellCast(GameObject caster, int spellEnum)
    {
        GameObject observer;
        string actorName = null;
        if (caster != null)
        {
            observer = GameSystems.Party.GetLeader();
            actorName = GameSystems.MapObject.GetDisplayName(caster, observer);
        }

        var spellName = GameSystems.Spell.GetSpellName(spellEnum);
        var spellHelpId = GameSystems.Spell.GetSpellHelpTopic(spellEnum);

        var spellHelpLink = $"~{spellName}~[{spellHelpId}]";

        var text = _messageTranslations[49]; // [ACTOR] casts [SPELL]!
        var messageText =
            GameSystems.RollHistory.ReplaceHistoryLinePlaceholders(text, actorName, null, spellHelpLink);
        OnHistoryLineAdded?.Invoke(D20RollHistoryLine.Create(messageText));
    }

    [TempleDllLocation(0x100E01F0)]
    public void CreateRollHistoryLineFromMesfile(int historyMesLine, GameObject obj, GameObject obj2)
    {
        var messageText = _messageTranslations[historyMesLine];
        string actorName = null;
        string targetName = null;

        if (obj != null)
        {
            actorName = GameSystems.MapObject.GetDisplayNameForParty(obj);
        }

        if (obj2 != null)
        {
            targetName = GameSystems.MapObject.GetDisplayNameForParty(obj2);
        }

        messageText = ReplaceHistoryLinePlaceholders(messageText, actorName, targetName, null);

        OnHistoryLineAdded?.Invoke(D20RollHistoryLine.Create(messageText));
    }

    private const string PlaceholderActor = "[ACTOR]";
    private const string PlaceholderTarget = "[TARGET]";
    private const string PlaceholderSpell = "[SPELL]";

    [TempleDllLocation(0x100e00b0)]
    private string ReplaceHistoryLinePlaceholders(string controlString, string actorName, string targetName,
        string spellName)
    {
        var result = new StringBuilder(controlString.Length);
        ReadOnlySpan<char> chars = controlString;

        for (var i = 0; i < chars.Length;)
        {
            var rest = chars.Slice(i);
            if (rest.StartsWith(PlaceholderActor))
            {
                result.Append(actorName ?? "");
                i += PlaceholderActor.Length;
            }
            else if (rest.StartsWith(PlaceholderTarget))
            {
                result.Append(targetName ?? "");
                i += PlaceholderTarget.Length;
            }
            else if (rest.StartsWith(PlaceholderSpell))
            {
                result.Append(spellName ?? "");
                i += PlaceholderSpell.Length;
            }
            else
            {
                result.Append(chars[i]);
                i++;
            }
        }

        result.Append("\n\n");
        return result.ToString();
    }

    [TempleDllLocation(0x100475a0)]
    public string GetTranslation(int key)
    {
        return _translations[key];
    }

    public void Dispose()
    {
        Stub.TODO();
    }

    [TempleDllLocation(0x10048960)]
    [TemplePlusLocation("history.cpp:205")]
    private void FormatHistoryEntry(int histId, StringBuilder builder)
    {
        var entry = FindEntry(histId);
        if (entry != null)
        {
            entry.FormatShort(builder);
            if (builder.Length > 0)
            {
                builder.Append('\n');
            }
        }
    }

    public HistoryEntry FindEntry(int histId)
    {
        HistoryEntry entry = null;
        foreach (var arrayEntry in _historyArray)
        {
            if (arrayEntry.histId == histId)
            {
                entry = arrayEntry;
                break;
            }
        }

        return entry;
    }
}

/// <summary>
/// Note that vanilla used a circular buffer here instead.
/// </summary>
public readonly struct D20RollHistoryLine
{
    public readonly string Text;
    public readonly List<D20HelpLink> Links;

    public D20RollHistoryLine(string text, List<D20HelpLink> links)
    {
        Text = text;
        Links = links;
    }

    [TempleDllLocation(0x1010ee00)]
    public static D20RollHistoryLine Create(ReadOnlySpan<char> text)
    {
        var displayText = new StringBuilder();
        var links = new List<D20HelpLink>();
        // ProcessHelpLinks(text, displayText, links);
        return new D20RollHistoryLine(new string(text), links);
    }

    [TempleDllLocation(0x100e7c60)]
    private static void ProcessHelpLinks(ReadOnlySpan<char> rawText, StringBuilder textOut,
        IList<D20HelpLink> links)
    {
        for (var i = 0; i < rawText.Length; i++)
        {
            var rest = rawText.Slice(i);
            if (GameSystems.Help.TryParseLink("(Dynamically Generated)", rest, textOut, out var charsConsumed,
                    out var helpLink))
            {
                i += charsConsumed - 1;
                links.Add(helpLink);
                continue;
            }

            textOut.Append(rawText[i]);
        }
    }
}