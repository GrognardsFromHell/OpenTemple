using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers;

internal class PersonalTargetBehavior : PickerBehavior
{
    public override UiPickerType Type => UiPickerType.Personal;

    public override string Name => "SP_M_PERSONAL";

    public PersonalTargetBehavior(PickerState pickerState) : base(pickerState)
    {
    }

    [TempleDllLocation(0x10135fd0)]
    internal override bool LeftMouseButtonReleased(IGameViewport viewport, MouseEvent e)
    {
        ClearResults();

        var raycastFlags = PickerState.GetFlagsFromExclusions(e);
        if (!GameSystems.Raycast.PickObjectOnScreen(viewport, e.X, e.Y, out var target, raycastFlags) ||
            target != Picker.caster)
        {
            return true;
        }

        if (!Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Radius))
        {
            Picker.SetSingleTgt(target);
        }
        else
        {
            Picker.SetAreaTargets(target.GetLocationFull());
        }

        return FinalizePicker();
    }

    [TempleDllLocation(0x101361b0)]
    internal override bool MouseMoved(IGameViewport viewport, MouseEvent e)
    {
        ClearResults();
        PickerStatusFlags |= PickerStatusFlags.Invalid;

        var raycastFlags = PickerState.GetFlagsFromExclusions(e);
        GameSystems.Raycast.PickObjectOnScreen(viewport, e.X, e.Y, out var target, raycastFlags);

        PickerState.Target = target;
        if (target != Picker.caster)
        {
            return false;
        }

        PickerStatusFlags &= ~PickerStatusFlags.Invalid;

        if (!Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Radius))
        {
            Picker.SetSingleTgt(target);
        }
        else
        {
            Picker.SetAreaTargets(target.GetLocationFull());
        }

        return false;
    }
}