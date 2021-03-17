using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Subjects;
using OpenTemple.Core.GameObject;

namespace OpenTemple.Core.Systems
{
    public sealed class CritterGroup : IList<GameObjectBody>, IReadOnlyList<GameObjectBody>
    {
        private IComparer<GameObjectBody> _comparer;

        private readonly List<GameObjectBody> _members = new();

        private readonly Subject<IReadOnlyList<GameObjectBody>> _changeObservable = new();

        public IObservable<IReadOnlyList<GameObjectBody>> ChangeObservable => _changeObservable;

        public void Sort() => Sort(true);

        public void Add(GameObjectBody item)
        {
            _members.Add(item);
            Sort();
            NotifyChange();
        }

        public void Clear()
        {
            if (_members.Count > 0)
            {
                _members.Clear();
                NotifyChange();
            }
        }

        public bool Contains(GameObjectBody obj) => _members.Contains(obj);

        public void CopyTo(GameObjectBody[] array, int arrayIndex)
        {
            _members.CopyTo(array, arrayIndex);
        }

        public bool Remove(GameObjectBody item)
        {
            var result = _members.Remove(item);
            Sort(false);
            if (result)
            {
                NotifyChange();
            }

            return result;
        }

        public int Count => _members.Count;

        public bool IsReadOnly => false;

        public IComparer<GameObjectBody> Comparer
        {
            get => _comparer;
            set
            {
                _comparer = value;
                Sort();
            }
        }

        public IEnumerator<GameObjectBody> GetEnumerator()
        {
            return _members.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _members).GetEnumerator();
        }

        public int IndexOf(GameObjectBody item)
        {
            return _members.IndexOf(item);
        }

        public void Insert(int index, GameObjectBody item)
        {
            _members.Insert(index, item);
            Sort(false);
            NotifyChange();
        }

        public void RemoveAt(int index)
        {
            _members.RemoveAt(index);
            NotifyChange();
        }

        public GameObjectBody this[int index]
        {
            get => _members[index];
            set
            {
                if (_members[index] != value)
                {
                    _members[index] = value;
                    Sort(false);
                    NotifyChange();
                }
            }
        }

        private void Sort(bool notify)
        {
            if (_comparer != null)
            {
                _members.Sort(_comparer);
                if (notify)
                {
                    NotifyChange();
                }
            }
        }

        private void NotifyChange()
        {
            _changeObservable.OnNext(this);
        }
    }
}
