using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation;

public interface IChargenSystem : IDisposable
{
    string HelpTopic { get; }

    void Reset(CharEditorSelectionPacket pkt)
    {
    }

    void Activate()
    {
    }

    void Resize(Size resizeArgs)
    {
    }

    void Show()
    {
        Container.Visible = true;
    }

    void Hide()
    {
        Container.Visible = false;
    }

    // checks if the char editing stage is complete (thus allowing you to move on to the next stage). This is checked at every render call.
    bool CheckComplete()
    {
        return true;
    }

    void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
    }

    void ButtonExited()
    {
    }

    void IDisposable.Dispose()
    {
    }

    WidgetContainer Container { get; }

    ChargenStages Stage { get; }

    string ButtonLabel => $"#{{pc_creation:{(int) Stage}}}";

    bool CompleteForTesting(Dictionary<string, object> props) => false;
}