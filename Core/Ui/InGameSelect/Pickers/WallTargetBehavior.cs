using System.Diagnostics;
using System.Drawing;
using OpenTemple.Core.Location;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers
{
    internal class WallTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Wall;
        public override string Name => "SP_M_WALL";

        public WallState WallState { get; private set; }

        private LocAndOffsets _wallEndPoint;

        public WallTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            WallState = WallState.StartPoint;
        }

        internal override void DrawTextAtCursor(int x, int y)
        {
            const int cursorOffset = 22;
            var destRect = new Rectangle(x + cursorOffset, y + cursorOffset, 100, 13);

            switch (WallState)
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

            if (WallState == WallState.StartPoint)
            {
                WallState = WallState.EndPoint;
                Result.flags |= PickerResultFlags.PRF_HAS_LOCATION;
            }

            else if (WallState == WallState.EndPoint)
            {
                var mouseLoc = GameSystems.Location.ScreenToLocPrecise(args.X, args.Y);
                var mouseLocTrim =
                    GameSystems.Location.TrimToLength(Result.location, mouseLoc, Picker.trimmedRangeInches);
                _wallEndPoint = mouseLocTrim;

                return FinalizePicker();
            }

            else if (WallState == WallState.CenterPoint)
            {
                WallState = WallState.Radius;
            }

            else if (WallState == WallState.Radius)
            {
                return FinalizePicker();
            }

            return true;
        }

        internal override bool MouseMoved(MessageMouseArgs args)
        {
            ClearResults();

            if (WallState == WallState.StartPoint || WallState == WallState.CenterPoint)
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

            if (WallState == WallState.EndPoint)
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
            else if (WallState == WallState.Radius)
            {
                // todo
            }

            return false;
        }

        internal override bool RightMouseButtonReleased(MessageMouseArgs args)
        {
            ClearResults();

            if (WallState == WallState.StartPoint || WallState == WallState.CenterPoint)
            {
                Result.flags = PickerResultFlags.PRF_CANCELLED;
                Picker.callback?.Invoke(ref Result, PickerState.CallbackArgs);
                return true;
            }

            if (WallState == WallState.EndPoint)
            {
                WallState = WallState.StartPoint;
                _wallEndPoint = LocAndOffsets.Zero;
            }
            else if (WallState == WallState.Radius)
            {
                WallState = WallState.CenterPoint;
            }

            return true;
        }

    }

    public enum WallState
    {
        // wall targeting consists of two stages: start point stage and end point stage
        StartPoint = 0,
        EndPoint = 1,

        // circle targeting
        CenterPoint = 10,
        Radius = 11
    }

}