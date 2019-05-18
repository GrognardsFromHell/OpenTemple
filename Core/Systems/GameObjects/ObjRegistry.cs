using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Logging;

namespace SpicyTemple.Core.Systems.GameObjects
{
    internal class ObjRegistry : IEnumerable<KeyValuePair<ObjHndl, GameObjectBody>>
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        public ObjRegistry()
        {
            _objects = new Dictionary<ObjHndl, GameObjectBody>(8192);
            _objectIndex = new Dictionary<ObjectId, ObjHndl>();
        }

        public ObjectId GetIdByHandle(ObjHndl handle)
        {
            var obj = Get(handle);

            if (obj == null)
            {
                return ObjectId.CreateNull();
            }

            return obj.id;
        }

        public ObjHndl GetHandleById(ObjectId id)
        {
            if (_objectIndex.TryGetValue(id, out var handle))
            {
                return handle;
            }

            return ObjHndl.Null;
        }

        public void AddToIndex(ObjHndl handle, ObjectId objectId)
        {
            _objectIndex[objectId] = handle;
        }

        // Remove any object from the index that is not a prototype
        public void RemoveDynamicObjectsFromIndex()
        {
            _objectIndex = new Dictionary<ObjectId, ObjHndl>(_objectIndex.Where(pair => pair.Key.IsPrototype));
            _objects = new Dictionary<ObjHndl, GameObjectBody>(
                _objects.Where(pair => _objectIndex.ContainsValue(pair.Key))
            );
        }

        public void Clear()
        {
            _lastObj = ObjHndl.Null;
            _lastObjBody = null;

            // We will concurrently modify the object handle list when we remove them,
            // so we make a copy here first
            var toRemove = new List<ObjHndl>(_objects.Keys);

            Logger.Info("Letting {0} leftover objects leak.", toRemove.Count);

            _objectIndex.Clear();
        }

        [TempleDllLocation(0x100c3030)]
        public bool Remove(ObjHndl handle)
        {
            if (_lastObj == handle)
            {
                _lastObj = ObjHndl.Null;
                _lastObjBody = null;
            }

            if (_objects.Remove(handle, out var obj))
            {
                _objectIndex.Remove(obj.id);
                return true;
            }

            return false;
        }

        public bool Contains(ObjHndl handle)
        {
            return _objects.ContainsKey(handle);
        }

        public ObjHndl Add(GameObjectBody obj)
        {
            var id = new ObjHndl(_nextId++);

            Trace.Assert(!_objects.ContainsKey(id));

            _objects[id] = obj;

            // Cache for later use
            _lastObj = id;
            _lastObjBody = obj;

            return id;
        }

        public GameObjectBody Get(ObjHndl handle)
        {
            if (!handle)
            {
                return null;
            }

            // This would be the traditional way and it does detect when handles
            // are no longer valid
            if (_lastObj == handle)
            {
                return _lastObjBody;
            }

            if (!_objects.TryGetValue(handle, out var obj))
            {
                return null;
            }

            _lastObj = handle;
            _lastObjBody = obj;
            return obj;
        }

        private Dictionary<ObjHndl, GameObjectBody> _objects;
        private Dictionary<ObjectId, ObjHndl> _objectIndex;
        private ulong _nextId = 1;

        private ObjHndl _lastObj = ObjHndl.Null;
        private GameObjectBody _lastObjBody;

        public IEnumerator<KeyValuePair<ObjHndl, GameObjectBody>> GetEnumerator() => _objects.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _objects.GetEnumerator();
    }
}