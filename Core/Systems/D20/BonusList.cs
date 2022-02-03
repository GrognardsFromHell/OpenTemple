using System;
using System.Text;
using OpenTemple.Core.Systems.Feats;

namespace OpenTemple.Core.Systems.D20;

public struct BonusEntry
{
    public int bonValue;
    public int bonType; // types 0, 8 and 21 can stack ( 100E6490 ); use negative number for modifier
    public string bonusMesString; // parsable string for the help system e.g. "~Item~[TAG_ITEM]"
    public string bonusDescr; // e.g. "Magic Full Plate +1"
}

public struct BonusCap
{
    public int capValue;
    public int bonType;
    public string bonCapperString;
    public string bonCapDescr;
}

public struct BonusList
{
    public BonusEntry[] bonusEntries;
    public int bonCount;
    public BonusCap[] bonCaps;
    public int bonCapperCount;

    /// a line from the bonus.mes that is auto assigned a 0 value (I think it will print ---). Probably for overrides like racial immunity and stuff.
    public int[] zeroBonusReasonMesLine;

    public int zeroBonusCount;

    /// init to largest  positive int; controlls what the sum of all the modifiers of various types cannot exceed
    public BonusEntry overallCapHigh;

    /// init to most negative int
    public BonusEntry overallCapLow;

    /// init 0; 0x1 - overallCapHigh set; 0x2 - overallCapLow set; 0x4 - force cap override (otherwise it can only impose restrictions i.e. it will only change the cap if it's lower than the current one)
    public int bonFlags;

    public static BonusList Create() => Default;

    public static BonusList Default => new BonusList
    {
        bonCount = 0,
        bonCapperCount = 0,
        zeroBonusCount = 0,
        bonFlags = 0,
        bonusEntries = new BonusEntry[40],
        bonCaps = new BonusCap[10],
        zeroBonusReasonMesLine = new int[10],
        overallCapHigh = new BonusEntry
        {
            bonValue = int.MaxValue
        },
        overallCapLow = new BonusEntry
        {
            bonValue = int.MinValue
        }
    };

