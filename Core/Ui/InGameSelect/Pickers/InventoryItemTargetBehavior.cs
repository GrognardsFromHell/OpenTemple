
namespace SpicyTemple.Core.Ui.InGameSelect.Pickers
{
    internal class InventoryItemTargetBehavior : PickerBehavior
    {
        public override UiPickerType Type => UiPickerType.InventoryItem;

        public override string Name => "SP_M_INVENTORY_ITEM";

        [TempleDllLocation(0x10138790)]
        public InventoryItemTargetBehavior(PickerState pickerState) : base(pickerState)
        {
            UiSystems.CharSheet.ShowItemPicker(Picker.caster, SelectSingleTargetCallback);
        }

    }
}