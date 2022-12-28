using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7ac4)]
internal class FeatsSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_FEATS";

    public ChargenStage Stage => ChargenStage.Feats;

    public WidgetContainer Container { get; }

    private CharEditorSelectionPacket _pkt;

    private bool _isSelectingBonusFeat;
    private List<SelectableFeat> _bonusFeats;
    private bool _featsActivated;

    private List<SelectableFeat> _existingFeats = new();
    private List<SelectableFeat> _selectableFeats = new();
    private List<SelectableFeat> _multiSelectFeats = new();
    private List<SelectableFeat> _multiSelectMasterFeats = new();

    private readonly IComparer<SelectableFeat> _featComparer;

    private Dictionary<FeatId, string> _featsMasterFeatStrings;

    private readonly DraggableItemList<SelectableFeat> _featList;

    [TempleDllLocation(0x101847f0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:165")]
    public FeatsSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/feats_ui.json");
        Container = doc.GetRootContainer();

        // TODO featsMasterFeatStrings

        _featComparer = Comparer<SelectableFeat>.Create((x, y) =>
        {
            var firstEnum = x.featEnum;
            var secEnum = y.featEnum;

            var firstName = GetFeatName(firstEnum);
            var secondName = GetFeatName(secEnum);

            return string.Compare(firstName, secondName, StringComparison.CurrentCultureIgnoreCase);
        });

        _featList = new DraggableItemList<SelectableFeat>
        {
            PixelSize = new SizeF(300, 200),
            TextFactory = feat => new SimpleInlineElement(GetFeatName(feat.featEnum))
        };
        Container.Add(_featList);
    }

    [TempleDllLocation(0x10181f40)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:168")]
    public void Reset(CharEditorSelectionPacket selPkt)
    {
        _pkt = selPkt;

        _featsActivated = false;
        //mIsSelectingBonusFeat = false; // should not do this here, since then if a user goes back to skills and decreases/increases them, it can cause problems

        selPkt.feat0 = null;
        selPkt.feat1 = null;
        if (selPkt.classCode != Stat.level_ranger ||
            UiSystems.PCCreation.EditedChar.GetStat(Stat.level_ranger) != 1)
        {
            selPkt.feat2 = null;
        }

        _existingFeats.Clear();
        _selectableFeats.Clear();
        _multiSelectFeats.Clear();
    }

    [TempleDllLocation(0x10182a30)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:167")]
    public void Activate()
    {
        _featsActivated = true;

        var handle = UiSystems.PCCreation.EditedChar;

        _isSelectingBonusFeat = D20ClassSystem.IsSelectingFeatsOnLevelup(handle, _pkt.classCode);

        if (_isSelectingBonusFeat)
        {
            _bonusFeats.Clear();
            _bonusFeats.AddRange(D20ClassSystem.LevelupGetBonusFeats(handle, _pkt.classCode));
        }

        _existingFeats.Clear();
        foreach (var featId in GameSystems.Feat.FeatListGet(handle, _pkt.classCode))
        {
            if (_pkt.feat0 != featId && _pkt.feat1 != featId && _pkt.feat2 != featId)
            {
                _existingFeats.Add(new SelectableFeat(featId));
            }
        }

        _existingFeats.Sort(_featComparer);

        // TODO featsExistingScrollbar = *uiManager->GetScrollBar(featsExistingScrollbarId);
        // TODO featsExistingScrollbar.scrollbarY = 0;
        // TODO featsExistingScrollbarY = 0;
        // TODO featsExistingScrollbar.yMax = max((int)mExistingFeats.size() - FEATS_EXISTING_BTN_COUNT, 0);
        // TODO *uiManager->GetScrollBar(featsExistingScrollbarId) = featsExistingScrollbar;

        // Available feats
        _selectableFeats.Clear();
        for (var i = 0; i < (int) FeatId.NONE; i++)
        {
            var feat = (FeatId) i;
            if (!GameSystems.Feat.IsFeatEnabled(feat) && !GameSystems.Feat.IsFeatMultiSelectMaster(feat))
                continue;
            if (GameSystems.Feat.IsFeatRacialOrClassAutomatic(feat))
                continue;
            if (GameSystems.Feat.IsFeatPartOfMultiselect(feat))
                continue;
            if (feat == FeatId.NONE)
                continue;
            _selectableFeats.Add(new SelectableFeat(feat));
        }

        foreach (var feat in GameSystems.Feat.NewFeats)
        {
            if (!GameSystems.Feat.IsFeatEnabled(feat) && !GameSystems.Feat.IsFeatMultiSelectMaster(feat))
                continue;
            if (!Globals.Config.nonCoreMaterials && GameSystems.Feat.IsNonCore(feat))
                continue;
            if (IsClassBonusFeat(feat))
            {
                _selectableFeats.Add(new SelectableFeat(feat));
                continue;
            }

            if (GameSystems.Feat.IsFeatRacialOrClassAutomatic(feat))
                continue;
            if (GameSystems.Feat.IsFeatPartOfMultiselect(feat))
                continue;
            if (feat == FeatId.NONE)
                continue;

            _selectableFeats.Add(new SelectableFeat(feat));
        }

        _selectableFeats.Sort(_featComparer);

        _featList.Items = _selectableFeats;
        // TODO featsScrollbar = *uiManager->GetScrollBar(featsScrollbarId);
        // TODO featsScrollbar.scrollbarY = 0;
        // TODO featsScrollbarY = 0;
        // TODO featsScrollbar.yMax = max((int)mSelectableFeats.size() - FEATS_AVAIL_BTN_COUNT, 0);
        // TODO *uiManager->GetScrollBar(featsScrollbarId) = featsScrollbar;
    }

    [TempleDllLocation(0x10181fa0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:173")]
    public bool CheckComplete()
    {
        // is a 3rd level and no feat chosen
        if (IsSelectingNormalFeat() && !_pkt.feat0.HasValue)
            return false;

        if (IsSelectingSecondFeat() && !_pkt.feat1.HasValue)
            return false;

        // the logic will be handled in the msg callbacks & Python API now
        if (IsSelectingBonusFeat() && !_pkt.feat2.HasValue)
            return false;

        return true;
    }

    private bool IsSelectingNormalFeat()
    {
        return true;
    }

    private bool IsSelectingSecondFeat()
    {
        return D20RaceSystem.BonusFirstLevelFeat(_pkt.raceId.GetValueOrDefault());
    }

    private bool IsSelectingBonusFeat()
    {
        return _isSelectingBonusFeat;
    }

    private bool IsClassBonusFeat(FeatId feat)
    {
        // TODO return chargen.IsClassBonusFeat(feat);
        throw new NotImplementedException();
    }

    private bool IsBonusFeatDisregardingPrereqs(FeatId feat)
    {
        // TODO return chargen.IsBonusFeatDisregardingPrereqs(feat);
        throw new NotImplementedException();
    }

    [TempleDllLocation(0x10181fe0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:172")]
    public void Finalize(CharEditorSelectionPacket charSpec, ref GameObject playerObj)
    {
        if (charSpec.feat0.HasValue)
        {
            GameSystems.Feat.AddFeat(playerObj, charSpec.feat0.Value);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
        }

        if (charSpec.feat1.HasValue)
        {
            GameSystems.Feat.AddFeat(playerObj, charSpec.feat1.Value);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
        }

        if (charSpec.feat2.HasValue)
        {
            GameSystems.Feat.AddFeat(playerObj, charSpec.feat2.Value);
            GameSystems.D20.Status.D20StatusRefresh(playerObj);
        }
    }

    [TempleDllLocation(0x10182070)]
    private void UpdateDescriptionBox()
    {
        UiSystems.PCCreation.ShowHelpTopic(HelpTopic);
    }

    private string GetFeatName(FeatId feat)
    {
        if (feat >= FeatId.EXOTIC_WEAPON_PROFICIENCY && feat <= FeatId.GREATER_WEAPON_FOCUS)
            return _featsMasterFeatStrings[feat];

        return GameSystems.Feat.GetFeatName(feat);
    }
}

public struct SelectableFeat
{
    public FeatId featEnum;
    public int minLevel;

    public bool IsAutomaticClassFeat { get; set; }
    public bool IsBonusSelectableFeat { get; set; }
    public bool IsIgnoreRequirements { get; set; }

    public SelectableFeat(FeatId featEnum)
    {
        this.featEnum = featEnum;
        minLevel = 1;
        IsAutomaticClassFeat = false;
        IsBonusSelectableFeat = false;
        IsIgnoreRequirements = false;
    }

    public SelectableFeat(string featEnum) : this((FeatId) ElfHash.Hash(featEnum))
    {
    }
}