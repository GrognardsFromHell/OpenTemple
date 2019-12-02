using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO.Fonts;
using SpicyTemple.Core.IO.SaveGames;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.Systems.Script.Hooks;

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

    /// <summary>
    /// Loads / Saves the Co8 persistent data.
    /// </summary>
    public class Co8PersistentDataSave : ISaveGameHook
    {
        public void OnAfterSave(string saveDirectory, SaveGameFile saveFile)
        {
            throw new NotImplementedException();
        }

        public void OnAfterLoad(string saveDirectory, SaveGameFile saveFile)
        {
            var co8State = saveFile.Co8State;
            if (co8State != null)
            {
                Co8PersistentData.Flags = new Dictionary<string, bool>(co8State.Flags);
                Co8PersistentData.Vars = new Dictionary<string, int>(co8State.Vars);
                Co8PersistentData.StringVars = new Dictionary<string, string>(co8State.StringVars);
            }
            else
            {
                Co8PersistentData.Reset();
            }
        }
    }

    public static class Co8PersistentData
    {
        public static Dictionary<string, bool> Flags { get; set; } = new Dictionary<string, bool>();

        public static Dictionary<string, int> Vars { get; set; } = new Dictionary<string, int>();

        public static Dictionary<string, string> StringVars { get; set; } = new Dictionary<string, string>();

        public static void Reset()
        {
            Flags.Clear();
            Vars.Clear();
            StringVars.Clear();
        }

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
            return Flags.GetValueOrDefault(key, false);
        }

        public static void SetBool(string key, bool value)
        {
            Flags[key] = value;
        }

        public static int GetInt(string key)
        {
            return Vars.GetValueOrDefault(key, 0);
        }

        public static void SetInt(string key, int value)
        {
            Vars[key] = value;
        }

        public static string GetString(string key)
        {
            return StringVars.GetValueOrDefault(key, "");
        }

        public static void SetString(string key, string value)
        {
            StringVars[key] = value;
        }
    }
}