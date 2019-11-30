using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.IO.SaveGames.GameState;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems
{
    public class DescriptionSystem : IGameSystem, ISaveGameAwareGameSystem, IModuleAwareSystem, IResetAwareSystem
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        private readonly Dictionary<int, string> _descriptions;
        private readonly Dictionary<int, string> _longDescriptions;

        [TempleDllLocation(0x10AB7574)]
        private Dictionary<int, string> _gameDesc;

        [TempleDllLocation(0x10AB756C)]
        private Dictionary<int, string> _gameKeyLog;

        [TempleDllLocation(0x10AB7578)]
        private readonly List<string> _customNames = new List<string>();

        [TempleDllLocation(0x100865d0)]
        public DescriptionSystem()
        {
            _descriptions = Tig.FS.ReadMesFile("mes/description.mes");
            _longDescriptions = Tig.FS.ReadMesFile("mes/long_description.mes");

            // Allow description overrides
            if (Tig.FS.FileExists("mes/description_ext.mes"))
            {
                var descriptionExt = Tig.FS.ReadMesFile("mes/description_ext.mes");
                foreach (var (key, value) in descriptionExt)
                {
                    _descriptions[key] = value;
                }
            }

            LoadDescriptionsFrom("mes/description", _descriptions);
            LoadDescriptionsFrom("mes/long_descr", _longDescriptions);
        }

        private static void LoadDescriptionsFrom(string directory, Dictionary<int, string> result)
        {
            foreach (var filename in Tig.FS.ListDirectory(directory))
            {
                if (!filename.EndsWith(".mes"))
                {
                    continue;
                }

                var path = Path.Combine(directory, filename);
                var descriptionExt = Tig.FS.ReadMesFile(path);
                Logger.Debug("Loaded {0} descriptions from {1}", descriptionExt.Count, path);
                foreach (var (key, value) in descriptionExt)
                {
                    result[key] = value;
                }
            }
        }

        [TempleDllLocation(0x10086670)]
        public void Dispose()
        {
        }

        [TempleDllLocation(0x10086710)]
        public void LoadModule()
        {
            _gameDesc = Tig.FS.ReadMesFile("mes/gamedesc.mes");
            _gameKeyLog = Tig.FS.ReadMesFile("mes/gamekeylog.mes");
        }

        [TempleDllLocation(0x10086780)]
        public void UnloadModule()
        {
            _gameDesc = null;
            _gameKeyLog = null;
        }

        [TempleDllLocation(0x100866c0)]
        public void Reset()
        {
            _customNames.Clear();
        }

        [TempleDllLocation(0x10086810)]
        public void SaveGame(SavedGameState savedGameState)
        {
            savedGameState.DescriptionState = new SavedDescriptionState
            {
                CustomNames = _customNames.ToList()
            };
        }

        [TempleDllLocation(0x100868b0)]
        public void LoadGame(SavedGameState savedGameState)
        {
            _customNames.Clear();
            _customNames.AddRange(savedGameState.DescriptionState.CustomNames);
        }

        [TempleDllLocation(0x100867a0)]
        public string GetLong(int descrIdx)
        {
            return _longDescriptions.GetValueOrDefault(descrIdx, null);
        }

        [TempleDllLocation(0x10086a50)]
        public int Create(string customName)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x100869d0)]
        public string Get(int descrIdx)
        {
            // check if custom name idx
            if ((descrIdx & 0x40000000) != 0)
            {
                descrIdx &= ~0x40000000;
                if (descrIdx >= 0 && descrIdx < _customNames.Count)
                {
                    return _customNames[descrIdx];
                }

                return null;
            }

            // Indices above 30000 come from the module's description.mes
            var mesLines = _descriptions;
            if (descrIdx >= 30000)
            {
                if (_gameDesc == null)
                {
                    return null;
                }

                mesLines = _gameDesc;
            }

            // look it up in the .mes extensions first
            return mesLines.GetValueOrDefault(descrIdx, null);
        }

        [TempleDllLocation(0x100867e0)]
        public string GetKeyName(int keyId)
        {
            return _gameKeyLog[keyId];
        }

    }
}