using System;
using System.Drawing;
using OpenTemple.Core.GFX;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

public class AbilityScoreValueWidget : WidgetBase
{
    private const string LabelStyle = "charGenAssignedStat";

    private readonly Func<int> _valueGetter;

    private readonly Action<int> _valueSetter;

    private readonly WidgetText _label;

    public int Value
    {
        get => _valueGetter();
        set => _valueSetter(value);
    }

    public bool IsDragging { get; set; }

    public bool IsAssigned { get; }

    public AbilityScoreValueWidget(Size size, Func<int> valueGetter, Action<int> valueSetter, bool assigned)
    {
        _valueGetter = valueGetter;
        _valueSetter = valueSetter;
        IsAssigned = assigned;

        Size = size;
        AddContent(new WidgetRectangle
        {
            Pen = new PackedLinearColorA(assigned ? 0xFF43586E : 0xFFF6FF04)
        });
        _label = new WidgetText("", LabelStyle);
        AddContent(_label);
    }

    public override void Render()
    {
        var currentStatValue = _valueGetter();
        if (IsDragging || currentStatValue == -1)
        {
            _label.Visible = false;
        }
        else
        {
            _label.Visible = true;
            _label.Text = currentStatValue.ToString();
        }

        base.Render();
    }

}