using OpenTemple.Core.Ui.Styles;

#nullable enable

namespace OpenTemple.Core.Ui.FlowModel;

public class Block : Styleable
{
    public IFlowContentHost? Host { get; set; }

    public override IStyleable? StyleParent => Host;
}