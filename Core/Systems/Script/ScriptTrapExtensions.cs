using System;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script.Extensions;

namespace SpicyTemple.Core.Systems.Script
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