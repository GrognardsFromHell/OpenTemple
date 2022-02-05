using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using OpenTemple.Core.Utils;

#nullable enable

namespace OpenTemple.Core.Systems.D20;

public class D20Initiative : IDisposable, IReadOnlyList<GameObject>
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    /// <summary>
    /// This is for tests that want to override the initiative to ensure a specific turn order.
    /// </summary>
    public Func<GameObject, int>? InitiativeOverride { get; set; }

    [TempleDllLocation(0x10BCAD90)]
    private bool _surpriseRound;

    [TempleDllLocation(0x10BCAC78)]
    private readonly CritterGroup _initiativeOrder = new();

    [TempleDllLocation(0x10BCAD88)]
    private GameObject? _currentActor;

    [TempleDllLocation(0x100dec60)]
    public void OnExitCombat()
    {
        // The list can change while we iterate...
        var copiedList = new List<GameObject>(_initiativeOrder);
        foreach (var member in copiedList)
        {
            GameSystems.Script.ExecuteObjectScript(member, member, ObjScriptEvent.ExitCombat);
        }
    }

    [TempleDllLocation(0x100ded90)]
    public void Sort()
    {
        _initiativeOrder.Sort();
    }

    public void Dispose()
    {
    }

    public void Reset()
    {
        _initiativeOrder.Clear();
    }

    [TempleDllLocation(0x100dedb0)]
    public int GetInitiative(GameObject obj)
    {
        return obj.GetInt32(obj_f.initiative);
    }

    [TempleDllLocation(0x100dedd0)]
    public bool Contains(GameObject obj)
    {
        return _initiativeOrder.Contains(obj);
    }

    public GameObject? CurrentActor
    {
        [TempleDllLocation(0x100dee40)]
        get => _currentActor;

        [TempleDllLocation(0x100dee10)]
        set => _currentActor = value;
    }

    [TempleDllLocation(0x100dee50)]
    public int CurrentActorIndex => _initiativeOrder.IndexOf(_currentActor);

    public int IndexOf(GameObject obj) => _initiativeOrder.IndexOf(obj);

    /// <summary>
    /// Sorts by initiative in descending order.
    /// </summary>
    private class InitiativeComparer : IComparer<GameObject>
    {
        [TempleDllLocation(0x100def20)]
        public int Compare(GameObject? x, GameObject? y)
        {
            var xInit = x?.GetInt32(obj_f.initiative) ?? int.MinValue;
            var yInit = y?.GetInt32(obj_f.initiative) ?? int.MinValue;

            if (xInit != yInit)
            {
                return yInit.CompareTo(xInit);
            }

            var xSubinit = x?.GetInt32(obj_f.subinitiative) ?? int.MinValue;
            var ySubinit = y?.GetInt32(obj_f.subinitiative) ?? int.MinValue;

            return ySubinit.CompareTo(xSubinit);
        }
    }

    [TempleDllLocation(0x100defa0)]
    private void ArbitrateConflicts()
    {
        for (int i = 0; i < _initiativeOrder.Count - 1; i++)
        {
            var combatant = _initiativeOrder[i];
            var initiative = combatant.GetInt32(obj_f.initiative);
            var subinitiative = combatant.GetInt32(obj_f.subinitiative);

            for (int j = i + 1; j < _initiativeOrder.Count; j++)
            {
                var combatant2 = _initiativeOrder[j];
                var initiative2 = combatant2.GetInt32(obj_f.initiative);
                var subinitiative2 = combatant2.GetInt32(obj_f.subinitiative);
                if (initiative != initiative2)
                    break;
                if (subinitiative != subinitiative2)
                    break;

                combatant2.SetInt32(obj_f.subinitiative, subinitiative2 - 1);
            }
        }
    }

    [TempleDllLocation(0x100df080)]
    private void MakeUnique(int initiative, int subinitiative)
    {
        foreach (var actor in _initiativeOrder)
        {
            var memberInitiative = actor.GetInt32(obj_f.initiative);
            var memberSubinitiative = actor.GetInt32(obj_f.subinitiative);
            if (initiative == memberInitiative && subinitiative == memberSubinitiative)
            {
                actor.SetInt32(obj_f.subinitiative, memberSubinitiative - 1);
            }
        }
    }

    [TempleDllLocation(0x100ded20)]
    public void Save(SavedD20State savedState)
    {
        savedState.TurnOrder = _initiativeOrder.Select(o => o.id).ToArray();
        savedState.CurrentTurnIndex = _initiativeOrder.IndexOf(_currentActor);
    }

    [TempleDllLocation(0x100df100)]
    public void Load(SavedD20State savedState)
    {
        _initiativeOrder.Clear();
        foreach (var objectId in savedState.TurnOrder)
        {
            var obj = GameSystems.Object.GetObject(objectId);
            if (obj == null)
            {
                throw new CorruptSaveException($"Failed to restore turn order for object {objectId}");
            }

            _initiativeOrder.Add(obj);
        }
        _initiativeOrder.Comparer = new InitiativeComparer();

        var actorIdx = savedState.CurrentTurnIndex;
        if (actorIdx == -1)
        {
            _currentActor = null;
        }
        else
        {
            _currentActor = _initiativeOrder[actorIdx];
        }
    }

    [TempleDllLocation(0x100df190)]
    public void SurpriseRound()
    {
        _surpriseRound = true;

        foreach (var member in _initiativeOrder)
        {
            Stub.TODO();
        }
    }

    [TempleDllLocation(0x100df1e0)]
    public void AddToInitiative(GameObject obj)
    {
        if (Contains(obj))
        {
            return;
        }

        if (obj.HasFlag(ObjectFlag.DESTROYED) || obj.HasFlag(ObjectFlag.OFF))
        {
            return;
        }

        if (!GameSystems.D20.D20Query(obj, D20DispatcherKey.QUE_EnterCombat) )
        {
            return;
        }

        GameSystems.AI.OnAddToInitiative(obj);

        obj.SetInt32(obj_f.initiative, RollInitiative(obj));

        _initiativeOrder.Add(obj);

        // Dexterity is used as the tie-breaker
        var dexterity = GameSystems.Stat.StatLevelGet(obj, Stat.dexterity);
        obj.SetInt32(obj_f.subinitiative, 100 * dexterity);
        ArbitrateConflicts();

        // Add the flatfooted condition as long as the critter has not acted
        obj.AddCondition(BuiltInConditions.Flatfooted);

        // Add the surprise round condition
        if (_surpriseRound)
        {
            obj.AddCondition(BuiltInConditions.Surprised);
        }
    }

    private int RollInitiative(GameObject obj)
    {
        int initiative;
        if (InitiativeOverride != null)
        {
            initiative = InitiativeOverride(obj);
        }
        else
        {
            var initiativeRoll = Dice.D20.Roll();
            var initiativeBonus = GetInitiativeBonus(obj, out _);
            initiative = initiativeRoll + initiativeBonus;
        }

        Logger.Info("Rolled initiative {0} for {1}", initiative, obj);
        return initiative;
    }

    [TempleDllLocation(0x100df2b0)]
    public void RewindCurrentActor()
    {
        if (_initiativeOrder.Count == 0)
        {
            CurrentActor = null;
        }
        else
        {
            CurrentActor = _initiativeOrder[0];
        }
    }

    [TempleDllLocation(0x100df310)]
    public void NextActor()
    {
        if (_currentActor == null)
        {
            return;
        }

        var currentInitiative = GetInitiative(_currentActor);
        var nextActorIdx = CurrentActorIndex + 1;

        if (nextActorIdx >= _initiativeOrder.Count)
        {
            // Initiate the next round
            _surpriseRound = false;

            foreach (var member in _initiativeOrder)
            {
                DispatchInitiative(member);
            }

            nextActorIdx = 0;
            GameSystems.D20.ObjectRegistry.OnInitiativeTransition(currentInitiative, 0);
            currentInitiative = 0;
        }

        _currentActor = _initiativeOrder[nextActorIdx];

        if (_currentActor != null)
        {
            var nextInitiative = _currentActor.GetInt32(obj_f.initiative);
            if (currentInitiative != nextInitiative)
            {
                GameSystems.D20.ObjectRegistry.OnInitiativeTransition(currentInitiative, nextInitiative);
            }
        }
    }

    [TempleDllLocation(0x1004cef0)]
    public void DispatchInitiative(GameObject obj)
    {
        var dispatcher = obj.GetDispatcher();
        dispatcher?.Process(DispatcherType.Initiative, D20DispatcherKey.NONE, null);
    }

    [TempleDllLocation(0x100df2e0)]
    public void SetInitiative(GameObject obj, int initiative)
    {
        obj.SetInt32(obj_f.initiative, initiative);
        ArbitrateConflicts();
        _initiativeOrder.Sort();
    }

    /// <summary>
    /// Sets the initiative of the given object to be just before another.
    /// </summary>
    [TempleDllLocation(0x100df350)]
    public void SetInitiativeBefore(GameObject obj, GameObject objBefore)
    {
        var targetInitiative = GetInitiative(objBefore);
        obj.SetInt32(obj_f.initiative, targetInitiative);
        ArbitrateConflicts();
        _initiativeOrder.Sort();
        var targetSubinitiative = objBefore.GetInt32(obj_f.subinitiative) - 1;
        MakeUnique(targetInitiative, targetSubinitiative);
        obj.SetInt32(obj_f.subinitiative, targetSubinitiative);
        ArbitrateConflicts();
        _initiativeOrder.Sort();
    }

    /// <summary>
    /// Sets the initiative of the given object to be just after another.
    /// </summary>
    [TempleDllLocation(0x100df3d0)]
    [TempleDllLocation(0x100df570)]
    public void SetInitiativeTo(GameObject obj, GameObject targetObj)
    {
        var targetInitiative = GetInitiative(targetObj);
        obj.SetInt32(obj_f.initiative, targetInitiative);
        ArbitrateConflicts();
        _initiativeOrder.Sort();
        var targetSubinitiative = targetObj.GetInt32(obj_f.subinitiative);
        MakeUnique(targetInitiative, targetSubinitiative);
        obj.SetInt32(obj_f.subinitiative, targetSubinitiative);
        ArbitrateConflicts();
        _initiativeOrder.Sort();
    }

    [TempleDllLocation(0x100df450)]
    public void CreateForParty()
    {
        _initiativeOrder.Clear();
        _initiativeOrder.Comparer = new InitiativeComparer();

        if (GameUiBridge.IsTutorialActive() && GameSystems.Script.GetGlobalFlag(4))
        {
            GameUiBridge.ShowTutorialTopic(TutorialTopic.CombatInitiativeBar);
        }

        foreach (var member in GameSystems.Party.PartyMembers)
        {
            if (GameSystems.D20.D20Query(member, D20DispatcherKey.QUE_EnterCombat) )
            {
                AddToInitiative(member);
            }
        }

        RewindCurrentActor();
    }

    [TempleDllLocation(0x100df500)]
    public void AddSurprisedCondition(GameObject obj)
    {
        Stub.TODO();
        AddToInitiative(obj);
        // ConditionAdd___byObjHnd_0_args(ObjHnd, &Condition_SurpriseRound);
    }

    [TempleDllLocation(0x100df530)]
    public void RemoveFromInitiative(GameObject obj)
    {
        // We cannot immediately remove the object because it still has to be in the initiative list
        // in order to find the next actor.
        if (obj == _currentActor && _initiativeOrder.Count > 0)
        {
            NextActor();
            // If we're still the current actor, we're likely the only one in the initiative order
            if (_currentActor == obj)
            {
                _currentActor = null;
            }
        }

        _initiativeOrder.Remove(obj);
    }

    public int GetInitiativeBonus(GameObject obj, out BonusList bonusList)
    {
        var dispatcher = obj.GetDispatcher();
        if (dispatcher == null)
        {
            bonusList = default;
            return 0;
        }

        var bonusIo = DispIoObjBonus.Default;
        dispatcher.Process(DispatcherType.InitiativeMod, D20DispatcherKey.NONE, bonusIo);
        bonusList = bonusIo.bonlist;

        return bonusIo.bonlist.OverallBonus;
    }

    [TempleDllLocation(0x100df5a0)]
    public void Move(GameObject obj, int toIndex)
    {
        var currentIndex = _initiativeOrder.IndexOf(obj);

        if (toIndex < currentIndex)
        {
            SetInitiativeTo(obj, _initiativeOrder[toIndex]);
        }
        else if (toIndex > currentIndex)
        {
            SetInitiativeBefore(obj, _initiativeOrder[toIndex]);
        }
        else if (toIndex == currentIndex)
        {
            return;
        }

        NextActor();
    }

    [TempleDllLocation(0x100dedf0)]
    public IEnumerator<GameObject> GetEnumerator()
    {
        return _initiativeOrder.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _initiativeOrder).GetEnumerator();
    }

    [TempleDllLocation(0x100deda0)]
    public int Count => _initiativeOrder.Count;

    public GameObject this[int index] => _initiativeOrder[index];
}