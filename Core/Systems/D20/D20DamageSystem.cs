using System;
using System.Collections.Generic;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.TigSubsystems;

namespace SpicyTemple.Core.Systems.D20
{
    public class D20DamageSystem
    {
        private readonly Dictionary<int, string> _translations;

        [TempleDllLocation(0x100e0360)]
        public D20DamageSystem()
        {
            _translations = Tig.FS.ReadMesFile("mes/damage.mes");
        }

        public string GetTranslation(int id)
        {
            return _translations[id];
        }

        [TempleDllLocation(0x100e0ab0)]
        public string GetDamageTypeName(DamageType type)
        {
            return _translations[1001 + (int) type];
        }

    }
}