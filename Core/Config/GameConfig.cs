using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Config
{
    public class GameConfig
    {
        /// <summary>
        /// The folder where Tempe of Elemental Evil is installed.
        /// </summary>
        public string InstallationFolder { get; set; }

        public RenderingConfig Rendering { get; set; } = new RenderingConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public bool SkipLegal { get; set; } = true;

        public Dictionary<string, string> VanillaSettings = new Dictionary<string, string>();

        public string GetVanillaString(string name) => VanillaSettings[name];

        public int GetVanillaInt(string name) => int.Parse(GetVanillaString(name));

        public void AddVanillaSetting(string name, string defaultValue, Action changeCallback = null)
        {
            if (VanillaSettings.ContainsKey(name))
            {
                return;
            }

            VanillaSettings[name] = defaultValue;
            // TODO
        }
    }
}