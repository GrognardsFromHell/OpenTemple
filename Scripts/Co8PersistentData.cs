using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.IO.Fonts;
using OpenTemple.Core.IO.SaveGames;
using OpenTemple.Core.IO.SaveGames.Co8State;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Script.Hooks;

namespace Scripts
{

    /// <summary>
    /// Loads / Saves the Co8 persistent data.
    /// </summary>
    public class Co8PersistentDataSave : ISaveGameHook
    {
        public void OnAfterSave(string saveDirectory, SaveGameFile saveFile)
        {
            if (Co8PersistentData.Flags.Count == 0
                && Co8PersistentData.Vars.Count == 0
                && Co8PersistentData.StringVars.Count == 0
                && Co8PersistentData.ActiveSpellTargets.Count == 0)
            {
                return;
            }

            saveFile.Co8State = new SavedCo8State
            {
                Flags = new Dictionary<string, bool>(Co8PersistentData.Flags),
                Vars = new Dictionary<string, int>(Co8PersistentData.Vars),
                StringVars = new Dictionary<string, string>(Co8PersistentData.StringVars),
                ActiveSpellTargets = Co8PersistentData.ActiveSpellTargets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToArray()
                )
            };
        }

        public void OnAfterLoad(string saveDirectory, SaveGameFile saveFile)
        {
            var co8State = saveFile.Co8State;
            if (co8State != null)
            {
                Co8PersistentData.Flags = new Dictionary<string, bool>(co8State.Flags);
                Co8PersistentData.Vars = new Dictionary<string, int>(co8State.Vars);
                Co8PersistentData.StringVars = new Dictionary<string, string>(co8State.StringVars);
                Co8PersistentData.ActiveSpellTargets = co8State.ActiveSpellTargets.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.ToHashSet()
                );
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

        public static Dictionary<int, HashSet<ObjectId>> ActiveSpellTargets { get; set; }
            = new Dictionary<int, HashSet<ObjectId>>();

        public static void Reset()
        {
            Flags.Clear();
            Vars.Clear();
            StringVars.Clear();
            ActiveSpellTargets.Clear();
        }

        public static void AddToSpellActiveList(string key, int spellId, GameObject target)
        {
            if (!ActiveSpellTargets.TryGetValue(spellId, out var targetIds))
            {
                targetIds = new HashSet<ObjectId>();
                ActiveSpellTargets[spellId] = targetIds;
            }

            targetIds.Add(target.id);
        }

        public static void CleanupActiveSpellTargets(string key, int spellId, Action<GameObject> cleanupCallback)
        {
            if (ActiveSpellTargets.TryGetValue(spellId, out var targetIds))
            {
                foreach (var targetId in targetIds)
                {
                    var target = GameSystems.Object.GetObject(targetId);
                    if (target != null)
                    {
                        cleanupCallback(target);
                    }
                }

                ActiveSpellTargets.Remove(spellId);
            }
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