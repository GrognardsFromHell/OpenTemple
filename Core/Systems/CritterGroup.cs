using System.Collections;
using System.Collections.Generic;
using System.IO;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;

namespace SpicyTemple.Core.Systems
{
    public sealed class CritterGroup : IList<GameObjectBody>, IReadOnlyList<GameObjectBody>
    {
        private IComparer<GameObjectBody> _comparer;

        private readonly List<GameObjectBody> _members = new List<GameObjectBody>();

        public List<GameObjectBody> Members { get; } = new List<GameObjectBody>();

        public void Sort()
        {
            if (_comparer != null)
            {
                _members.Sort(_comparer);
            }
        }

        public void Add(GameObjectBody item)
        {
            _members.Add(item);
            Sort();
        }

        public void Clear()
        {
            _members.Clear();
        }

        public bool Contains(GameObjectBody obj) => _members.Contains(obj);

        public void CopyTo(GameObjectBody[] array, int arrayIndex)
        {
            _members.CopyTo(array, arrayIndex);
        }

        public bool Remove(GameObjectBody item)
        {
            var result = _members.Remove(item);
            Sort();
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
            Sort();
        }

        public void RemoveAt(int index)
        {
            _members.RemoveAt(index);
        }

        public GameObjectBody this[int index]
        {
            get => _members[index];
            set
            {
                _members[index] = value;
                Sort();
            }
        }

        [TempleDllLocation(0x100df7c0)]
        public void WriteTo(BinaryWriter writer)
        {
            int memberCount = _members.Count;
            writer.Write(memberCount);
            foreach (var member in _members)
            {
                var id = GameSystems.Object.GetPersistableId(member);
                writer.WriteObjectId(id);
            }
        }

        [TempleDllLocation(0x100df860)]
        public void LoadFrom(BinaryReader reader)
        {
            _members.Clear();
            _comparer = null;

            var groupSize = reader.ReadInt32();
            for (int i = 0; i < groupSize; i++)
            {
                var id = reader.ReadObjectId();
                _members.Add(GameSystems.Object.GetObject(id));
            }
        }
    }
}