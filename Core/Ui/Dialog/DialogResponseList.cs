using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.Dialog;

internal class DialogResponseList : WidgetContainer
{
    public event Action<int> OnResponseSelected;

    public DialogResponseList(Rectangle rectangle) : base(rectangle)
    {
    }

    public void ClearResponses()
    {
        Clear();
    }

    public void SetResponses(IEnumerable<ResponseLine> responses)
    {
        Clear();

        var responseIndex = 0;
        foreach (var responseLine in responses)
        {
            // this was previously loaded from dlg_ui.mes file, but we ignore that now
            var numberText = $"{responseIndex + 1}.";
            var responseButton = new DialogResponseButton(
                new Rectangle(0, responseIndex * 25, 594, 23),
                numberText,
                responseLine.Text,
                responseLine.SkillUsed
            );
            var index = responseIndex;
            responseButton.AddClickListener(() => OnResponseSelected?.Invoke(index));
            Add(responseButton);
            responseIndex++;
        }
    }

}