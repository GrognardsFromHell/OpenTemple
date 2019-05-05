using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpicyTemple.Core.Platform;

namespace SpicyTemple.Core.Config
{
    public enum HpOnLevelUpMode
    {
        Max,
        Average,
        Roll
    }

    public class GameConfig
    {
        /// <summary>
        /// The folder where Tempe of Elemental Evil is installed.
        /// </summary>
        public string InstallationFolder { get; set; }

        public RenderingConfig Rendering { get; set; } = new RenderingConfig();

        public WindowConfig Window { get; set; } = new WindowConfig();

        public bool SkipLegal { get; set; } = true;
        public bool laxRules { get; set; }

        public bool disableAlignmentRestrictions { get; set; }

        public bool drawObjCylinders { get; set; }

        public bool softShadows { get; set; }

        public string fogOfWar { get; set; } = "";

        public HpOnLevelUpMode HpOnLevelUpMode { get; set; }

        public bool MaxHpForNpcHitdice { get; set; }

        public bool monstrousRaces { get; set; }

        public bool forgottenRealmsRaces { get; set; }

        public bool newRaces { get; set; }

        public Dictionary<string, string> VanillaSettings = new Dictionary<string, string>();

        public string GetVanillaString(string name) => VanillaSettings[name];

        public int GetVanillaInt(string name) => int.Parse(GetVanillaString(name));

        public void SetVanillaInt(string name, int value) => VanillaSettings[name] = value.ToString();

        public void AddVanillaSetting(string name, string defaultValue, Action changeCallback = null)
        {
            if (VanillaSettings.ContainsKey(name))
            {
                return;
            }

            VanillaSettings[name] = defaultValue;
            // TODO
        }

        public void RemoveVanillaCallback(string name)
        {
            // TODO
        }
    }
}