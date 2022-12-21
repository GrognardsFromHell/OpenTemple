using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.Systems.Dialog;

public enum DialogSkill
{
    None = 0,
    Bluff,
    Diplomacy,
    Intimidate,
    SenseMotive,
    GatherInformation
}

public class DialogState
{
    public DialogScript DialogScript;
    public int Unk;
    public GameObject Pc;
    public ObjectId PcId; // TimeEventObjInfo for PC
    public GameObject NPC;
    public ObjectId NPCId; // TimeEventObjInfo for NPC
    public int ReqNpcLineId;
    public int DialogScriptId; // current known use is for speech
    public string NPCLineText;
    /// <summary>
    /// If the speaker is a PC, this is just the speech file id.
    /// If the speaker is a NPC, the upper 16-bit are his dlg script id, the
    /// lower 16-bit are the voice sample id.
    /// </summary>
    public int SpeechId;
    public string[] PcLineText;
    public DialogSkill[] PcLineSkillUse;
    /// <summary>
    /// determined by the reply Op Code
    /// 0 - normal
    /// 1 - exit
    /// 2 - barter
    /// 3 - Exit with bubble
    /// 7 - something deprecated I think
    /// 4,5,6,8 - ??
    /// </summary>
    public int ActionType;
    public int LineNumber;
    public DialogReplyOpCode[] PcReplyOpcode;
    public int[] NPCReplyIds; // the ID of the NPC response to each PC line
    public int[] EffectLineKey; // The line# from which the effect should be run
    public int AnswerLineId;
    public int RngSeed;
    public int Field185C;
    // Used by the rumor for money opcode
    public ReplyOp AskForMoneyOp;

    public DialogState(GameObject speaker, GameObject listener)
    {
        NPC = speaker;
        NPCId = speaker.id;
        Pc = listener;
        PcId = listener.id;
        DialogScriptId = 0;
    }
}


/*
     for regular lines: depends on NPC response ID to the line.
            answer ID = 0 then 1 (exit);
            answer ID > 0 then 0 (normal go to line);
            answer ID < 0 then 2 (normal go to line, but skip effect)
     for barter lines : 3 (26 if NPC has to sell equipment first)
     for rumor lines  : 8
 */
public enum DialogReplyOpCode
{
    GoToLine = 0,
    ExitDialog = 1,
    GoToLineWithoutEffect = 2,
    Barter = 3,
    /// <summary>
    /// Deducts the money for a rumor and proceeds with handing it out.
    /// </summary>
    AskForMoney = 4,
    /// <summary>
    /// Offer a rumor (at a cost)
    /// </summary>
    OfferRumor = 8,
    /// <summary>
    /// Gives out the actual rumor.
    /// </summary>
    GiveRumor = 9,
    // 24 was Arcanum Story State related
    NpcSellOffThenBarter = 26,

}

public readonly struct ReplyOp
{
    public readonly DialogReplyOpCode OpCode;
    public readonly int Argument;

    public ReplyOp(DialogReplyOpCode opCode, int argument)
    {
        OpCode = opCode;
        Argument = argument;
    }
}