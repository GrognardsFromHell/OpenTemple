using SpicyTemple.Core.Platform;
using SpicyTemple.Core.Systems;

namespace SpicyTemple.Core.Ui.InGameSelect.Pickers
{
    internal class PersonalTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Personal;

        public override string Name => "SP_M_PERSONAL";

        public PersonalTargetBehavior(PickerState pickerState) : base(pickerState)
        {
        }

        [TempleDllLocation(0x10135fd0)]
        internal override bool LeftMouseButtonReleased(MessageMouseArgs args)
        {
            ClearResults();

            var raycastFlags = PickerState.GetFlagsFromExclusions();
            if (!GameSystems.Raycast.PickObjectOnScreen(args.X, args.Y, out var target, raycastFlags) ||
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
        internal override bool MouseMoved(MessageMouseArgs args)
        {
            ClearResults();
            PickerStatusFlags |= PickerStatusFlags.Invalid;

            var raycastFlags = PickerState.GetFlagsFromExclusions();
            GameSystems.Raycast.PickObjectOnScreen(args.X, args.Y, out var target, raycastFlags);

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
}