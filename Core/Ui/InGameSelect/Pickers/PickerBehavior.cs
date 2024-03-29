using System.Diagnostics;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Ui.Events;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers;

internal abstract class PickerBehavior
{

    public abstract UiPickerType Type { get; }

    public abstract string Name { get; }

    public PickerState PickerState { get; }

    [TempleDllLocation(0x10BE5F2C)]
    public PickerStatusFlags PickerStatusFlags { get; protected set; }

    protected PickerBehavior(PickerState pickerState)
    {
        PickerState = pickerState;
    }

    protected PickerArgs Picker => PickerState.Picker;

    protected ref PickerResult Result => ref PickerState.Picker.result;

    [TempleDllLocation(0x100b9970)]
    protected void ClearResults()
    {
        PickerState.Picker.result.flags = default;
        PickerState.Picker.result.objList = null;
        PickerState.Picker.result.handle = null;
        PickerState.Picker.result.location = LocAndOffsets.Zero;
        PickerState.Picker.result.offsetz = 0;

        PickerState.Target = null;
        PickerState.tgtIdx = 0;
    }

    protected void SetSingleResult(GameObject obj)
    {
        Trace.Assert(obj != null);
        PickerState.Picker.result.flags = PickerResultFlags.PRF_HAS_SINGLE_OBJ;
        PickerState.Picker.result.handle = obj;
        PickerState.Picker.result.objList = null;
    }

    protected bool FinalizePicker()
    {
        var pickArgs = PickerState.Picker;
        pickArgs.DoExclusions();

        var flags = pickArgs.result.flags;
        ref var pickerRes = ref pickArgs.result;

        // Validate presence of targets according to the flags on the result
        var modeTarget = pickArgs.GetBaseModeTarget();
        if (modeTarget == UiPickerType.Single || modeTarget == UiPickerType.Multi)
        {
            if (!pickerRes.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ) &&
                !pickerRes.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ))
            {
                return true;
            }

            if (pickerRes.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ) &&
                pickArgs.result.objList.Count <= 0)
            {
                return true;
            }

            if (pickerRes.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ) && pickArgs.result.handle == null)
            {
                return true;
            }
        }

        pickArgs.callback?.Invoke(ref pickerRes, PickerState.CallbackArgs);
        ClearResults();
        return flags != 0;
    }

    protected void SetResultLocation(GameObject obj)
    {
        Result.flags |= PickerResultFlags.PRF_HAS_LOCATION;
        Result.location = obj.GetLocationFull();
        Result.offsetz = obj.OffsetZ;
    }

    protected void SetResultLocation(LocAndOffsets location)
    {
        Result.flags |= PickerResultFlags.PRF_HAS_LOCATION;
        Result.location = location;
        Result.offsetz = 0;
    }

    [TempleDllLocation(0x10136b20)]
    protected void SetResultLocationFromMouse(MouseEvent e)
    {
        // NOTE: We don't reset the taget list fully, as vailla did
        Result.flags |= PickerResultFlags.PRF_HAS_LOCATION;
        Result.location = GameViews.Primary.ScreenToTile(e.X, e.Y);
        Result.offsetz = 0;
    }

    protected bool HandleClickInUnexploredArea(float x, float y, bool resetTargets = true)
    {
        // Make sure we're not picking in fog of war
        var loc = GameViews.Primary.ScreenToTile(x, y);
        if (!GameSystems.MapFogging.IsExplored(loc))
        {
            if (resetTargets)
            {
                // change from vanilla - decided not to reset the selection list. atari bug #536
                ClearResults();
            }

            PickerStatusFlags |= PickerStatusFlags.Invalid;
            return true;
        }

        return false;
    }

    public void CancelPicker()
    {
        Result.flags |= PickerResultFlags.PRF_CANCELLED;
        Picker.callback?.Invoke(ref Result, PickerState.CallbackArgs);
        UiSystems.Party.SetTargetCallbacks(null, null, null);
        ClearResults();
    }

    internal virtual void DrawTextAtCursor(int x, int y)
    {
    }

    internal virtual bool LeftMouseButtonClicked(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool LeftMouseButtonReleased(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool RightMouseButtonClicked(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    /// <summary>
    /// The default behavior is to cancel.
    /// </summary>
    [TempleDllLocation(0x10135f60)]
    internal virtual bool RightMouseButtonReleased(IGameViewport viewport, MouseEvent e)
    {
        CancelPicker();
        return true;
    }

    internal virtual bool MiddleMouseButtonClicked(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool MiddleMouseButtonReleased(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool MouseMoved(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool MouseWheelScrolled(IGameViewport viewport, MouseEvent e)
    {
        return true;
    }

    internal virtual bool KeyDown(IGameViewport viewport, KeyboardEvent e)
    {
        return true;
    }

    internal virtual bool KeyUp(IGameViewport viewport, KeyboardEvent e)
    {
        return true;
    }

    #region Party UI Targetting Callbacks

    [TempleDllLocation(0x10136E60)]
    protected bool SelectSingleTargetCallback(GameObject obj)
    {
        if (obj != null)
        {
            ClearResults();
            SetSingleResult(obj);
            PickerState.Picker.DoExclusions();

            return FinalizePicker();
        }

        CancelPicker();
        return true;
    }

    [TempleDllLocation(0x10138590)]
    protected void PickerMultiSingleCheckFlags(GameObject target)
    {
        PickerState.Target = target;

        if (PickerState.Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st))
        {
            if (!PickerState.Picker.TargetValid(target))
            {
                PickerStatusFlags |= PickerStatusFlags.Invalid;
            }
        }
        else
        {
            if (!PickerState.Picker.CheckTargetVsIncFlags(target))
            {
                PickerStatusFlags |= PickerStatusFlags.Invalid;
            }
            else
            {
                if (PickerState.Picker.TargetValid(target))
                {
                    PickerStatusFlags &= ~PickerStatusFlags.Invalid;
                }
                else
                {
                    PickerStatusFlags |= PickerStatusFlags.Invalid;
                }
            }
        }

        if (!PickerState.Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired) &&
            PickerState.Picker.LosBlocked(target))
        {
            PickerStatusFlags |= PickerStatusFlags.Invalid;
        }

        if (PickerState.Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Range))
        {
            var casterLoc = PickerState.Picker.caster.GetLocation();
            var targetLoc = target.GetLocation();
            if (casterLoc.EstimateDistance(targetLoc) > PickerState.Picker.range)
            {
                PickerStatusFlags |= PickerStatusFlags.OutOfRange;
            }
        }
    }

    [TempleDllLocation(0x10136b90)]
    protected void ActivePickerReset(GameObject obj)
    {
        PickerState.Target = null;
        PickerStatusFlags &= ~(PickerStatusFlags.Invalid | PickerStatusFlags.OutOfRange);
    }

    #endregion

}