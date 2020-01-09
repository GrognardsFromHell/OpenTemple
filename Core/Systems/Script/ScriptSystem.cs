using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using JetBrains.Annotations;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.IO.SaveGames.GameState;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Systems.Script.Hooks;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Systems.Script
{
    public class ScriptSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        public delegate void InitiateDialog(GameObjectBody obj1, GameObjectBody obj2, int scriptNumber,
            int unk1, int argFromEvent);

        public delegate void ShowMessage(GameObjectBody speaker, GameObjectBody speakingTo, string text, int speechId);

        private const bool IsEditor = false;

        [TempleDllLocation(0x103073B8)]
        private int[] _globalVars = new int[2000];

        [TempleDllLocation(0x103073A8)]
        private uint[] _globalFlags = new uint[100];

        [TempleDllLocation(0x103073A0)]
        private int _currentStoryState = 0;

        [TempleDllLocation(0x103073AC)]
        private InitiateDialog _scriptDialogInitiate = null;

        [TempleDllLocation(0x103073BC)]
        private ShowMessage _scriptShowMessage = null;

        [TempleDllLocation(0x102AC388)]
        private Dictionary<int, string> _storyStateText;

        private readonly ScriptAssembly _scriptAssembly;

        private readonly Dictionary<Type, object> _hooks = new Dictionary<Type, object>();

        public SpellScriptSystem Spells { get; }

        public ActionScriptSystem Actions { get; }

        [TempleDllLocation(0x10006580)]
        public ScriptSystem()
        {
            // TODO: init python from here
            _scriptAssembly = new ScriptAssembly(Globals.Config.ScriptAssemblyName);

            Spells = new SpellScriptSystem(_scriptAssembly);
            Actions = new ActionScriptSystem();
        }

        [TempleDllLocation(0x10007b60)]
        public void Dispose()
        {
            // TODO: shutdown python here
        }

        [TempleDllLocation(0x10006630)]
        public void LoadModule()
        {
            _storyStateText = Tig.FS.ReadMesFile("mes/storystate.mes");
        }

        [TempleDllLocation(0x10006650)]
        public void UnloadModule()
        {
            _storyStateText.Clear();
        }

        [TempleDllLocation(0x10007ae0)]
        public void Reset()
        {
            Stub.TODO("Clear dialoger picker args"); // TODO Clear dialog picker args PyGame_Exit

            _currentStoryState = 0;
            Array.Fill(_globalVars, 0);
            Array.Fill(_globalFlags, 0u);
        }

        [TempleDllLocation(0x100066e0)]
        public void SaveGame(SavedGameState savedGameState)
        {
            if (savedGameState.ScriptState == null)
            {
                savedGameState.ScriptState = new SavedScriptState();
            }

            savedGameState.ScriptState.GlobalFlags = _globalFlags.ToArray();
            savedGameState.ScriptState.GlobalVars = _globalVars.ToArray();
            savedGameState.ScriptState.StoryState = _currentStoryState;
        }

        [TempleDllLocation(0x10006670)]
        public void LoadGame(SavedGameState savedGameState)
        {
            var scriptState = savedGameState.ScriptState;
            scriptState.GlobalVars.CopyTo(_globalVars, 0);
             scriptState.GlobalFlags.CopyTo(_globalFlags, 0);
             _currentStoryState = scriptState.StoryState;
        }

        public bool TryGetDialogScript(int scriptId, out IDialogScript dialogScript)
        {
            return _scriptAssembly.TryCreateDialogScript(scriptId, out dialogScript);
        }

        [TempleDllLocation(0x1000bb60)]
        public bool Invoke(ref ObjScriptInvocation invocation)
        {
            if (IsEditor)
            {
                return true;
            }

            var attachee = invocation.attachee;
            if (attachee == null)
            {
                throw new NullReferenceException("Cannot run a script without an attachee");
            }

            var script = attachee.GetScript(obj_f.scripts_idx, (int) invocation.eventId);

            if (script.scriptId == 0)
            {
                return true; // No script attached
            }

            if (!_scriptAssembly.TryCreateObjectScript(script.scriptId, out var scriptObj))
            {
                Logger.Error("Object {0} has broken script {1} attached.", attachee, script.scriptId);
                return true;
            }

            return scriptObj.Invoke(ref invocation);
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, int spellId,
            ObjScriptEvent evt)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            if (spellId != 0)
            {
                invocation.spell = GameSystems.Spell.GetActiveSpell(spellId);
            }

            return Invoke(ref invocation) ? 1 : 0;
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, GameObjectBody objectArg,
            ObjScriptEvent evt, int unk2)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            return Invoke(ref invocation) ? 1 : 0;
        }

        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, ObjScriptEvent evt)
        {
            var invocation = new ObjScriptInvocation();
            invocation.eventId = evt;
            invocation.triggerer = triggerer;
            invocation.attachee = attachee;
            return Invoke(ref invocation) ? 1 : 0;
        }

        public bool GetLegacyHeader(ref ObjectScript script)
        {
            var path = GameSystems.ScriptName.GetScriptPath(script.scriptId);
            if (path != null)
            {
                using var reader = Tig.FS.OpenBinaryReader(path);
                script.unk1 = reader.ReadInt32();
                script.counters = reader.ReadUInt32();
                return true;
            }

            return false;
        }

        [TempleDllLocation(0x10006a20)]
        [TempleDllLocation(0x10006a30)]
        public int StoryState
        {
            get => _currentStoryState;
            set
            {
                if (value > _currentStoryState)
                {
                    _currentStoryState = value;
                }
            }
        }

        [TempleDllLocation(0x10006790)]
        public bool GetGlobalFlag(int index) => ((_globalFlags[index / 32] >> index % 32) & 1) != 0;

        [TempleDllLocation(0x100067c0)]
        public void SetGlobalFlag(int index, bool enable)
        {
            var value = enable ? 1 : 0;

            _globalFlags[index / 32] = (uint) ((value << index % 32) | _globalFlags[index / 32] & ~(1 << index % 32));
        }

        [TempleDllLocation(0x10006760)]
        public int GetGlobalVar(int index) => _globalVars[index];

        [TempleDllLocation(0x10006770)]
        public void SetGlobalVar(int index, int value) => _globalVars[index] = value;

        [TempleDllLocation(0x10BCA76C)]
        private GameObjectBody _animationScriptContext;

        [TempleDllLocation(0x100aeda0)]
        public void SetAnimObject(GameObjectBody obj)
        {
            // Sets the Python global for which obj was just animated
            _animationScriptContext = obj;
        }

        /// <summary>
        /// Executes custom Python script logic.
        /// </summary>
        public T ExecuteScript<T>(string module, string function, params object[] args)
        {
            throw new NotImplementedException();
        }

        [CanBeNull]
        public T GetHook<T>() where T : class
        {
            if (_hooks.TryGetValue(typeof(T), out var hookInstance))
            {
                return (T) hookInstance;
            }

            if (!_scriptAssembly.TryCreateHook<T>(out var newHookInstance))
            {
                _hooks[typeof(T)] = null;
                return null;
            }

            return newHookInstance;
        }
    }
}