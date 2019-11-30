using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.MapSector;
using SpicyTemple.Core.Time;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Systems
{
    public enum ObjectEventShape
    {
        Cone,
        Circle,
        Wall
    }

    public class ObjectEvent
    {
        private const int OBJ_EVENT_WALL_ENTERED_HANDLER_ID = 50;

        private const int OBJ_EVENT_WALL_EXITED_HANDLER_ID = 51;

        public SectorLoc SectorLoc { get; }

        public GameObjectBody SourceObject { get; }

        public ObjectEventShape Shape { get; }

        public int EnterCallbackId { get; }

        public int LeaveCallbackId { get; }

        public ObjectListFilter ObjListFlags { get; }

        public float RadiusInch { get; }

        public float ConeAngleStart { get; }

        public float ConeRadians { get; }

        public List<GameObjectBody> PreviouslyAffected { get; } = new List<GameObjectBody>();

        public ObjectEvent(SectorLoc sectorLoc, GameObjectBody sourceObject, int enterCallbackId, int leaveCallbackId,
            ObjectListFilter objListFlags, float radiusInch, float coneAngleStart, float coneRadians)
        {
            Trace.Assert(sourceObject != null);
            SectorLoc = sectorLoc;
            SourceObject = sourceObject;
            EnterCallbackId = enterCallbackId;
            LeaveCallbackId = leaveCallbackId;
            ObjListFlags = objListFlags;
            RadiusInch = radiusInch;
            ConeAngleStart = coneAngleStart;
            ConeRadians = coneRadians;

            if (EnterCallbackId == OBJ_EVENT_WALL_ENTERED_HANDLER_ID
                && LeaveCallbackId == OBJ_EVENT_WALL_EXITED_HANDLER_ID)
            {
                Shape = ObjectEventShape.Wall;
            }
            else if (Math.Abs(coneAngleStart) < 0.01 && Math.Abs(coneRadians - Angles.ToRadians(360)) < 0.01)
            {
                Shape = ObjectEventShape.Circle;
            }
            else
            {
                Shape = ObjectEventShape.Cone;
            }
        }

        private LineSegment GetWallLineSegment()
        {
            Trace.Assert(Shape == ObjectEventShape.Wall);
            // use startPt + angleMin to find endPt
            var vectorAngle = 5 * MathF.PI / 4 - ConeAngleStart;
            var dir = new Vector2(MathF.Cos(vectorAngle), MathF.Sin(vectorAngle));

            var from = SourceObject.GetLocationFull().ToInches2D();
            var to = from + dir * RadiusInch;
            return new LineSegment(from, to);
        }

        public IEnumerable<GameObjectBody> EnumerateObjectsInEffect()
        {
            var currentPos = SourceObject.GetLocationFull();

            switch (Shape)
            {
                case ObjectEventShape.Cone:
                    throw new InvalidOperationException("Cone is currently not supported.");
                case ObjectEventShape.Circle:
                {
                    using var objList = ObjList.ListRadius(currentPos, RadiusInch, ObjListFlags);
                    foreach (var obj in objList)
                    {
                        yield return obj;
                    }
                }
                    break;
                case ObjectEventShape.Wall:
                {
                    using var objList = ObjList.ListRadius(currentPos, RadiusInch, ObjListFlags);
                    foreach (var obj in objList)
                    {
                        yield return obj;
                    }
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool IsInAreaOfEffect(Vector2 loc, float objRadius)
        {
            if (Shape == ObjectEventShape.Wall)
            {
                // Treat the wall as a line, and calculate the distance between that
                // line and loc.
                var lineSegment = GetWallLineSegment();

                // We are assuming 5 feet (one square) wall width, so half of that on either side of the line
                var width = 5 * locXY.INCH_PER_FEET / 2;

                var distanceSquared = lineSegment.GetDistanceFromLineSquared(loc);

                return distanceSquared - width < objRadius;
            }
            else if (Shape == ObjectEventShape.Circle)
            {
                var effectCenterPos = SourceObject.GetLocationFull().ToInches2D();
                var distanceSquared = (effectCenterPos - loc).LengthSquared();

                var maxDistance = RadiusInch + objRadius;
                maxDistance *= maxDistance;

                return distanceSquared < maxDistance;
            }

            throw new InvalidOperationException("Cone is currently not supported.");
        }
    }

    public class ObjectEventSystem : IGameSystem, ISaveGameAwareGameSystem, IResetAwareSystem, ITimeAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly Dictionary<int, ObjectEvent> _objectEvents = new Dictionary<int, ObjectEvent>();

        // Used to track all movement that happened until AdvanceTime processes it
        private readonly List<PendingObjectMovement> _movementQueue = new List<PendingObjectMovement>();

        private bool _processingPendingMovement;

        private int _nextId = 1;

        [TempleDllLocation(0x10045110)]
        public ObjectEventSystem()
        {
        }

        [TempleDllLocation(0x10045140)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10045160)]
        public void Reset()
        {
            _movementQueue.Clear();
            _objectEvents.Clear();
            _nextId = 1;
        }

        [TempleDllLocation(0x100456d0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            var savedEvents = new List<SavedObjectEvent>(_objectEvents.Count);
            foreach (var (id, objEvent) in _objectEvents)
            {
                savedEvents.Add(new SavedObjectEvent
                {
                    Id = id,
                    SectorLoc = objEvent.SectorLoc,
                    SourceObjectId = objEvent.SourceObject.id,
                    EnterCallbackId = objEvent.EnterCallbackId,
                    LeaveCallbackId = objEvent.LeaveCallbackId,
                    ObjListFlags = objEvent.ObjListFlags,
                    RadiusInch = objEvent.RadiusInch,
                    ConeAngleStart = objEvent.ConeAngleStart,
                    ConeRadians = objEvent.ConeRadians
                });
            }

            savedGameState.ObjectEventState = new SavedObjectEventState
            {
                NextObjectEventId = _nextId,
                Events = savedEvents
            };
        }

        [TempleDllLocation(0x100451b0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            _objectEvents.Clear();

            var savedEvents = savedGameState.ObjectEventState;
            _nextId = savedEvents.NextObjectEventId;

            foreach (var savedEvent in savedEvents.Events)
            {
                var sourceObject = GameSystems.Object.GetObject(savedEvent.SourceObjectId);
                if (sourceObject == null)
                {
                    throw new CorruptSaveException(
                        $"Couldn't restore source of object event {savedEvent.SourceObjectId}");
                }

                _objectEvents[savedEvent.Id] = new ObjectEvent(
                    savedEvent.SectorLoc,
                    sourceObject,
                    savedEvent.EnterCallbackId,
                    savedEvent.LeaveCallbackId,
                    savedEvent.ObjListFlags,
                    savedEvent.RadiusInch,
                    savedEvent.ConeAngleStart,
                    savedEvent.ConeRadians
                );
            }
        }

        [TempleDllLocation(0x10045740)]
        public void AdvanceTime(TimePoint time)
        {
            PruneDuplicateMovement();

            foreach (var kvp in _objectEvents)
            {
                UpdateEffectSources(kvp.Value);
            }

            _processingPendingMovement = true;
            foreach (var pendingMovement in _movementQueue)
            {
                foreach (var kvp in _objectEvents)
                {
                    ObjEventHandler(kvp.Value, kvp.Key, pendingMovement);
                }
            }

            _processingPendingMovement = false;
        }

        [TempleDllLocation(0x100c3310)]
        private void OnLeaveAreaOfEffect(GameObjectBody effectSource, GameObjectBody obj, int eventId)
        {
            if (obj.IsCritter())
            {
                var dispIo = DispIoObjEvent.Default;
                dispIo.aoeObj = effectSource;
                dispIo.tgt = obj;
                dispIo.evtId = eventId;

                obj.GetDispatcher()?.Process(DispatcherType.ObjectEvent, D20DispatcherKey.OnLeaveAoE, dispIo);
            }
        }

        [TempleDllLocation(0x100c3290)]
        private void OnEnterAreaOfEffect(GameObjectBody effectSource, GameObjectBody obj, int eventId)
        {
            if (obj.IsCritter())
            {
                var dispIo = DispIoObjEvent.Default;
                dispIo.aoeObj = effectSource;
                dispIo.tgt = obj;
                dispIo.evtId = eventId;

                obj.GetDispatcher()?.Process(DispatcherType.ObjectEvent, D20DispatcherKey.OnEnterAoE, dispIo);
            }
        }

        private void ObjEventHandler(ObjectEvent aoeEvt, int id, PendingObjectMovement evt)
        {
            if (aoeEvt.SourceObject != evt.Obj)
            {
                // find the event obj in the aoeEvt list of previously appearing ObjectHandles. to determine leaving / entering status
                var foundInNodes = aoeEvt.PreviouslyAffected.Contains(evt.Obj);

                var objRadius = evt.Obj.GetRadius();
                bool isInAreaOfEffect = aoeEvt.IsInAreaOfEffect(evt.To.ToInches2D(), objRadius);

                if (foundInNodes)
                {
                    if (!isInAreaOfEffect)
                    {
                        aoeEvt.PreviouslyAffected.Remove(evt.Obj);
                        OnLeaveAreaOfEffect(aoeEvt.SourceObject, evt.Obj, id);
                        return;
                    }
                }
                else if (isInAreaOfEffect)
                {
                    aoeEvt.PreviouslyAffected.Add(evt.Obj);
                    OnEnterAreaOfEffect(aoeEvt.SourceObject, evt.Obj, id);
                }

                return;
            }

            // The effect's source has moved. Update the list of affected objects.
            Span<bool> visitedObj = stackalloc bool[aoeEvt.PreviouslyAffected.Count];
            foreach (var objNowInEffect in aoeEvt.EnumerateObjectsInEffect())
            {
                var idx = aoeEvt.PreviouslyAffected.IndexOf(objNowInEffect);
                if (idx == -1)
                {
                    OnEnterAreaOfEffect(aoeEvt.SourceObject, objNowInEffect, id);
                    aoeEvt.PreviouslyAffected.Add(objNowInEffect);
                }
                else
                {
                    visitedObj[idx] = true;
                }
            }

            // Remove and notify about leaving the AOE any object that we didn't just visit
            for (int i = visitedObj.Length - 1; i >= 0; i--)
            {
                if (!visitedObj[i])
                {
                    var obj = aoeEvt.PreviouslyAffected[i];
                    aoeEvt.PreviouslyAffected.RemoveAt(i);
                    OnLeaveAreaOfEffect(aoeEvt.SourceObject, obj, id);
                }
            }
        }

        private void UpdateEffectSources(ObjectEvent evt)
        {
            // This may happen if an effect source is removed from the world
            // TODO: This should be changed to an event-based approach (i.e. OnRemoveFromWorld callback)
            var obj = evt.SourceObject;
            if (GameSystems.Object.IsValidHandle(obj))
            {
                Logger.Warn("Event source removed without being removed from event system: {0}", obj.id);
            }
        }

        /// <summary>
        /// Remove any superflous queued movement entries for objects that moved multiple times last frame.
        /// </summary>
        private void PruneDuplicateMovement()
        {
            var i = 0;
            while (i < _movementQueue.Count)
            {
                var obj = _movementQueue[i].Obj;
                bool found = false;
                for (var j = i + 1; j < _movementQueue.Count; j++)
                {
                    if (_movementQueue[j].Obj == obj)
                    {
                        found = true;
                        break;
                    }
                }

                if (found)
                {
                    _movementQueue.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }

        [TempleDllLocation(0x10045290)]
        public void NotifyMoved(GameObjectBody obj, LocAndOffsets fromLoc, LocAndOffsets toLoc)
        {
            if (_processingPendingMovement)
            {
                return;
            }

            Trace.Assert(obj != null);
            _movementQueue.Add(new PendingObjectMovement(obj, fromLoc, toLoc));
        }

        [TempleDllLocation(0x10044AE0)]
        private int GetFreeId()
        {
            while (_objectEvents.ContainsKey(_nextId))
            {
                _nextId++;
            }

            return _nextId;
        }

        public int AddEvent(GameObjectBody aoeObj, int onEnterFuncIdx, int onLeaveFuncIdx, ObjectListFilter olcFilter,
            float radiusInch)
        {
            return AddEvent(aoeObj, onEnterFuncIdx, onLeaveFuncIdx, olcFilter,
                radiusInch, 0, 2 * MathF.PI);
        }

        [TempleDllLocation(0x10045580)]
        public int AddEvent(GameObjectBody aoeObj, int onEnterFuncIdx, int onLeaveFuncIdx, ObjectListFilter olcFilter,
            float radiusInch, float angleBase, float angleSize)
        {
            if (aoeObj == null)
            {
                Logger.Warn("ObjectEventAppend: Null aoeObj!");
                return 0;
            }

            var objLoc = aoeObj.GetLocationFull();
            var secLoc = new SectorLoc(objLoc.location);

            var evt = new ObjectEvent(
                secLoc,
                aoeObj,
                onEnterFuncIdx,
                onLeaveFuncIdx,
                olcFilter,
                radiusInch,
                angleBase,
                angleSize
            );

            var id = GetFreeId();
            _objectEvents[id] = evt;

            NotifyMoved(aoeObj, LocAndOffsets.Zero, objLoc);

            return id;
        }

        [TempleDllLocation(0x10044a10)]
        public void Remove(int eventId)
        {
            _objectEvents.Remove(eventId);
        }

        public void FlushEvents()
        {
            Reset();
        }

        private readonly struct PendingObjectMovement
        {
            public readonly GameObjectBody Obj;
            public readonly LocAndOffsets From;
            public readonly LocAndOffsets To;

            public PendingObjectMovement(GameObjectBody obj, LocAndOffsets from, LocAndOffsets to)
            {
                Obj = obj;
                From = from;
                To = to;
            }
        }
    }
}