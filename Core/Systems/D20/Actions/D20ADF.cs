using System;

namespace SpicyTemple.Core.Systems.D20.Actions
{
    [Flags]
    public enum D20ADF
    {
        D20ADF_None = 0,
        D20ADF_Unk1 = 1,
        D20ADF_Unk2 = 2,
        D20ADF_Movement = 4,
        D20ADF_TargetSingleExcSelf = 8,
        D20ADF_MagicEffectTargeting = 0x10,
        D20ADF_Unk20 = 0x20,
        D20ADF_Unk40 = 0x40,

        // will trigger an AoO depending on a D20 Query for Action_Triggers_AOO (returns 1 by default from the Global condition, Cast Defensively sets this to 0 for D20A_CAST_SPELL)
        D20ADF_QueryForAoO = 0x80,
        D20ADF_TriggersAoO = 0x100,
        D20ADF_TargetSingleIncSelf = 0x200,
        D20ADF_TargetingBasedOnD20Data = 0x400,

        // might be somewhat more general actually
        D20ADF_TriggersCombat = 0x800,
        D20ADF_CallLightningTargeting = 0x1000,
        D20ADF_Unk2000 = 0x2000,
        D20ADF_Unk4000 = 0x4000,

        // indicates that the target should be selected with a "normal" cursor (as opposed to a picker)
        D20ADF_UseCursorForPicking = 0x8000,
        D20ADF_TargetContainer = 0x10000,
        D20ADF_SimulsCompatible = 0x20000,

        // will draw path even without holding ALT
        D20ADF_DrawPathByDefault = 0x40000,
        D20ADF_DoLocationCheckAtDestination = 0x80000,
        D20ADF_Breaks_Concentration = 0x100000,

        D20ADF_Python = 0x1000000
    }
}