using System.Drawing;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.CharSheet.Spells.Metamagic;

public class MetamagicFeatButton : WidgetButtonBase
{
    public FeatId Feat { get; }

    public bool Applied { get; }

    public MetamagicFeatButton(bool applied, FeatId feat, Rectangle rect) : base(rect)
    {
        Feat = feat;
        Applied = applied;
        AddContent(new WidgetText(GameSystems.Feat.GetFeatName(feat), "metamagic-feat-button"));
    }
}
