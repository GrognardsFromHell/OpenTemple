using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers
{
    internal class MultiTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Multi;

        public override string Name => "SP_M_MULTI";

        [TempleDllLocation(0x101389e0)]
        public MultiTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            UiSystems.Party.SetTargetCallbacks(
                PickerMultiHandleTargetSelection,
                PickerMultiSingleCheckFlags,
                ActivePickerReset
            );
        }

        [TempleDllLocation(0x100b9ba0)]
        private bool AlreadyHasTarget(GameObjectBody target)
        {
            if (target == null)
            {
                return false;
            }

            ref var results = ref Result;
            if (results.flags == default)
            {
                return false;
            }

            if (results.flags.HasFlag(PickerResultFlags.PRF_HAS_SINGLE_OBJ) && results.handle == target)
            {
                return true;
            }

            if (!results.flags.HasFlag(PickerResultFlags.PRF_HAS_MULTI_OBJ))
            {
                return false;
            }

            foreach (var currentTarget in results.objList)
            {
                if (currentTarget == target)
                {
                    return true;
                }
            }

            return false;
        }

        [TempleDllLocation(0x10136f50)]
        private void AddTarget(GameObjectBody obj)
        {
            if (Result.HasMultipleResults)
            {
                Result.objList.Add(obj);
            }
            else if (Result.HasSingleResult)
            {
                var previousTarget = Result.handle;
                ClearResults();
                Result.flags = PickerResultFlags.PRF_HAS_MULTI_OBJ;
                Result.objList = new List<GameObjectBody> {previousTarget, obj};
            }
            else
            {
                ClearResults();
                Result.flags = PickerResultFlags.PRF_HAS_SINGLE_OBJ;
                Result.handle = obj;
            }
        }

        [TempleDllLocation(0x10138370)]
        private bool PickerMultiHandleTargetSelection(GameObjectBody handle)
        {
            if (!CanAddTarget(handle))
            {
                return false;
            }

            AddTarget(handle);
            ++PickerState.tgtIdx;

            // TODO: Should be replaced with a flag on the current picker (no ->UI calls!)
            UiSystems.InGameSelect.ShowConfirmSelectionButton(Picker.caster);

            if (Result.TargetCount < Picker.maxTargets)
            {
                return false;
            }

            // have selected all target slots
            UiSystems.InGameSelect.HideConfirmSelectionButton();

            PickerState.Target = null;
            PickerState.tgtIdx = 0;
            return FinalizePicker();
        }

        private bool CanAddTarget(GameObjectBody handle)
        {
            if (Picker.modeTarget.HasFlag(UiPickerType.OnceMulti) && AlreadyHasTarget(handle))
            {
                // Only allow unique targets
                return false;
            }

            // Check whether the target is within 30 feet of other targets if requested
            if (Picker.modeTarget.HasFlag(UiPickerType.Any30Feet))
            {
                if (Result.HasSingleResult)
                {
                    if (Result.handle.DistanceToObjInFeet(handle) > 30.0f)
                    {
                        return false;
                    }
                }
                else if (Result.HasMultipleResults)
                {
                    foreach (var currentTarget in Result.objList)
                    {
                        if (currentTarget.DistanceToObjInFeet(handle) > 30.0f)
                        {
                            return false;
                        }
                    }
                }
            }

            // Check whether the target is within 30 feet of the first target if requested
            if (Picker.modeTarget.HasFlag(UiPickerType.Primary30Feet))
            {
                var primaryTarget = Result.FirstTarget;
                if (primaryTarget != null && primaryTarget.DistanceToObjInFeet(handle) > 30.0f)
                {
                    return false;
                }
            }

            if (Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st) && !Picker.TargetValid(handle))
            {
                return false;
            }

            if (!Picker.CheckTargetVsIncFlags(handle)
                || !Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Exclude1st) && !Picker.TargetValid(handle))
            {
                return false;
            }

            if (!Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.LosNotRequired) && Picker.LosBlocked(handle))
            {
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x10137000)]
        internal override void DrawTextAtCursor(float x, float y)
        {
            // This dirty hack will render tooltips and THEN the text over them...
            var widget = Globals.UiManager.GetWidgetAt(x, y);
            widget?.RenderTooltip(x, y);

            var text = $"{PickerState.tgtIdx + 1}/{Picker.maxTargets}";

            var style = UiSystems.InGameSelect.GetTextStyle();

            var metrics = new TigFontMetrics();
            Tig.Fonts.Measure(style, text, ref metrics);

            var extents = new RectangleF(
                x + 32,
                y + 32,
                metrics.width,
                metrics.height
            );
            Tig.Fonts.RenderText(text, extents, style);
        }

        [TempleDllLocation(0x10137da0)]
        internal override bool KeyStateChanged(MessageKeyStateChangeArgs args)
        {
            // Releasing space bar will end the picker
            if (args.key == DIK.DIK_SPACE && !args.down)
            {
                return ForceEnd();
            }

            return false;
        }

        [TempleDllLocation(0x10136810)]
        public bool ForceEnd()
        {
            if (PickerState.tgtIdx != 0)
            {
                PickerState.Target = null;
                PickerState.tgtIdx = 0;
                return FinalizePicker();
            }
            else
            {
                CancelPicker();
                return true;
            }
        }

        [TempleDllLocation(0x10137a70)]
        internal override bool LeftMouseButtonReleased(MessageMouseArgs args)
        {
            if (HandleClickInUnexploredArea(args.X, args.Y, false))
            {
                return false;
            }

            var raycastFlags = PickerState.GetFlagsFromExclusions();
            if (!GameSystems.Raycast.PickObjectOnScreen(args.X, args.Y, out var objFound, raycastFlags))
            {
                return true;
            }

            if (!CanAddTarget(objFound))
            {
                return true;
            }

            AddTarget(objFound);
            PickerState.tgtIdx++;

            UiSystems.InGameSelect.ShowConfirmSelectionButton(Picker.caster);

            if (Result.TargetCount < Picker.maxTargets)
            {
                return true;
            }

            // have selected all target slots
            UiSystems.InGameSelect.HideConfirmSelectionButton();

            PickerState.Target = null;
            PickerState.tgtIdx = 0;
            return FinalizePicker();
        }

        [TempleDllLocation(0x10138170)]
        internal override bool MouseMoved(MessageMouseArgs args)
        {
            if (HandleClickInUnexploredArea(args.X, args.Y, false))
            {
                // Not resetting targets here is a change vs. vanilla
                return false;
            }

            var raycastFlags = PickerState.GetFlagsFromExclusions();
            if (!GameSystems.Raycast.PickObjectOnScreen(args.X, args.Y, out var target, raycastFlags))
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

        [TempleDllLocation(0x10136790)]
        internal override bool RightMouseButtonReleased(MessageMouseArgs args)
        {
            if (PickerState.tgtIdx != 0)
            {
                UiSystems.InGameSelect.HideConfirmSelectionButton();

                PickerState.Target = null;
                PickerState.tgtIdx = 0;
                ClearResults();
                return true;
            }

            return base.RightMouseButtonReleased(args);
        }
    }
}