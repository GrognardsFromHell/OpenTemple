using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Time;

namespace SpicyTemple.Core.Ui
{

    internal class KeylogEntry
    {
        public TimePoint Acquired { get; set; }
        public TimePoint Used { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public bool IsAcquired => Acquired != default;
    }

    public class LogbookUi
    {
        [TempleDllLocation(0x101260f0)]
        public bool IsVisible { get; }

        [TempleDllLocation(0x10c4c698)]
        private Dictionary<int, KeylogEntry> _keys = new Dictionary<int,KeylogEntry>();

        [TempleDllLocation(0x10198290)]
        public void MarkKeyUsed(int keyId, TimePoint timeUsed)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101982c0)]
        public bool IsKeyAcquired(int keyId)
        {
            if (!_keys.TryGetValue(keyId, out var key))
            {
                return false;
            }

            return key.IsAcquired;

        }

        [TempleDllLocation(0x101d4ca0)]
        public void RecordKill(GameObjectBody killer, GameObjectBody killed)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101d0070)]
        public void RecordCriticalHit(GameObjectBody attacker)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10D249F8)]
        private int _turnCounter = 0;

        [TempleDllLocation(0x101d0040)]
        public void RecordCombatStart()
        {
            _turnCounter = 1;
        }

        [TempleDllLocation(0x101d0050)]
        public void RecordNewTurn()
        {
            if (_turnCounter > 0)
            {
                _turnCounter++;
            }
        }

        [TempleDllLocation(0x10126030)]
        public void Hide()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10128310)]
        public void Show()
        {
            throw new NotImplementedException();
        }
    }
}