#nullable enable
using System;
using OpenTemple.Core.Ui.Styles;

namespace OpenTemple.Core.Ui.FlowModel;

public interface IFlowContentHost : IStyleable
{
    void NotifyStyleChanged();

    void NotifyTextFlowChanged();
}

/// <summary>
/// Basic implementation of a flow content host.
/// </summary>
public class FlowContentHost : Styleable, IFlowContentHost
{
    private IStyleable? _styleParent;

    public override IStyleable? StyleParent => _styleParent;

    public void SetStyleParent(IStyleable? parent)
    {
        _styleParent = parent;
        NotifyStyleChanged();
    }

    public event Action? OnStyleChanged;

    public event Action? OnTextFlowChanged;

    public void NotifyStyleChanged() => OnStyleChanged?.Invoke();

    public void NotifyTextFlowChanged() => OnTextFlowChanged?.Invoke();
}