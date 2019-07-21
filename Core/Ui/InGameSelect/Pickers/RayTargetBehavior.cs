using System.Collections.Generic;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems.Raycast;

namespace SpicyTemple.Core.Ui.InGameSelect.Pickers
{
    internal class RayTargetBehavior : PickerBehavior
    {
        public RayTargetBehavior(PickerState pickerState) : base(pickerState)
        {
        }

        public override UiPickerType Type => UiPickerType.Ray;
        public override string Name => "SP_M_RAY";

        [TempleDllLocation(0x10136ad0)]
        internal override bool LeftMouseButtonReleased(MessageMouseArgs args)
        {
            MouseMoved(args);
            return FinalizePicker();
        }

        [TempleDllLocation(0x10136860)]
        internal override bool MouseMoved(MessageMouseArgs args)
        {
            ClearResults();

            SetResultLocationFromMouse(args);
            if (Picker.flagsTarget.HasFlag(UiPickerFlagsTarget.Radius))
            {
                TrimRangeOfPicker(Result.location);

                using var rayPkt = CreateRaycast(Result.location);
                rayPkt.Raycast();

                foreach (var resultItem in rayPkt)
                {
                    if (resultItem.obj != null)
                    {
                        if (Result.objList == null)
                        {
                            Result.objList = new List<GameObjectBody>();
                        }

                        Result.objList.Add(resultItem.obj);
                    }
                }

                if (Result.objList != null)
                {
                    Result.flags |= PickerResultFlags.PRF_HAS_MULTI_OBJ;
                }
            }

            Picker.DoExclusions();
            return true;
        }

        // Trim the range of the Picker so it stops at the first blocker of the given raycast
        private void TrimRangeOfPicker(LocAndOffsets targetLocation)
        {
            Picker.trimmedRangeInches = Picker.range * locXY.INCH_PER_FEET;
            var rayPkt = CreateRaycast(targetLocation);
            rayPkt.Raycast();
            foreach (var resultItem in rayPkt)
            {
                if (resultItem.obj == null)
                {
                    var dist = rayPkt.origin.DistanceTo(resultItem.loc);
                    if (dist < Picker.trimmedRangeInches)
                    {
                        Picker.trimmedRangeInches = dist;
                    }
                }
            }
        }

        private RaycastPacket CreateRaycast(LocAndOffsets targetLocation)
        {
            var rayPkt = new RaycastPacket();
            rayPkt.flags |= RaycastFlag.HasRangeLimit | RaycastFlag.ExcludeItemObjects | RaycastFlag.HasSourceObj |
                            RaycastFlag.HasRadius;
            rayPkt.sourceObj = Picker.caster;
            rayPkt.origin = Picker.caster.GetLocationFull();
            rayPkt.rayRangeInches = Picker.trimmedRangeInches;
            rayPkt.radius = Picker.radiusTarget * locXY.INCH_PER_FEET / 2.0f;
            rayPkt.targetLoc = targetLocation;
            return rayPkt;
        }
    }
}