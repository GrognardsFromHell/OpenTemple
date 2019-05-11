using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20ObjectRegistry : IDisposable
    {

        private readonly List<GameObjectBody> _objects = new List<GameObjectBody>();

        [TempleDllLocation(0x100dfa30)]
        public D20ObjectRegistry()
        {
        }

        [TempleDllLocation(0x100dfa80)]
        public void Dispose()
        {
            foreach (var obj in _objects)
            {
                GameSystems.D20.RemoveDispatcher(obj);
            }
            _objects.Clear();
        }

        [TempleDllLocation(0x100dfdf0)]
        public void SendSignalAll(D20DispatcherKey signal, int arg1 = 0, int arg2 = 0)
        {
            foreach (var obj in _objects)
            {
                GameSystems.D20.D20SendSignal(obj, signal, arg1, arg2);
            }
        }

        [TempleDllLocation(0x100dfad0)]
        public void Add(GameObjectBody obj)
        {
            if (!_objects.Contains(obj))
            {
                _objects.Add(obj);
            }
        }

        [TempleDllLocation(0x100dfb00)]
        public bool Contains(GameObjectBody obj)
        {
            return _objects.Contains(obj);
        }

        [TempleDllLocation(0x100dfb50)]
        public void Remove(GameObjectBody obj)
        {
            _objects.Remove(obj);
        }

        [TempleDllLocation(0x100dfda0)]
        public void BeginRoundForAll(int numRounds)
        {
            if ( !GameSystems.Combat.IsCombatActive() )
            {
                foreach (var obj in _objects)
                {
                    GameSystems.Combat.DispatchBeginRound(obj, numRounds);
                }
            }
        }

        [TempleDllLocation(0x100dfe40)]
        public void OnInitiativeTransition(int currentInitiative, int nextInitiative)
        {
            Stub.TODO();
        }
    }
}