using System;

namespace OpenTemple.Core.Ui.Widgets;

/// <summary>
/// Cause for layout update
/// </summary>
[Flags]
public enum LayoutChangeFlag
{
    OwnSize = 1,
    OwnPosition = 2,
    Style = 4,
    Content = 8
}
