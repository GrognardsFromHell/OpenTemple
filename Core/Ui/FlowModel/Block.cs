using OpenTemple.Core.Ui.Styles;



namespace OpenTemple.Core.Ui.FlowModel;

public abstract class Block : Styleable
{
    public IFlowContentHost? Host { get; set; }

    public override IStyleable? StyleParent => Host;
}
