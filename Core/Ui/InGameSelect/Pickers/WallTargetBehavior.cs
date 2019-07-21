using System.Diagnostics;
using System.Drawing;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui.InGameSelect.Pickers
{
    internal class WallTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Wall;
        public override string Name => "SP_M_WALL";

        private WallState _wallState;

        private LocAndOffsets _wallEndPoint;

        public WallTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            _wallState = WallState.StartPoint;
        }

        internal override void DrawTextAtCursor(int x, int y)
        {
            const int cursorOffset = 22;
            var destRect = new Rectangle(x + cursorOffset, y + cursorOffset, 100, 13);

            switch (_wallState)
            {
                case WallState.StartPoint:
                    Tig.Fonts.RenderText("Start Point", destRect, TigTextStyle.standardWhite);
                    break;
                case WallState.EndPoint:
                    Tig.Fonts.RenderText("End Point", destRect, TigTextStyle.standardWhite);
                    break;
                case WallState.CenterPoint:
                    Tig.Fonts.RenderText("Center Point", destRect, TigTextStyle.standardWhite);
                    break;
                case WallState.Radius:
                    Tig.Fonts.RenderText("Ring Radius", destRect, TigTextStyle.standardWhite);
                    break;
            }
        }

        internal override bool LeftMouseButtonClicked(MessageMouseArgs args)
        {
            return true;
        }

        internal override bool LeftMouseButtonReleased(MessageMouseArgs args)
        {
            MouseMoved(args);

            if (_wallState == WallState.StartPoint)
            {
                _wallState = WallState.EndPoint;
                Result.flags |= PickerResultFlags.PRF_HAS_LOCATION;
            }

            else if (_wallState == WallState.EndPoint)
            {
                var mouseLoc = GameSystems.Location.ScreenToLocPrecise(args.X, args.Y);
                var mouseLocTrim =
                    GameSystems.Location.TrimToLength(Result.location, mouseLoc, Picker.trimmedRangeInches);
                _wallEndPoint = mouseLocTrim;

                return FinalizePicker();
            }

            else if (_wallState == WallState.CenterPoint)
            {
                _wallState = WallState.Radius;
            }

            else if (_wallState == WallState.Radius)
            {
                return FinalizePicker();
            }

            return true;
        }

        internal override bool MouseMoved(MessageMouseArgs args)
        {
            ClearResults();

            if (_wallState == WallState.StartPoint || _wallState == WallState.CenterPoint)
            {
                // get startpoint location from mouse
                SetResultLocationFromMouse(args);
                return true;
            }

            var mouseLoc = GameSystems.Location.ScreenToLocPrecise(args.X, args.Y);
            Trace.Assert(Result.HasLocation);

            var maxRange = (float) (Picker.range * locXY.INCH_PER_FEET);
            var dist = Result.location.DistanceTo(mouseLoc);
            if (maxRange > dist)
            {
                maxRange = dist;
            }

            if (_wallState == WallState.EndPoint)
            {
                // get radius and range up to mouse (trimmed by walls and such)
                var radiusInch = Picker.radiusTarget * locXY.INCH_PER_FEET / 2.0f;
                Picker.GetTrimmedRange(Result.location, mouseLoc, radiusInch, maxRange, locXY.INCH_PER_FEET * 5.0f);
                // TODO: the 2.35 might be unneeded
                Picker.degreesTarget =
                    2.3561945f -
                    Result.location.RotationTo(mouseLoc); // putting this in radians, unlike the usual usage
                Picker.GetTargetsInPath(Result.location, mouseLoc, radiusInch);
                Picker.DoExclusions();
                return true;
            }
            else if (_wallState == WallState.Radius)
            {
                // todo
            }

            return false;
        }

        internal override bool RightMouseButtonReleased(MessageMouseArgs args)
        {
            ClearResults();

            if (_wallState == WallState.StartPoint || _wallState == WallState.CenterPoint)
            {
                Result.flags = PickerResultFlags.PRF_CANCELLED;
                Picker.callback?.Invoke(ref Result, PickerState.CallbackArgs);
                return true;
            }

            if (_wallState == WallState.EndPoint)
            {
                _wallState = WallState.StartPoint;
                _wallEndPoint = LocAndOffsets.Zero;
            }
            else if (_wallState == WallState.Radius)
            {
                _wallState = WallState.CenterPoint;
            }

            return true;
        }

        private enum WallState
        {
            // wall targeting consists of two stages: start point stage and end point stage
            StartPoint = 0,
            EndPoint = 1,

            // circle targeting
            CenterPoint = 10,
            Radius = 11
        }
    }
}