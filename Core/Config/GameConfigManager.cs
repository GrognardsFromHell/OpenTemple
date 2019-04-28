using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SpicyTemple.Core.Config
{
    /**
     * Manages loading and saving the game configuration.
     */
    public class GameConfigManager
    {
        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriterOptions = new JsonWriterOptions
            {
                Indented = true
            }
        };

        private readonly string _configPath;

        public GameConfig Config { get; }

        public GameConfigManager(string configPath)
        {
            _configPath = configPath;

            if (File.Exists(_configPath))
            {
                var configJson = File.ReadAllBytes(_configPath);

                Config = JsonSerializer.Parse<GameConfig>(configJson);
            }
            else
            {
                Config = new GameConfig();
                Save(); // Writes a default config
            }
        }

        public void Save()
        {
            var configJson = JsonSerializer.ToBytes(Config, JsonOptions);
            File.WriteAllBytes(_configPath, configJson);
        }
    }
}