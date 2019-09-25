using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;

namespace Scripts
{

    public class GetSpellActiveList
    {
        public void Add(int spellId, GameObjectBody target)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<GameObjectBody> EnumerateTargets(int spellId)
        {
            throw new NotImplementedException();
        }

        public void Remove(GameObjectBody target)
        {
            throw new NotImplementedException();
        }
    }

    public static class Co8PersistentData
    {
        public static GetSpellActiveList GetSpellActiveList(string key)
        {
throw new NotImplementedException();
        }

        public static void AddToSpellActiveList(string key, int spellId, GameObjectBody target)
        {
            GetSpellActiveList(key).Add(spellId, target);
        }

        public static void CleanupActiveSpellTargets(string key, int spellId, Action<GameObjectBody> cleanupCallback)
        {
            throw new NotImplementedException();
        }

        public static bool GetBool(string key)
        {
            throw new NotImplementedException();
        }

        public static void SetBool(string key, bool value)
        {
            throw new NotImplementedException();
        }

        public static int GetInt(string key)
        {
            throw new NotImplementedException();
        }

        public static void SetInt(string key, int value)
        {
            throw new NotImplementedException();
        }

        public static string GetString(string key)
        {
            // Returns "" instead of null
            throw new NotImplementedException();
        }

        public static void SetString(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}