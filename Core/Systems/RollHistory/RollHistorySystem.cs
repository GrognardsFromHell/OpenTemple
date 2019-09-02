using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems.RollHistory
{

    public class RollHistorySystem : IGameSystem, IResetAwareSystem
    {
        [TempleDllLocation(0x109DDA20)]
        private readonly List<HistoryEntry> _historyArray = new List<HistoryEntry>();

        [TempleDllLocation(0x102b016c)]
        private int rollSerialNumber;

        [TempleDllLocation(0x109dda18)]
        private int lastHistoryId;

        public event EventHandler<HistoryEntry> OnHistoryEvent;

        private readonly Dictionary<int, string> _translations;

        public RollHistorySystem()
        {
            Reset();
            _translations = Tig.FS.ReadMesFile("mes/roll_ui.mes");
        }

        [TempleDllLocation(0x100dff90)]
        public void Clear()
        {
            // TODO: This actually belongs to another subsystem (the roll console)
            Stub.TODO();
        }

        [TempleDllLocation(0x100dffc0)]
        public void CreateFromFreeText(string text)
        {
            // TODO: This actually belongs to another subsystem (the roll console)
            Stub.TODO();
        }

        [TempleDllLocation(0x10047430)]
        private int AddHistoryEntry(HistoryEntry entry)
        {
            // Assign a history entry serial number and time of recording
            entry.histId = ++rollSerialNumber;
            entry.recorded = TimePoint.Now;
            _historyArray.Insert(0, entry);

            if ( entry.obj != null )
            {
                entry.objId = GameSystems.Object.GetPersistableId(entry.obj);
                entry.objDescr = GameSystems.MapObject.GetDisplayNameForParty(entry.obj);
            }
            else
            {
                entry.objId = ObjectId.CreateNull();
                entry.objDescr = "";
            }

            if ( entry.obj2 != null )
            {
                entry.obj2Id = GameSystems.Object.GetPersistableId(entry.obj2);
                entry.obj2Descr = GameSystems.MapObject.GetDisplayNameForParty(entry.obj2);
            }
            else
            {
                entry.obj2Id = ObjectId.CreateNull();
                entry.obj2Descr = "";
            }

            OnHistoryEvent?.Invoke(this, entry);

            return entry.histId;
        }

        [TempleDllLocation(0x100dfff0)]
        public void CreateRollHistoryString(int histId)
        {
            // TODO: This actually belongs to another subsystem (the roll console)
            if (histId != -1)
            {
                var builder = new StringBuilder();
                FormatHistoryEntry(histId, builder);
                if (builder.Length > 0)
                {
                    Console.WriteLine(builder.ToString());
                    Stub.TODO();
                    // RollHistoryEntryCreate /*0x1010ee00*/(&D20RollHistoryConsole /*0x11868f80*/, textBuffer);
                }
            }
        }

        [TempleDllLocation(0x10047bd0)]
        public int AddAttackRoll(int attackRoll, int criticalConfirmRoll, GameObjectBody attacker, GameObjectBody defender,
            BonusList attackerBonus, BonusList defenderBonus, D20CAF flags)
        {
            var entry = new HistoryAttackRoll
            {
                rollRes = attackRoll,
                critRollRes = criticalConfirmRoll,
                obj = attacker,
                obj2 = defender,
                bonlist = attackerBonus,
                defenderRollId = AddMiscBonus(defender, defenderBonus, 33, 0),
                d20Caf = flags,
                defenderOverallBonus = defenderBonus.OverallBonus
            };
            return AddHistoryEntry(entry);
        }

        [TempleDllLocation(0x10047C80)]
        public int AddDamageRoll(GameObjectBody attacker, GameObjectBody victim, DamagePacket damPkt)
        {
            var entry = new HistoryDamageRoll(damPkt); // TODO: We should actually make a *copy* of the packet here
            entry.obj = attacker;
            entry.obj2 = victim;

            return AddHistoryEntry(entry);
        }

        [TempleDllLocation(0x10047CF0)]
        public int AddSkillCheck(GameObjectBody objHnd, GameObjectBody objHnd2, SkillId skillIdx, Dice dice,
            int rollResult,
            int dc,
            in BonusList bonlist)
        {
            var entry = new HistorySkillCheck();
            entry.obj = objHnd;
            entry.obj2 = objHnd2;
            entry.dice = dice;
            entry.rollResult = rollResult;
            entry.skillIdx = skillIdx;
            entry.dc = dc;
            entry.bonlist = bonlist;

            return AddHistoryEntry(entry);
        }

        [TempleDllLocation(0x10047D90)]
        public int AddSavingThrow(GameObjectBody obj, int dc, SavingThrowType saveType, D20SavingThrowFlag flags,
            Dice dice, int rollResult, in BonusList bonListIn)
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10047e30)]
        public int AddMiscCheck(GameObjectBody obj, int dc, string historyText, Dice dice, int rollResult,
            BonusList bonusList)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10047ec0)]
        public int AddPercentageCheck(GameObjectBody obj, GameObjectBody tgt, int failChance,
            int combatMesFailureReason, int rollResult, int combatMesResult, int combatMesTitle)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10047F70)]
        public int AddOpposedCheck(GameObjectBody performer, GameObjectBody opponent,
            int roll, int opposingRoll,
            in BonusList bonus, in BonusList opposingBonus,
            int combatMesLineTitle, D20CombatMessage combatMesLineResult, int flag)
        {
            throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x100475f0)]
        public int AddMiscBonus(GameObjectBody critter, BonusList bonList, int line, int rollResult)
        {
            var entry = new HistoryMiscBonus(bonList, line, rollResult)
            {
                obj = critter
            };
            return AddHistoryEntry(entry);
        }

        [TempleDllLocation(0x100E01F0)]
        public int CreateRollHistoryLineFromMesfile(int historyMesLine, GameObjectBody obj, GameObjectBody obj2)
        {
            throw new System.NotImplementedException();
        }

        public string GetTranslation(int key)
        {
            return _translations[key];
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10048960)]
        [TemplePlusLocation("history.cpp:205")]
        private void FormatHistoryEntry(int histId, StringBuilder builder)
        {
            var entry = FindEntry(histId);
            if (entry != null)
            {
                entry.PrintToConsole(builder);
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }
            }
        }

        private HistoryEntry FindEntry(int histId)
        {
            HistoryEntry entry = null;
            foreach (var arrayEntry in _historyArray)
            {
                if (arrayEntry.histId == histId)
                {
                    entry = arrayEntry;
                    break;
                }
            }

            return entry;
        }

        [TempleDllLocation(0x10047160)]
        public void Reset()
        {
            _historyArray.Clear();
            rollSerialNumber = 0;
            lastHistoryId = 0;
        }
    }
}