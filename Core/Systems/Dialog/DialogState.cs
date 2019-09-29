using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems.Dialog
{
    public class DialogState
    {
        public DialogScript dialogScript;
        public int unk;
        public GameObjectBody pc;
        public ObjectId pcId; // TimeEventObjInfo for PC
        public GameObjectBody npc;
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
        public int[] pcLineSkillUse; // 0 - none, 1 - bluff, 2 -diplo, 3 - intimidate, 4 - sense motive, 5 - gather info
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
        /*
             for regular lines: depends on NPC response ID to the line.
                    answer ID = 0 then 1 (exit);
                    answer ID > 0 then 0 (normal go to line);
                    answer ID < 0 then 2 (normal go to line, but skip effect)
             for barter lines : 3 (26 if NPC has to sell equipment first)
             for rumor lines  : 8
         */
        public int[] pcReplyOpcode;
        public int[] npcReplyIds; // the ID of the NPC response to each PC line
        public int[] field_182C;     // I'm guessing this was the test field for each line, but no reason to replicate it here since these are already compiled from actually possible responses
        public string[] effectFields; // python commands to run for each line
        public int answerLineId;
        public int rngSeed;
        public int field_185C;

        public DialogState(GameObjectBody speaker, GameObjectBody listener)
        {
            npc = speaker;
            npcId = speaker.id;
            pc = listener;
            pcId = listener.id;
            dialogScriptId = 0;
        }
    }
}