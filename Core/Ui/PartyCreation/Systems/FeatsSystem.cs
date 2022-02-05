using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;

namespace OpenTemple.Core.Ui.PartyCreation.Systems;

[TempleDllLocation(0x102f7ac4)]
internal class FeatsSystem : IChargenSystem
{
    public string HelpTopic => "TAG_CHARGEN_FEATS";

    public ChargenStages Stage => ChargenStages.CG_Stage_Feats;

    public WidgetContainer Container { get; private set; }

    private CharEditorSelectionPacket _pkt;

    private bool mIsSelectingBonusFeat;
    private List<SelectableFeat> mBonusFeats;
    private bool mFeatsActivated;

    private List<SelectableFeat> mExistingFeats = new();
    private List<SelectableFeat> mSelectableFeats = new();
    private List<SelectableFeat> mMultiSelectFeats = new();
    private List<SelectableFeat> mMultiSelectMasterFeats = new();

    private readonly IComparer<SelectableFeat> _featComparer;

    private Dictionary<FeatId, string> featsMasterFeatStrings;

    [TempleDllLocation(0x101847f0)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:165")]
    public FeatsSystem()
    {
        var doc = WidgetDoc.Load("ui/pc_creation/feats_ui.json");
        Container = doc.GetRootContainer();
        Container.Visible = false;

        // TODO featsMasterFeatStrings

        _featComparer = Comparer<SelectableFeat>.Create((x, y) =>
        {
            var firstEnum = x.featEnum;
            var secEnum = y.featEnum;

            var firstName = GetFeatName(firstEnum);
            var secondName = GetFeatName(secEnum);

            return string.Compare(firstName, secondName, StringComparison.CurrentCultureIgnoreCase);
        });

        int result;
        int v2;
        string meslineValue;
        int meslineKey;

        // meslineKey = 19000;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_FeatsAvailable /*0x10c3a3ac*/ = (string) meslineValue;
        // TODO meslineKey = 19001;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_Feats /*0x10c38c08*/ = (string) meslineValue;
        // TODO meslineKey = 19002;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_ClassFeats /*0x10c39888*/ = (string) meslineValue;
        // TODO meslineKey = 19003;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_ClassBonusFeat /*0x10c37dec*/ = (string) meslineValue;
        // TODO meslineKey = 19200;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_Accept /*0x10c38ac0*/ = (string) meslineValue;
        // TODO meslineKey = 19201;
        // TODO GetLine_Safe /*0x101e65e0*/(pc_creationMes /*0x11e72ef0*/, &mesline);
        // TODO pcCreationLabel_Cancel /*0x10c3c148*/ = (string) meslineValue;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.textColor = &pcCreationDarkGreen /*0x102fd688*/;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.colors4 = &pcCreationDarkGreen /*0x102fd688*/;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.colors2 = &pcCreationDarkGreen /*0x102fd688*/;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.flags = 0x4000;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.field2c = -1;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.field0 = 0;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.kerning = 1;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.leading = 0;
        // TODO pcCreationClassFeatsBtnStyle /*0x10c39890*/.tracking = 3;
        // TODO stru_10C398E0 /*0x10c398e0*/.flags = 0x4000;
        // TODO stru_10C398E0 /*0x10c398e0*/.field2c = -1;
        // TODO stru_10C398E0 /*0x10c398e0*/.textColor = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C398E0 /*0x10c398e0*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C398E0 /*0x10c398e0*/.colors4 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C398E0 /*0x10c398e0*/.colors2 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C398E0 /*0x10c398e0*/.field0 = 0;
        // TODO stru_10C398E0 /*0x10c398e0*/.kerning = 1;
        // TODO stru_10C398E0 /*0x10c398e0*/.leading = 0;
        // TODO stru_10C398E0 /*0x10c398e0*/.tracking = 3;
        // TODO stru_10C38908 /*0x10c38908*/.flags = 0x4000;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.flags = 0x4000;
        // TODO stru_10C38960 /*0x10c38960*/.flags = 0x4000;
        // TODO dword_10C3C178 /*0x10c3c178*/ = 0x4000;
        // TODO stru_10C39838 /*0x10c39838*/.flags = 0x4000;
        // TODO stru_10C38908 /*0x10c38908*/.field2c = -1;
        // TODO stru_10C38908 /*0x10c38908*/.textColor = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38908 /*0x10c38908*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C38908 /*0x10c38908*/.colors4 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38908 /*0x10c38908*/.colors2 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38908 /*0x10c38908*/.field0 = 0;
        // TODO stru_10C38908 /*0x10c38908*/.kerning = 1;
        // TODO stru_10C38908 /*0x10c38908*/.leading = 0;
        // TODO stru_10C38908 /*0x10c38908*/.tracking = 3;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.field2c = -1;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.textColor = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.colors4 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.colors2 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.field0 = 0;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.kerning = 1;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.leading = 0;
        // TODO pcCreationClassFeatsStyle /*0x10c3ae00*/.tracking = 3;
        // TODO stru_10C38960 /*0x10c38960*/.field2c = -1;
        // TODO stru_10C38960 /*0x10c38960*/.textColor = (ColorRect*) &unk_102FD668 /*0x102fd668*/;
        // TODO stru_10C38960 /*0x10c38960*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C38960 /*0x10c38960*/.colors4 = (ColorRect*) &unk_102FD668 /*0x102fd668*/;
        // TODO stru_10C38960 /*0x10c38960*/.colors2 = (ColorRect*) &unk_102FD668 /*0x102fd668*/;
        // TODO stru_10C38960 /*0x10c38960*/.field0 = 0;
        // TODO stru_10C38960 /*0x10c38960*/.kerning = 1;
        // TODO stru_10C38960 /*0x10c38960*/.leading = 0;
        // TODO stru_10C38960 /*0x10c38960*/.tracking = 3;
        // TODO dword_10C3C17C /*0x10c3c17c*/ = -1;
        // TODO dword_10C3C184 /*0x10c3c184*/ = (int) &stru_102FD698 /*0x102fd698*/;
        // TODO dword_10C3C18C /*0x10c3c18c*/ = (int) &stru_102FD658 /*0x102fd658*/;
        // TODO dword_10C3C190 /*0x10c3c190*/ = (int) &stru_102FD698 /*0x102fd698*/;
        // TODO dword_10C3C188 /*0x10c3c188*/ = (int) &stru_102FD698 /*0x102fd698*/;
        // TODO dword_10C3C150 /*0x10c3c150*/ = 0;
        // TODO dword_10C3C158 /*0x10c3c158*/ = 1;
        // TODO dword_10C3C15C /*0x10c3c15c*/ = 0;
        // TODO dword_10C3C154 /*0x10c3c154*/ = 3;
        // TODO stru_10C39838 /*0x10c39838*/.field2c = -1;
        // TODO stru_10C39838 /*0x10c39838*/.textColor = &stru_102FD678 /*0x102fd678*/;
        // TODO stru_10C39838 /*0x10c39838*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C39838 /*0x10c39838*/.colors4 = &stru_102FD678 /*0x102fd678*/;
        // TODO stru_10C39838 /*0x10c39838*/.colors2 = &stru_102FD678 /*0x102fd678*/;
        // TODO stru_10C39838 /*0x10c39838*/.field0 = 0;
        // TODO stru_10C39838 /*0x10c39838*/.kerning = 1;
        // TODO stru_10C39838 /*0x10c39838*/.leading = 0;
        // TODO stru_10C39838 /*0x10c39838*/.tracking = 3;
        // TODO stru_10C38D08 /*0x10c38d08*/.flags = 16;
        // TODO stru_10C38D08 /*0x10c38d08*/.field2c = -1;
        // TODO stru_10C38D08 /*0x10c38d08*/.textColor = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38D08 /*0x10c38d08*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C38D08 /*0x10c38d08*/.colors4 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38D08 /*0x10c38d08*/.colors2 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38D08 /*0x10c38d08*/.field0 = 0;
        // TODO stru_10C38D08 /*0x10c38d08*/.kerning = 1;
        // TODO stru_10C38D08 /*0x10c38d08*/.leading = 0;
        // TODO stru_10C38D08 /*0x10c38d08*/.tracking = 3;
        // TODO stru_10C38A70 /*0x10c38a70*/.flags = 3088;
        // TODO stru_10C38A70 /*0x10c38a70*/.field2c = -1;
        // TODO stru_10C38A70 /*0x10c38a70*/.textColor = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38A70 /*0x10c38a70*/.shadowColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C38A70 /*0x10c38a70*/.colors4 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38A70 /*0x10c38a70*/.colors2 = (ColorRect*) &unk_102FD648 /*0x102fd648*/;
        // TODO stru_10C38A70 /*0x10c38a70*/.bgColor = &stru_102FD658 /*0x102fd658*/;
        // TODO stru_10C38A70 /*0x10c38a70*/.field0 = 0;
        // TODO stru_10C38A70 /*0x10c38a70*/.kerning = 1;
        // TODO stru_10C38A70 /*0x10c38a70*/.leading = 0;
        // TODO stru_10C38A70 /*0x10c38a70*/.tracking = 3;
        // TODO dword_10C3ADF8 /*0x10c3adf8*/ = Globals.UiAssets.LoadImg("art\\interface\\pc_creation\\meta_backdrop.img");
        // TODO if (dword_10C3ADF8 /*0x10c3adf8*/)
        // TODO {
        // TODO     v2 = a1.height;
        // TODO     result = UiPcCreationFeatsWidgetsInit /*0x10183e00*/(a1.width) != 0;
        // TODO }
        // TODO else
        // TODO {
        // TODO     result = 0;
        // TODO }

        // TODO return result;
    }

    [TempleDllLocation(0x10181f40)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:168")]
    public void Reset(CharEditorSelectionPacket selPkt)
    {
        _pkt = selPkt;

        mFeatsActivated = false;
        //mIsSelectingBonusFeat = false; // should not do this here, since then if a user goes back to skills and decreases/increases them, it can cause problems

        selPkt.feat0 = null;
        selPkt.feat1 = null;
        if (selPkt.classCode != Stat.level_ranger ||
            UiSystems.PCCreation.EditedChar.GetStat(Stat.level_ranger) != 1)
        {
            selPkt.feat2 = null;
        }

        mExistingFeats.Clear();
        mSelectableFeats.Clear();
        mMultiSelectFeats.Clear();
    }

    [TempleDllLocation(0x10182a30)]
    [TemplePlusLocation("ui_pc_creation_hooks.cpp:167")]
    public void Activate()
    {
        mFeatsActivated = true;

        var handle = UiSystems.PCCreation.EditedChar;

        mIsSelectingBonusFeat = D20ClassSystem.IsSelectingFeatsOnLevelup(handle, _pkt.classCode);

        if (mIsSelectingBonusFeat)
        {
            mBonusFeats.Clear();
            mBonusFeats.AddRange(D20ClassSystem.LevelupGetBonusFeats(handle, _pkt.classCode));
        }

        mExistingFeats.Clear();
        foreach (var featId in GameSystems.Feat.FeatListGet(handle, _pkt.classCode))
        {
            if (_pkt.feat0 != featId && _pkt.feat1 != featId && _pkt.feat2 != featId)
            {
                mExistingFeats.Add(new SelectableFeat(featId));
            }
        }

        mExistingFeats.Sort(_featComparer);

        // TODO featsExistingScrollbar = *uiManager->GetScrollBar(featsExistingScrollbarId);
        // TODO featsExistingScrollbar.scrollbarY = 0;
        // TODO featsExistingScrollbarY = 0;
        // TODO featsExistingScrollbar.yMax = max((int)mExistingFeats.size() - FEATS_EXISTING_BTN_COUNT, 0);
        // TODO *uiManager->GetScrollBar(featsExistingScrollbarId) = featsExistingScrollbar;

        // Available feats
        mSelectableFeats.Clear();
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
            mSelectableFeats.Add(new SelectableFeat(feat));
        }

        foreach (var feat in GameSystems.Feat.NewFeats)
        {
            if (!GameSystems.Feat.IsFeatEnabled(feat) && !GameSystems.Feat.IsFeatMultiSelectMaster(feat))
                continue;
            if (!Globals.Config.nonCoreMaterials && GameSystems.Feat.IsNonCore(feat))
                continue;
            if (IsClassBonusFeat(feat))
            {
                mSelectableFeats.Add(new SelectableFeat(feat));
                continue;
            }

            if (GameSystems.Feat.IsFeatRacialOrClassAutomatic(feat))
                continue;
            if (GameSystems.Feat.IsFeatPartOfMultiselect(feat))
                continue;
            if (feat == FeatId.NONE)
                continue;

            mSelectableFeats.Add(new SelectableFeat(feat));
        }

        mSelectableFeats.Sort(_featComparer);

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
        return mIsSelectingBonusFeat;
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
            return featsMasterFeatStrings[feat];

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