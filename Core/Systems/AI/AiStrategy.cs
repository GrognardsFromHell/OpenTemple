using System.Collections.Generic;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.Spells;

namespace OpenTemple.Core.Systems.AI;

public interface AiTacticDef
{
    string name { get; }
    bool aiFunc(AiTactic tactic);
}

public class AiTactic
{
    public AiTacticDef aiTac;
    public int field4;
    public GameObject performer;
    public GameObject target;
    public int tacticIdx;
    public D20SpellData d20SpellData;
    public int field24;
    public SpellPacketBody spellPktBody;

    public AiTactic(GameObject performer, GameObject target)
    {
        this.performer = performer;
        this.target = target;
        spellPktBody = new SpellPacketBody();
    }
}

public class AiStrategy
{
    public string name { get; }
    public List<AiTacticDef> aiTacDefs = new();
    public List<int> field54 = new();
    public List<SpellStoreData> spellsKnown = new();
    public int numTactics => aiTacDefs.Count;

    public AiStrategy(string name)
    {
        this.name = name;
    }

    public void GetConfig(int tacIdx, AiTactic aiTacOut)
    {
        var spellPktBody = new SpellPacketBody();
        aiTacOut.spellPktBody = spellPktBody;

        aiTacOut.aiTac = aiTacDefs[tacIdx];
        aiTacOut.field4 = field54[tacIdx];
        aiTacOut.tacticIdx = tacIdx;

        var spellEnum = spellsKnown[tacIdx].spellEnum;
        spellPktBody.spellEnum = spellEnum;
        spellPktBody.spellEnumOriginal = spellEnum;
        if (spellEnum != -1)
        {
            aiTacOut.spellPktBody.caster = aiTacOut.performer;
            aiTacOut.spellPktBody.spellClass = spellsKnown[tacIdx].classCode;
            aiTacOut.spellPktBody.spellKnownSlotLevel = spellsKnown[tacIdx].spellLevel;
            GameSystems.Spell.SpellPacketSetCasterLevel(spellPktBody);
            aiTacOut.d20SpellData.SetSpellData(spellEnum, spellPktBody.spellClass,
                spellPktBody.spellKnownSlotLevel, -1, spellPktBody.metaMagicData);
        }
    }
}