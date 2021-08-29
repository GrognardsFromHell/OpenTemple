using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Actions;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.Utils;

namespace OpenTemple.Tests.TestUtils
{
    [Category("NeedsRealFiles")]
    [NonParallelizable]
    public abstract class HeadlessGameTest
    {
        private static HeadlessGameHelper _game;

        public HeadlessGameHelper Game => _game;

        protected List<D20Action> ActionLog { get; } = new();

        protected IEnumerable<D20Action> CompletedActions =>
            ActionLog.Where(a => a.d20Caf.HasFlag(D20CAF.ACTIONFRAME_PROCESSED));

        protected List<HistoryEntry> CombatLog { get; } = new();

        private record InitiativeOverride(int Initiative);

        private readonly ConditionalWeakTable<GameObjectBody, InitiativeOverride> _initiativeOverrides = new();

        protected void SetInitiative(GameObjectBody obj, int initiative)
        {
            _initiativeOverrides.Add(obj, new InitiativeOverride(initiative));
        }

        [OneTimeSetUp]
        public void StartGame()
        {
            Trace.Assert(_game == null);
            _game = new HeadlessGameHelper();

            // Record all actions as they're being performed
            GameSystems.D20.Actions.OnActionStarted += action =>
            {
                ActionLog.Add(action.Copy());
            };
            GameSystems.D20.Actions.OnActionEnded += action =>
            {
                ActionLog.Add(action.Copy());
            };

            // Record any combat log events
            GameSystems.RollHistory.OnHistoryEvent += entry =>
            {
                var str = new StringBuilder();
                entry.FormatShort(str);
                CombatLog.Add(entry);
                TestContext.Out.WriteLine(str);
            };

            // Hook into D20 initiative to allow a forced turn order
            GameSystems.D20.Initiative.InitiativeOverride += obj =>
            {
                if (_initiativeOverrides.TryGetValue(obj, out var value))
                {
                    return value!.Initiative;
                }

                return Dice.D20.Roll() + GameSystems.D20.Initiative.GetInitiativeBonus(obj, out _);
            };
        }

        [OneTimeTearDown]
        public void StopGame()
        {
            try
            {
                _game?.Dispose();
            }
            finally
            {
                _game = null;
            }

            // Run all pending finalizers, because sometimes these will crash and
            // if it happens later, it will be harder to associate them with the test
            // that actually caused the problem
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [SetUp]
        public void ClearLogs()
        {
            _initiativeOverrides.Clear();
            CombatLog.Clear();
            ActionLog.Clear();

            // Clear any text floaters or similar stuff that might be left-over from before
            GameSystems.TextFloater.RemoveAll();
        }

        [TearDown]
        public void TakeScreenshotOfFailure()
        {
        }
    }
}