using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTemple.Core.Logging;

namespace OpenTemple.Core.Config
{
    /// <summary>
    /// Manages loading and saving the game configuration.
    /// </summary>
    public class GameConfigManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(),
                new JsonTimeSpanConverter()
            }
        };

        private readonly string _configPath;

        public GameConfig Config { get; }

        public event Action OnConfigChanged;

        public void NotifyConfigChanged()
        {
            OnConfigChanged?.Invoke();
        }

        public GameConfigManager(GameFolders folders) : this(Path.Join(folders.UserDataFolder, "config.json"))
        {
        }

        public GameConfigManager(string configPath)
        {
            _configPath = configPath;

            if (File.Exists(_configPath))
            {
                var configJson = File.ReadAllBytes(_configPath);

                Config = JsonSerializer.Deserialize<GameConfig>(configJson, JsonOptions);
            }
            else
            {
                Config = new GameConfig();
                Save(); // Writes a default config
            }
        }

        /// <summary>
        /// This is used for testing.
        /// </summary>
        public GameConfigManager(GameConfig config)
        {
            _configPath = null;
            Config = config;
        }

        public void Save()
        {
            if (_configPath != null)
            {
                var configJson = JsonSerializer.SerializeToUtf8Bytes(Config, JsonOptions);
                File.WriteAllBytes(_configPath, configJson);
            }
        }
    }
}