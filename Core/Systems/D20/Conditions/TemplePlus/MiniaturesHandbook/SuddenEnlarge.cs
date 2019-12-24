using System;
using System.Collections.Generic;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.Script;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.Systems.GameObjects;
using OpenTemple.Core.Systems.D20.Conditions;
using OpenTemple.Core.Location;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.Ui;
using System.Linq;
using OpenTemple.Core.Startup.Discovery;
using OpenTemple.Core.Systems.RadialMenus;
using OpenTemple.Core.Systems.Script.Extensions;
using OpenTemple.Core.Utils;
using static OpenTemple.Core.Systems.Script.ScriptUtilities;

namespace OpenTemple.Core.Systems.D20.Conditions.TemplePlus
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