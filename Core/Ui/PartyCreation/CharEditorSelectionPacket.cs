using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Ui.FlowModel;

namespace OpenTemple.Core.Ui.PartyCreation;

public class CharEditorSelectionPacket
{
    public int[] abilityStats = new int[6];
    public int numRerolls; // number of rerolls
    public bool isPointbuy;
    public InlineElement? rerollString;
    public Stat statBeingRaised;
    public RaceId? raceId; // RACE_INVALID is considered invalid
    public Gender? genderId; // 2 is considered invalid
    public int height;
    public int weight;
    public float modelScale; // 0.0 is considered invalid
    public HairStyle? hairStyle;
    public HairColor? hairColor;
    public Stat classCode;
    public DeityId? deityId;
    public DomainId domain1;
    public DomainId domain2;
    public Alignment? alignment;
    public AlignmentChoice alignmentChoice; // 1 is for Positive Energy, 2 is for Negative Energy
    public FeatId? feat0;
    public FeatId? feat1;
    public FeatId? feat2;
    public Dictionary<SkillId, int> skillPointsAdded = new(); // idx corresponds to skill enum
    public int skillPointsSpent;
    public int availableSkillPoints;
    public int[] spellEnums = new int[SpellSystem.SPELL_ENUM_MAX_VANILLA];
    public int spellEnumsAddedCount;
    public int spellEnumToRemove; // for sorcerers who swap out spells
    public SchoolOfMagic wizSchool;
    public SchoolOfMagic forbiddenSchool1;
    public SchoolOfMagic forbiddenSchool2;
    public FeatId? feat3;
    public FeatId? feat4;
    public int portraitId;
    public string voiceFile;
    public int voiceId; // -1 is considered invalid
};