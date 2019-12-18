using System;
using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.Feats;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Spells;
using SpicyTemple.Core.Systems.GameObjects;
using SpicyTemple.Core.Systems.D20.Conditions;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.Ui;
using System.Linq;
using SpicyTemple.Core.Startup.Discovery;
using SpicyTemple.Core.Systems.RadialMenus;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.Utils;
using static SpicyTemple.Core.Systems.Script.ScriptUtilities;

namespace SpicyTemple.Core.Systems.D20.Conditions.TemplePlus
{
    // Sudden Enlarge:  Miniatures Handbook, p. 28
    public class SuddenEnlarge
    {
        public static void ApplyEnlarge(ref MetaMagicData metaMagicData)
        {
            // Don't enlarge more than once
            if (metaMagicData.metaMagicEnlargeSpellCount < 1)
            {
                metaMagicData.metaMagicEnlargeSpellCount = 1;
            }
        }

        // TODO tpdp.register_metamagic_feat("Sudden Enlarge");

        // Charges, Toggeled On, Spare, Spare
        [FeatCondition("Sudden Enlarge")]
        [AutoRegister] public static readonly ConditionSpec Condition = SuddenMetamagic
            .Create("Sudden Enlarge Feat", "Sudden Enlarge", ApplyEnlarge)
            .Build();
    }
}