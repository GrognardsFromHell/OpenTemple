using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Systems.GameObjects;

internal class ObjRegistry : IEnumerable<GameObject>
{
    private static readonly ILogger Logger = LoggingSystem.CreateLogger();

    private Dictionary<ObjectId, GameObject> _objectIndex = new Dictionary<ObjectId, GameObject>();

    private readonly List<GameObject> _objects = new List<GameObject>();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _objects.GetEnumerator();
    }

    public GameObject GetById(ObjectId id)
    {
        return _objectIndex.GetValueOrDefault(id, null);
    }

    public void AddToIndex(GameObject obj, ObjectId objectId)
    {
        Trace.Assert(objectId.IsPermanent || objectId.IsPrototype || objectId.IsPositional);
        _objectIndex[objectId] = obj;
    }

    // Remove any object from the index that is not a prototype
    public void RemoveDynamicObjectsFromIndex()
    {
        _objectIndex = new Dictionary<ObjectId, GameObject>(_objectIndex.Where(pair => pair.Key.IsPrototype));
        _objects.RemoveAll(o => !_objectIndex.ContainsValue(o));
    }

    public void Clear()
    {
        // We will concurrently modify the object handle list when we remove them,
        // so we make a copy here first
        Logger.Info("Letting {0} leftover objects leak.", _objects.Count);

        _objectIndex.Clear();
        _objects.Clear();
    }

    [TempleDllLocation(0x100c3030)]
    public bool Remove(GameObject obj)
    {
        // TODO: In debug mode we should validate more

        if (_objects.Remove(obj))
        {
            _objectIndex.Remove(obj.id);
            return true;
        }

        return false;
    }

    public bool Contains(GameObject obj)
    {
        return _objects.Contains(obj);
    }

    public void Add(GameObject obj)
    {
        if (_objects.Contains(obj))
        {
            foreach (var (id, indexedObj) in _objectIndex)
            {
                if (indexedObj == obj)
                {
                    throw new InvalidOperationException(
                        $"Object {obj} is already in the registry and in the index under ID {id}");
                }
            }

            throw new InvalidOperationException($"Object {obj} is already in the registry (but not in the index)");
        }

        _objects.Add(obj);
    }

    public IEnumerator<GameObject> GetEnumerator()
    {
        return _objects.GetEnumerator();
    }
}