    /// <summary>
    /// Returns true if the given bonus is capped by one of the caps in this bonus list and returns the
    /// index of the lowest cap in cappedByIdx, if the pointer is not null.
    /// </summary>
    [TempleDllLocation(0x100E6410)]
    public bool IsBonusCapped(int bonusIdx, out int cappedByIdx)
    {
        var lowestCap = 255;
        var foundCap = false;
        cappedByIdx = -1;

        ref var bonus = ref bonusEntries[bonusIdx];

        for (var i = 0; i < bonCapperCount; ++i)
        {
            // Caps apparently can apply to all bonus types or only specific ones
            if (bonCaps[i].bonType != 0 && bonCaps[i].bonType != bonus.bonType)
            {
                continue;
            }

            var capVal = Math.Abs(bonCaps[i].capValue);
            if (capVal < lowestCap)
            {
                lowestCap = capVal;
                cappedByIdx = i;
                foundCap = true;
            }
        }

        if (!foundCap || bonus.bonValue < lowestCap)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the given bonus is suppressed by another bonus (i.e. of the same type and
    /// the bonus type does not stack). If true is returned, the idx of the supressing bonus is returned
    /// in suppressedByIdx.
    /// </summary>
    [TempleDllLocation(0x100E6490)]
    public bool IsBonusSupressed(int bonusIdx, out int suppressedByIdx)
    {
        var curHighest = bonusEntries[bonusIdx].bonValue;
        var type = bonusEntries[bonusIdx].bonType;
        var isMalus = (curHighest <= 0);
        var curIdx = bonusIdx;
        var suppressed = false;
        suppressedByIdx = -1;

        // These bonus types stack and are therefor never suppressed
        if (type == 0 || type == 8 || type == 21)
        {
            return false;
        }

        for (var i = 0; i < bonCount; ++i)
        {
            ref var other = ref bonusEntries[i];
            // Cannot be suppressed by itself or a bonus of another type
            if (i == bonusIdx || other.bonType != type)
            {
                continue;
            }

            // For bonuses of the same value, we use their position in the list as the tiebreaker
            // For bonuses, it's the first, for maluses it's the last
            if (isMalus)
            {
                if (other.bonValue > curHighest || (other.bonValue == curHighest && i < bonusIdx))
                {
                    continue;
                }
            }
            else
            {
                if (other.bonValue < curHighest || (other.bonValue == curHighest && i > bonusIdx))
                {
                    continue;
                }
            }

            suppressed = true;
            curIdx = i;
            curHighest = other.bonValue;
        }

        if (suppressed)
        {
            suppressedByIdx = curIdx;
        }

        return suppressed;
    }

    [TempleDllLocation(0x100E65C0)]
    public int OverallBonus
    {
        get
        {
            var bonusSum = 0;

            for (var index = 0; index < bonCount; index++)
            {
                var bonusValue = bonusEntries[index].bonValue;
                if (IsBonusCapped(index, out var capIndex))
                {
                    var capValue = bonCaps[capIndex].capValue;
                    if (bonusValue > capValue && bonusValue > 0)
                    {
                        bonusValue = capValue;
                    }
                    else if (bonusValue < capValue && bonusValue < 0)
                    {
                        bonusValue = capValue;
                    }
                }

                if (!IsBonusSupressed(index, out _))
                {
                    bonusSum += bonusValue;
                }
            }

            if ((bonFlags & 1) != 0 && bonusSum > overallCapHigh.bonValue)
            {
                bonusSum = overallCapHigh.bonValue;
            }

            if ((bonFlags & 2) != 0 && bonusSum < overallCapLow.bonValue)
            {
                bonusSum = overallCapLow.bonValue;
            }

            return bonusSum;
        }
    }

    [TempleDllLocation(0x100e6680)]
    public int HighestBonus
    {
        get
        {
            var highestBonus = 0;

            for (var index = 0; index < bonCount; index++)
            {
                var bonusValue = bonusEntries[index].bonValue;
                if (IsBonusCapped(index, out var capIndex))
                {
                    var capValue = bonCaps[capIndex].capValue;
                    if (bonusValue > capValue && bonusValue > 0)
                    {
                        bonusValue = capValue;
                    }
                    else if (bonusValue < capValue && bonusValue < 0)
                    {
                        bonusValue = capValue;
                    }
                }

                if (!IsBonusSupressed(index, out _) && bonusValue > highestBonus)
                {
                    highestBonus = bonusValue;
                }
            }

            if ((bonFlags & 1) != 0 && highestBonus > overallCapHigh.bonValue)
            {
                highestBonus = overallCapHigh.bonValue;
            }

            if ((bonFlags & 2) != 0 && highestBonus < overallCapLow.bonValue)
            {
                highestBonus = overallCapLow.bonValue;
            }

            return highestBonus;
        }
    }

    /// <summary>
    /// Adds a bonus of a particular type.
    /// Will register in the D20 roll history using the specified line from bonus.mes
    /// </summary>
    [TempleDllLocation(0x100e6260)]
    [TempleDllLocation(0x100e6110)]
    public bool AddBonus(int bonValue, int bonType, int bonusDescriptionId, string description = null)
    {
        if (bonCount >= bonusEntries.Length)
        {
            return false;
        }

        bonusEntries[bonCount].bonValue = bonValue;
        bonusEntries[bonCount].bonType = bonType;
        bonusEntries[bonCount].bonusMesString = GameSystems.D20.BonusSystem.GetBonusDescription(bonusDescriptionId);
        bonusEntries[bonCount].bonusDescr = description;
        bonCount++;

        return true;
    }

    public bool AddBonus(int bonValue, int bonType, string description = null)
    {
        if (bonCount >= bonusEntries.Length)
        {
            return false;
        }

        bonusEntries[bonCount].bonValue = bonValue;
        bonusEntries[bonCount].bonType = bonType;
        bonusEntries[bonCount].bonusMesString = description;
        bonusEntries[bonCount].bonusDescr = null;
        bonCount++;

        return true;
    }

    [TempleDllLocation(0x100e6520)]
    public bool ReplaceBonus(int bonType, int bonValue, int bonusDescriptionId, string description = null)
    {
        for (var i = 0; i < bonCount; i++)
        {
            ref var entry = ref bonusEntries[i];
            if (entry.bonType == bonType)
            {
                entry.bonValue = bonValue;
                var text = GameSystems.D20.BonusSystem.GetBonusDescription(bonusDescriptionId);
                entry.bonusMesString = text;
                entry.bonusDescr = description;
                return true;
            }
        }

        return false;
    }

    [TempleDllLocation(0x100e6590)]
    [TempleDllLocation(0x100e61a0)]
    [TemplePlusLocation("bonus.cpp:29")]
    public bool SetOverallCap(int newBonFlags, int newCap, int newCapType, int newCapMesLineNum,
        string capDescr = null)
    {
        if (newBonFlags == 0)
            return false;

        var bonMesString = GameSystems.D20.BonusSystem.GetBonusDescription(newCapMesLineNum);

        if ((newBonFlags & 1) != 0 && (overallCapHigh.bonValue > newCap || (newBonFlags & 4) != 0))
        {
            overallCapHigh.bonValue = newCap;
            overallCapHigh.bonType = newCapType;
            overallCapHigh.bonusMesString = bonMesString;
            overallCapHigh.bonusDescr = capDescr;
        }

        if ((newBonFlags & 2) != 0 && (overallCapLow.bonValue < newCap || (newBonFlags & 4) != 0))
        {
            overallCapLow.bonValue = newCap;
            overallCapLow.bonType = newCapType;
            overallCapLow.bonusMesString = bonMesString;
            overallCapLow.bonusDescr = capDescr;
        }

        bonFlags |= newBonFlags;
        return true;
    }

    [TempleDllLocation(0x100e62a0)]
    [TempleDllLocation(0x100e6340)]
    public void AddCap(int capType, int capValue, int bonusDescriptionId, string descriptionText = null)
    {
        if (bonCapperCount >= bonCaps.Length)
        {
            return;
        }

        bonCaps[bonCapperCount].capValue = capValue;
        bonCaps[bonCapperCount].bonType = capType;
        bonCaps[bonCapperCount].bonCapperString =
            GameSystems.D20.BonusSystem.GetBonusDescription(bonusDescriptionId);
        bonCaps[bonCapperCount].bonCapDescr = descriptionText;
        bonCapperCount++;
    }

    // TODO: Rename to "AddNote" or sth like that
    [TempleDllLocation(0x100e6380)]
    public void zeroBonusSetMeslineNum(int zeroBonusMesLineNum)
    {
        var count = zeroBonusCount;
        if (count >= zeroBonusReasonMesLine.Length)
        {
            return;
        }

        zeroBonusReasonMesLine[count] = zeroBonusMesLineNum;
        ++zeroBonusCount;
    }

    public void AddBonusFromFeat(int value, int bonType, int mesline, FeatId feat)
    {
        var featName = GameSystems.Feat.GetFeatName(feat);
        AddBonus(value, bonType, mesline, featName);
    }


    public void ModifyBonus(int value, int bonType, int meslineIdentifier)
    {
        var lineText = GameSystems.D20.BonusSystem.GetBonusDescription(meslineIdentifier);

        for (var i = 0; i < bonCount; i++)
        {
            ref var entry = ref bonusEntries[i];

            if (entry.bonType == bonType)
            {
                if (meslineIdentifier != 0 && lineText == entry.bonusMesString)
                {
                    entry.bonValue += value;
                }

                if (meslineIdentifier != 0)
                    break;
            }
        }
    }

    [TempleDllLocation(0x1019b730)]
    public readonly void FormatTo(StringBuilder builder)
    {
        var bonusCount = BonusLineCount;
        for (var i = 0; i < bonusCount; ++i)
        {
            FormatBonusLine(i, out var bonValueOut, out var strBonusOut, out var strCapOut);
            if (strBonusOut != null)
            {
                if (bonValueOut <= 0)
                {
                    if (bonValueOut >= 0)
                    {
                        builder.Append("--");
                    }
                    else
                    {
                        builder.Append(bonValueOut);
                    }
                }
                else
                {
                    builder.Append('+');
                    builder.Append(bonValueOut);
                }

                builder.Append("@t");
                builder.Append(strBonusOut);

                if (strCapOut != null)
                {
                    builder.Append(strCapOut);
                }

                builder.Append("\n");
            }
        }
    }

    [TempleDllLocation(0x100e63b0)]
    public readonly int BonusLineCount => bonCount + zeroBonusCount;

    [TempleDllLocation(0x100e6740)]
    public readonly void FormatBonusLine(int index, out int bonValueOut, out string strBonusOut,
        out string strCapOut)
    {
        if (index < bonCount)
        {
            strBonusOut = null;
            ref var bonEntry = ref bonusEntries[index];
            strCapOut = null;
            bonValueOut = bonEntry.bonValue;
            if (bonEntry.bonValue != 0)
            {
                if (bonEntry.bonusDescr != null)
                {
                    strBonusOut = $"{bonEntry.bonusMesString} : {bonEntry.bonusDescr}";
                }
                else
                {
                    strBonusOut = $"{bonEntry.bonusMesString}";
                }

                if (IsBonusCapped(index, out var capIndex))
                {
                    var cap = bonCaps[capIndex];
                    bonValueOut = cap.capValue;
                    var uncappedValue = bonEntry.bonValue;

                    string prefix;
                    string suffix;
                    if (cap.capValue != 0)
                    {
                        if (uncappedValue <= 0)
                        {
                            prefix = GameSystems.D20.BonusSystem.GetBonusDescription(8); // penalty reduced by
                            suffix = GameSystems.D20.BonusSystem.GetBonusDescription(9); //
                        }
                        else
                        {
                            prefix = GameSystems.D20.BonusSystem.GetBonusDescription(6); // bonus reduced by
                            suffix = GameSystems.D20.BonusSystem.GetBonusDescription(7); //
                        }
                    }
                    else if (uncappedValue <= 0)
                    {
                        prefix = GameSystems.D20.BonusSystem.GetBonusDescription(2); // no penalty due to
                        suffix = GameSystems.D20.BonusSystem.GetBonusDescription(3); //
                    }
                    else
                    {
                        prefix = GameSystems.D20.BonusSystem.GetBonusDescription(0); // bonus lost due to
                        suffix = GameSystems.D20.BonusSystem.GetBonusDescription(1);
                    }

                    var capDescr = cap.bonCapDescr;
                    if (capDescr != null)
                    {
                        strCapOut = $"{prefix}{cap.bonCapperString} : {capDescr}{suffix}";
                    }
                    else
                    {
                        strCapOut = $"{prefix}{cap.bonCapperString}{suffix}";
                    }
                }
                else if (IsBonusSupressed(index, out var suppressedByIdx))
                {
                    ref var otherBonus = ref bonusEntries[suppressedByIdx];
                    bonValueOut = 0;
                    string cappedByDesc;
                    if (otherBonus.bonusDescr != null)
                    {
                        cappedByDesc = $"{otherBonus.bonusMesString} : {otherBonus.bonusDescr}";
                    }
                    else
                    {
                        cappedByDesc = $"{otherBonus.bonusMesString}";
                    }

                    var prefix = GameSystems.D20.BonusSystem.GetBonusDescription(4); // does not stack with
                    var suffix = GameSystems.D20.BonusSystem.GetBonusDescription(5);
                    strCapOut = $"{prefix}{cappedByDesc}{suffix}";
                }

                if ((bonFlags & 1) != 0)
                {
                    var prefix = GameSystems.D20.BonusSystem.GetBonusDescription(10); // bonus capped (high) by
                    var suffix = GameSystems.D20.BonusSystem.GetBonusDescription(11);
                    if (overallCapHigh.bonusDescr != null)
                    {
                        strCapOut =
                            $"{strCapOut} [{prefix} {overallCapHigh.bonusMesString}{overallCapHigh.bonusDescr}{suffix}]";
                    }
                    else
                    {
                        strCapOut = $"{strCapOut} [{prefix} {overallCapHigh.bonusMesString}{suffix}]";
                    }
                }

                if ((bonFlags & 2) != 0)
                {
                    var prefix = GameSystems.D20.BonusSystem.GetBonusDescription(12); // bonus capped (low) by
                    var suffix = GameSystems.D20.BonusSystem.GetBonusDescription(13);
                    if (overallCapLow.bonusDescr != null)
                    {
                        strCapOut =
                            $"{strCapOut} [{prefix} {overallCapLow.bonusMesString}{overallCapLow.bonusDescr}{suffix}]";
                    }
                    else
                    {
                        strCapOut = $"{strCapOut} [{prefix} {overallCapLow.bonusMesString}{suffix}]";
                    }
                }
            }
        }
        else if (index >= bonCount && index < bonCount + zeroBonusCount)
        {
            strBonusOut = GameSystems.D20.BonusSystem.GetBonusDescription(zeroBonusReasonMesLine[index - bonCount]);
            strCapOut = null;
            bonValueOut = 0;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
    }
}