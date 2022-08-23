using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers;

internal class SingleTargetBehavior : PickerBehavior
{
    [TempleDllLocation(0x101389c0)]
    public SingleTargetBehavior(PickerState pickerState) : base(pickerState)
    {
        UiSystems.Party.SetTargetCallbacks(
            SelectSingleTargetCallback,
            PickerMultiSingleCheckFlags,
            ActivePickerReset
        );
    }

    public override UiPickerType Type => UiPickerType.Single;

    public override string Name => "SP_M_SINGLE";

    [TempleDllLocation(0x10137870)]
    internal override bool LeftMouseButtonReleased(IGameViewport viewport, MessageMouseArgs args)
    {
        if (HandleClickInUnexploredArea(args.X, args.Y))
        {
            return false;
        }

        ClearResults();

        var raycastFlags = PickerState.GetFlagsFromExclusions(args);
        if (!GameSystems.Raycast.PickObjectOnScreen(viewport, args.X, args.Y, out var objFound, raycastFlags))
        {
            return false;
        }

        if (!PickerState.Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired) &&
            PickerState.Picker.LosBlocked(objFound))
        {
            return false;
        }

        if (PickerState.Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st) &&
            !PickerState.Picker.TargetValid(objFound))
        {
            return false;
        }

        if (!PickerState.Picker.CheckTargetVsIncFlags(objFound))
        {
            return false;
        }

        Result.flags = PickerResultFlags.PRF_HAS_SINGLE_OBJ;
        Result.handle = objFound;
        SetResultLocation(objFound);

        return FinalizePicker();
    }

    [TempleDllLocation(0x10138170)]
    internal override bool MouseMoved(IGameViewport viewport, MessageMouseArgs args)
    {
        if (HandleClickInUnexploredArea(args.X, args.Y))
        {
            return false;
        }

        var raycastFlags = PickerState.GetFlagsFromExclusions(args);
        if (!GameSystems.Raycast.PickObjectOnScreen(viewport, args.X, args.Y, out var target, raycastFlags))
        {
            PickerStatusFlags &= ~(PickerStatusFlags.Invalid | PickerStatusFlags.OutOfRange);
            PickerState.Target = null;
            return true;
        }

        PickerState.Target = target;

        if (Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st))
        {
            if (!Picker.TargetValid(target))
            {
                PickerStatusFlags |= PickerStatusFlags.Invalid;
            }
        }

        if (!Picker.CheckTargetVsIncFlags(target))
        {
            PickerStatusFlags |= PickerStatusFlags.Invalid;
        }

        if (!Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired) && Picker.LosBlocked(target))
        {
            PickerStatusFlags |= PickerStatusFlags.Invalid;
        }

        if (Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Range))
        {
            // TODO: This distance check seems bugged too (feet vs. inch, no radius considered)
            var dist = Picker.caster.GetLocationFull().DistanceTo(target.GetLocationFull());

            if (dist > Picker.range)
            {
                PickerStatusFlags |= PickerStatusFlags.OutOfRange;
            }
        }

        return true;
    }
}