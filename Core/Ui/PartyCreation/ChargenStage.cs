using System;
using System.Collections.Immutable;

namespace OpenTemple.Core.Ui.PartyCreation;

public enum ChargenStage
{
    Stats = 0,
    Race,
    Gender,
    Height,
    Hair,
    Class,
    Alignment,
    Deity,
    ClassFeatures,
    Feats,
    Skills,
    Spells,
    Portrait,
    Voice
}

public static class ChargenStages
{
    public static IImmutableList<ChargenStage> All = ImmutableList.Create(Enum.GetValues<ChargenStage>());

    public static ChargenStage First => All[0];

    public static ChargenStage Last => All[^1];

    public static int Count => All.Count;
}
