using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Raycast;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers;

internal class LocationTargetBehavior : PickerBehavior
{
    public override UiPickerType Type => UiPickerType.Location;

    public override string Name => "SP_M_LOCATION";

    public LocationTargetBehavior(PickerState pickerState) : base(pickerState)
    {
    }

    [TempleDllLocation(0x101363c0)]
    internal override bool LeftMouseButtonReleased(IGameViewport viewport, MouseEvent e)
    {
        ClearResults();

        var targetLocation = GameViews.Primary.ScreenToTile(e.X, e.Y);

        var distance = Picker.caster.GetLocationFull().DistanceTo(targetLocation);

        // TODO: This seems like a bug, since range seems to be in feet, but the distance is in inches
        // TODO: Second bug: Radius of caster is ignored (see MouseMove)
        if (distance >= Picker.range)
        {
            return true;
        }


        if (Picker.modeTarget.HasFlag(UiPickerType.LocIsClear))
        {
            if (!IsTargetLocationClear(targetLocation))
            {
                return true;
            }
        }

        SetResultLocation(targetLocation);
        return FinalizePicker();
    }

    private bool IsTargetLocationClear(LocAndOffsets targetLocation)
    {
        var radius = Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Radius)
            ? Picker.radiusTarget * locXY.INCH_PER_FEET
            : Picker.caster.GetRadius();

        using var raycast = new RaycastPacket();
        raycast.sourceObj = Picker.caster;
        raycast.origin = targetLocation;
        raycast.targetLoc = targetLocation;
        raycast.radius = radius;
        raycast.flags = RaycastFlag.ExcludeItemObjects
                        | RaycastFlag.StopAfterFirstBlockerFound
                        | RaycastFlag.StopAfterFirstFlyoverFound
                        | RaycastFlag.HasSourceObj
                        | RaycastFlag.HasRadius;

        return raycast.RaycastShortRange() == 0;
    }

    [TempleDllLocation(0x101365d0)]
    internal override bool MouseMoved(IGameViewport viewport, MouseEvent e)
    {
        var sourceLocation = Picker.caster.GetLocationFull();
        var radius = Picker.caster.GetRadius();
        var targetLocation = GameViews.Primary.ScreenToTile(e.X, e.Y);

        PickerStatusFlags &= ~(PickerStatusFlags.Invalid | PickerStatusFlags.OutOfRange);

        if (Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Range))
        {
            var distance = sourceLocation.DistanceTo(targetLocation) - radius;
            if (distance > Picker.range)
            {
                PickerStatusFlags |= PickerStatusFlags.OutOfRange;
            }
        }

        if (Picker.modeTarget.HasFlag(UiPickerType.LocIsClear))
        {
            if (!IsTargetLocationClear(targetLocation))
            {
                PickerStatusFlags |= PickerStatusFlags.Invalid;
            }
        }

        return true;
    }
}