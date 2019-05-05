using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.GameObjects;

namespace SpicyTemple.Core.Systems
{
    public class PartySystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem
    {

        public void Dispose()
        {
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        public void AddToPCGroup(in ObjHndl objHndl)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002b1b0)]
        public bool IsInParty(GameObjectBody obj)
        {
            // TODO
            return false;
        }

        public ObjHndl GetLeader()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002B170)]
        public GameObjectBody GetPCGroupMemberN(int index)
        {
            throw new NotImplementedException();
        }

        // One entry per coin type
        private int[] _partyMoney = new int[4];

        [TempleDllLocation(0x1002B750)]
        public int GetPartyMoney()
        {
            return GetWorthInCopper(MoneyType.Copper) * GetPartyMoneyOfType(MoneyType.Copper)
                   + GetWorthInCopper(MoneyType.Silver) * GetPartyMoneyOfType(MoneyType.Silver)
                   + GetWorthInCopper(MoneyType.Gold) * GetPartyMoneyOfType(MoneyType.Gold)
                   + GetWorthInCopper(MoneyType.Platinum) * GetPartyMoneyOfType(MoneyType.Platinum);
        }

        private ref int GetPartyMoneyOfType(MoneyType type) => ref _partyMoney[(int) type];

        [TempleDllLocation(0x1002B780)]
        public void GetPartyMoneyCoins(out int platinCoins, out int goldCoins, out int silverCoins, out int copperCoins)
        {
            platinCoins = GetPartyMoneyOfType(MoneyType.Platinum);
            goldCoins = GetPartyMoneyOfType(MoneyType.Gold);
            silverCoins = GetPartyMoneyOfType(MoneyType.Silver);
            copperCoins = GetPartyMoneyOfType(MoneyType.Copper);
        }

        /// <summary>
        /// Calculates the overall worth of a bunch of coins.
        /// </summary>
        [TempleDllLocation(0x1002B880)]
        public int GetCoinWorth(int platinCoins = 0, int goldCoins = 0, int silverCoins = 0, int copperCoins = 0)
        {
            return GetWorthInCopper(MoneyType.Platinum) * platinCoins
                   + GetWorthInCopper(MoneyType.Gold) * goldCoins
                   + GetWorthInCopper(MoneyType.Silver) * silverCoins
                   + copperCoins;
        }

        /// <summary>
        /// Returns the worth of a piece of currency in copper coins based on its type.
        /// </summary>
        [TempleDllLocation(0x10064D10)]
        private static int GetWorthInCopper(MoneyType moneyType)
        {
            switch (moneyType)
            {
                case MoneyType.Copper:
                    return 1;
                case MoneyType.Silver:
                    return 10;
                case MoneyType.Gold:
                    return 100;
                case MoneyType.Platinum:
                    return 1000;
                default:
                    throw new ArgumentOutOfRangeException(nameof(moneyType), moneyType, null);
            }
        }

        [TempleDllLocation(0x1002bd00)]
        public void RemoveFromAllGroups(GameObjectBody obj)
        {
            // Obj_Remove_From_Group_Array(&groupCurrentlySelected, a1);
            // Obj_Remove_From_Group_Array(&groupAiFollowers, a1);
            // Obj_Remove_From_Group_Array(&groupPCs, a1);
            // Obj_Remove_From_Group_Array(&groupNpcFollowers, a1);
            // Obj_Remove_From_Group_Array(&groupList, a1);
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1002BA40)]
        public void SaveCurrent()
        {
            // TODO
        }

        [TempleDllLocation(0x1002AEA0)]
        public void RestoreCurrent()
        {
            // TODO
        }
    }
}