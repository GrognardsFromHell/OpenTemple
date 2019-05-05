using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
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
            throw new NotImplementedException();
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
    }
}