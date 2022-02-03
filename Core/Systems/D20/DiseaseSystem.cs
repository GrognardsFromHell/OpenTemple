using System.Collections.Generic;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Systems.D20;

public class DiseaseSpec
{
    public int Id { get; }

    public int NameId { get; }

    public int DC { get; }

    public int ConsecutiveSavesNeeded { get; }

    public int DaysIncubation { get; }

    public Stat AbilityLossStat { get; }

    public Dice AbilityLossDice { get; }

    public DiseaseSpec(int id, int nameId, int dc, int consecutiveSavesNeeded,
        int daysIncubation, Stat abilityLossStat, Dice abilityLossDice)
    {
        Id = id;
        NameId = nameId;
        DC = dc;
        ConsecutiveSavesNeeded = consecutiveSavesNeeded;
        DaysIncubation = daysIncubation;
        AbilityLossStat = abilityLossStat;
        AbilityLossDice = abilityLossDice;
    }
}

public class DiseaseSystem
{
    private readonly Dictionary<int, DiseaseSpec> _diseases = new Dictionary<int, DiseaseSpec>
    {
        {0, new DiseaseSpec(0, 400, 16, 2, 3, Stat.strength, Dice.D4)}, // Blinding Sickness
        {1, new DiseaseSpec(1, 401, 16, 2, 0, Stat.wisdom, Dice.D6)}, // Cackle Fever
        {2, new DiseaseSpec(2, 402, 18, 2, 0, Stat.constitution, Dice.D6)}, // Demon Fever
        {3, new DiseaseSpec(3, 403, 14, 3, 3, Stat.strength, Dice.D4)}, // Devil Chills
        {4, new DiseaseSpec(4, 404, 12, 2, 3, Stat.dexterity, Dice.D3)}, // Filth Fever
        {5, new DiseaseSpec(5, 405, 12, 2, 0, Stat.intelligence, Dice.D4)}, // Mindfire
        {6, new DiseaseSpec(6, 406, 20, 2, 0, Stat.constitution, Dice.D6)}, // Mummy Rot
        {7, new DiseaseSpec(7, 407, 15, 2, 3, Stat.strength, Dice.D6)}, // Red Ache
        {8, new DiseaseSpec(8, 408, 13, 2, 0, Stat.dexterity, Dice.D8)}, // Shakes
        {9, new DiseaseSpec(9, 409, 14, 2, 0, Stat.constitution, Dice.D4)}, // Slimy Doom
    };

    public DiseaseSystem()
    {
    }

    public string GetName(DiseaseSpec spec) => GameSystems.D20.Combat.GetCombatMesLine(spec.Id);

    public DiseaseSpec GetDisease(int id) => _diseases[id];
}