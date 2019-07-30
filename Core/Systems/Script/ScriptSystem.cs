using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.Script
{
    public class ScriptSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {

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

        public SpellScriptSystem Spells { get; } = new SpellScriptSystem();

        public ActionScriptSystem Actions { get; } = new ActionScriptSystem();

        [TempleDllLocation(0x10006580)]
        public ScriptSystem()
        {
            // TODO: init python from here
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
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100066e0)]
        public bool SaveGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10006670)]
        public bool LoadGame()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1000bb60)]
        public void Invoke(ref ObjScriptInvocation invocation)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, int spellId, int unk1,
            ObjScriptEvent evt, int unk2)
        {
            if (!IsEditor)
            {
                Stub.TODO();
            }

            return 1;
        }

        [TempleDllLocation(0x10025d60)]
        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, GameObjectBody objectArg,
            ObjScriptEvent evt, int unk2)
        {
            Stub.TODO();
            return 1;
        }

        public int ExecuteObjectScript(GameObjectBody triggerer, GameObjectBody attachee, ObjScriptEvent evt)
        {
            return ExecuteObjectScript(triggerer, attachee, 0, 0, evt, 0);
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

        [TempleDllLocation(0x10006790)]
        public bool GetGlobalFlag(int index) => ((_globalFlags[index / 32] >> index % 32) & 1) != 0;

        [TempleDllLocation(0x100067c0)]
        public void SetGlobalFlag(int index, bool enable)
        {
            var value = enable ? 1 : 0;

            _globalFlags[index / 32] = (uint) ((value << index % 32) | _globalFlags[index / 32] & ~(1 << index % 32));
        }

        [TempleDllLocation(0x10BCA76C)]
        private GameObjectBody _animationScriptContext;

        [TempleDllLocation(0x100aeda0)]
        public void SetAnimObject(GameObjectBody obj)
        {
            // Sets the Python global for which obj was just animated
            _animationScriptContext = obj;
        }

        [TempleDllLocation(0x100c0180)]
        public void ExecuteSpellScript(int spellId, SpellEvent evt)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Is used to judge whether someone is threatening an attack of opportunity during movement.
        /// </summary>
        public bool ShouldIgnoreTargetDuringCombat(GameObjectBody obj, GameObjectBody target)
        {
            Stub.TODO();
            return false;
        }
    }
}