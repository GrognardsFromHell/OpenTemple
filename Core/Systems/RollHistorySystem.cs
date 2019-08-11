using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public class RollHistorySystem : IGameSystem
    {
        public void Clear()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x100dffc0)]
        public void CreateFromFreeText(string text)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10047F70)]
        public int RollHistoryAddType6OpposedCheck(GameObjectBody performer, GameObjectBody opponent,
            int roll, int opposingRoll,
            in BonusList bonus, in BonusList opposingBonus,
            int combatMesLineTitle, D20CombatMessage combatMesLineResult, int flag)
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x100DFFF0)]
        public void CreateRollHistoryString(int rollHistId)
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10047C80)]
        public int RollHistoryType1Add(GameObjectBody objHnd, GameObjectBody objHnd2, DamagePacket damPkt)
        {
            throw new System.NotImplementedException();

        }

        [TempleDllLocation(0x10047CF0)]
        public int RollHistoryType2Add(GameObjectBody objHnd, GameObjectBody objHnd2, SkillId skillIdx, Dice dice, int rollResult,
            int dc, in BonusList bonlist)
        {
            throw new System.NotImplementedException();

        }

        [TempleDllLocation(0x10047D90)]
        public int RollHistoryType3Add(GameObjectBody obj, int dc, SavingThrowType saveType, D20SavingThrowFlag flags,
            Dice dice, int rollResult, in BonusList bonListIn)
        {
            throw new System.NotImplementedException();

        }

        [TempleDllLocation(0x10047e30)]
        public int RollHistoryType4Add(GameObjectBody obj, int dc, string historyText, Dice dice, int rollResult, BonusList bonusList)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10047ec0)]
        public int RollHistoryType5Add(GameObjectBody obj, GameObjectBody tgt, int failChance, int combatMesFailureReason, int rollResult, int combatMesResult, int combatMesTitle){
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100E01F0)]
        public int CreateRollHistoryLineFromMesfile(int historyMesLine, GameObjectBody obj, GameObjectBody obj2)
        {
            throw new System.NotImplementedException();

        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}