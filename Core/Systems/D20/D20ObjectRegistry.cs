using System;
using System.Collections.Generic;
using System.Resources;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Systems.D20
{
    public class D20ObjectRegistry : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        [TempleDllLocation(0x10BCAD94)] [TempleDllLocation(0x10BCAD98)]
        private readonly List<GameObjectBody> _objects = new List<GameObjectBody>();

        public IEnumerable<GameObjectBody> Objects => _objects;

        [TempleDllLocation(0x100dfa30)]
        public D20ObjectRegistry()
        {
        }

        [TempleDllLocation(0x100dfa80)]
        public void Dispose()
        {
            Clear();
        }

        public void Clear()
        {
            for (var i = _objects.Count - 1; i >= 0; i--)
            {
                // Note that remove dispatcher will also remove it from this registry, hence the reverse iteration
                GameSystems.D20.RemoveDispatcher(_objects[i]);
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

        public void SendSignalAll(D20DispatcherKey signal, object objectArg)
        {
            foreach (var obj in _objects)
            {
                GameSystems.D20.D20SendSignal(obj, signal, objectArg);
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
            if (!GameSystems.Combat.IsCombatActive())
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
            SendSignalAll(
                D20DispatcherKey.SIG_Initiative_Update,
                nextInitiative,
                currentInitiative
            );

            var someoneStartedTurn = false;
            foreach (var obj in _objects)
            {
                // The following is only used for out of combat initiative updates and for any non-critter in combat
                // since non-critters don't participate in combat.
                if (GameSystems.Combat.IsCombatActive() && obj.IsCritter())
                {
                    continue;
                }

                var objInitiative = GameSystems.D20.Initiative.GetInitiative(obj);

                // Keep in mind the initiative is counting DOWN!
                // So if the new initiative is higher than the old initiative, it wrapped around!
                if ( currentInitiative >= nextInitiative )
                {
                    if ( objInitiative >= currentInitiative || objInitiative < nextInitiative )
                    {
                        continue;
                    }
                }
                else
                {
                    // Initiative has wrapped around
                    if ( objInitiative >= currentInitiative && objInitiative < nextInitiative )
                    {
                        continue;
                    }
                }

                GameSystems.D20.Initiative.DispatchInitiative(obj);
                GameSystems.Combat.DispatchBeginRound(obj, 1);
                someoneStartedTurn = true;
            }

            if (someoneStartedTurn)
            {
                GameUiBridge.LogbookNextTurn();
            }

        }

    }
}