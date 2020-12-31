using OpenTemple.Core.GameObject;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;

namespace OpenTemple.Core.Ui.InGameSelect.Pickers
{
    internal class ConeTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.Cone;

        public override string Name => "SP_M_CONE";

        [TempleDllLocation(0x10138a00)]
        public ConeTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            UiSystems.Party.SetTargetCallbacks(
                TargetPartyMember, SetPartyMemberTarget, ResetPartyMemberTarget
            );
        }

        [TempleDllLocation(0x101386C0)]
        private bool TargetPartyMember(GameObjectBody partyMember)
        {
            PickerState.Target = partyMember;
            Picker.SetConeTargets(partyMember.GetLocationFull());
            return FinalizePicker();
        }

        [TempleDllLocation(0x10136bc0)]
        private void SetPartyMemberTarget(GameObjectBody partyMember)
        {
            PickerState.Target = partyMember;
            ClearResults();
            SetResultLocation(partyMember);
        }

        [TempleDllLocation(0x10136c40)]
        private void ResetPartyMemberTarget(GameObjectBody obj)
        {
            PickerState.Target = null;
            ClearResults();
        }

        [TempleDllLocation(0x101380b0)]
        internal override bool MouseMoved(IGameViewport viewport, MessageMouseArgs args)
        {
            var location = GameSystems.Location.ScreenToLocPrecise(args.X, args.Y);
            Picker.SetConeTargets(location);
            return false;
        }

        [TempleDllLocation(0x10138130)]
        internal override bool LeftMouseButtonReleased(IGameViewport viewport, MessageMouseArgs args)
        {
            MouseMoved(viewport, args);
            return FinalizePicker();
        }
    }
}