using System.Collections;
using System.Collections.Generic;
using System.IO;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO;

namespace OpenTemple.Core.Systems;

public sealed class CritterGroup : IList<GameObject>, IReadOnlyList<GameObject>
{
    private IComparer<GameObject> _comparer;

    private readonly List<GameObject> _members = new();

    public void Sort()
    {
        if (_comparer != null)
        {
            _members.Sort(_comparer);
        }
    }

    public void Add(GameObject item)
    {
        _members.Add(item);
        Sort();
    }

    public void Clear()
    {
        _members.Clear();
    }

    public bool Contains(GameObject obj) => _members.Contains(obj);

    public void CopyTo(GameObject[] array, int arrayIndex)
    {
        _members.CopyTo(array, arrayIndex);
    }

    public bool Remove(GameObject item)
    {
        var result = _members.Remove(item);
        Sort();
        return result;
    }

    public int Count => _members.Count;

    public bool IsReadOnly => false;

    public IComparer<GameObject> Comparer
    {
        get => _comparer;
        set
        {
            _comparer = value;
            Sort();
        }
    }

    public IEnumerator<GameObject> GetEnumerator()
    {
        return _members.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable) _members).GetEnumerator();
    }

    public int IndexOf(GameObject item)
    {
        return _members.IndexOf(item);
    }

    public void Insert(int index, GameObject item)
    {
        _members.Insert(index, item);
        Sort();
    }

    public void RemoveAt(int index)
    {
        _members.RemoveAt(index);
    }

    public GameObject this[int index]
    {
        get => _members[index];
        set
        {
            _members[index] = value;
            Sort();
        }
    }

}