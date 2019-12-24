using System;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script.Extensions;

namespace OpenTemple.Core.Systems.Script
{
    public static class ScriptTrapExtensions
    {

        [PythonName("attack")]
        public static D20CAF Attack(this TrapSprungEvent evt, GameObjectBody target, int attackBonus, int criticalHitRange, bool rangedAttack)
        {
            throw new NotImplementedException();
        }

    }
}