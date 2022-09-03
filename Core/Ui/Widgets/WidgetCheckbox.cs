using System;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace OpenTemple.Core.Ui.Widgets;

public class WidgetCheckbox : WidgetButtonBase
{
    private readonly WidgetImage _checkedImage;

    private readonly WidgetImage _uncheckedImage;

    public bool Checked { get; set; }

    public event Action<bool> OnCheckedChanged;

    public WidgetCheckbox(int x, int y,
        [CallerFilePath]
        string? filePath = null, [CallerLineNumber]
        int lineNumber = -1)
        : base(new Rectangle(x, y, 0, 0), filePath, lineNumber)
    {
        _checkedImage = new WidgetImage("art/interface/options_ui/checkbox_on.tga");
        AddContent(_checkedImage);

        _uncheckedImage = new WidgetImage("art/interface/options_ui/checkbox_off.tga");
        AddContent(_uncheckedImage);

        AddClickListener(() =>
        {
            Checked = !Checked;
            OnCheckedChanged?.Invoke(Checked);
        });

        SetSize(_checkedImage.GetPreferredSize());
    }

    public override void Render()
    {
        _checkedImage.Visible = Checked;
        _uncheckedImage.Visible = !Checked;

        base.Render();
    }
}