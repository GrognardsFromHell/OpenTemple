using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems.GameObjects;

namespace OpenTemple.Core.Systems.Dialog
{

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
        public DialogScript dialogScript;
        public int unk;
        public GameObject pc;
        public ObjectId pcId; // TimeEventObjInfo for PC
        public GameObject npc;
        public ObjectId npcId; // TimeEventObjInfo for NPC
        public int reqNpcLineId;
        public int dialogScriptId; // current known use is for speech
        public string npcLineText;
        /*
            If the speaker is a PC, this is just the speech file id.
            If the speaker is a NPC, the upper 16-bit are his dlg script id, the
            lower 16-bit are the voice sample id.
        */
        public int speechId;
        public string[] pcLineText;
        public DialogSkill[] pcLineSkillUse;
        /*
           determined by the reply Op Code
            0 - normal
            1 - exit
            2 - barter
            3 - Exit with bubble
            7 - something deprecated I think
            4,5,6,8 - ??
         */
        public int actionType;
        public int lineNumber;
        public DialogReplyOpCode[] pcReplyOpcode;
        public int[] npcReplyIds; // the ID of the NPC response to each PC line
        public int[] effectLineKey; // The line# from which the effect should be run
        public int answerLineId;
        public int rngSeed;
        public int field_185C;
        // Used by the rumor for money opcode
        public ReplyOp askForMoneyOp;

        public DialogState(GameObject speaker, GameObject listener)
        {
            npc = speaker;
            npcId = speaker.id;
            pc = listener;
            pcId = listener.id;
            dialogScriptId = 0;
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

}