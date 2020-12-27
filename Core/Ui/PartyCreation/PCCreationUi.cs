using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using OpenTemple.Core.GameObject;
using OpenTemple.Core.IO;
using OpenTemple.Core.Location;
using OpenTemple.Core.Logging;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.D20;
using OpenTemple.Core.Systems.D20.Classes;
using OpenTemple.Core.Systems.Feats;
using OpenTemple.Core.Systems.RollHistory;
using OpenTemple.Core.Systems.Spells;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet.Portrait;
using OpenTemple.Core.Ui.MainMenu;
using OpenTemple.Core.Ui.PartyCreation.Systems;
using OpenTemple.Core.Ui.Widgets;

namespace OpenTemple.Core.Ui.PartyCreation
{
    public class PCCreationUi : IDisposable
    {
        private static readonly ILogger Logger = LoggingSystem.CreateLogger();

        // TODO: Provide a way to customize. Co8 uses starting map 5107 for all alignments
        private static readonly Dictionary<Alignment, int> StartMaps = new Dictionary<Alignment, int>
        {
            {Alignment.TRUE_NEUTRAL, 5100},
            {Alignment.LAWFUL_NEUTRAL, 5103},
            {Alignment.CHAOTIC_NEUTRAL, 5104},
            {Alignment.NEUTRAL_GOOD, 5101},
            {Alignment.LAWFUL_GOOD, 5096},
            {Alignment.CHAOTIC_GOOD, 5097},
            {Alignment.NEUTRAL_EVIL, 5102},
            {Alignment.LAWFUL_EVIL, 5098},
            {Alignment.CHAOTIC_EVIL, 5099},
        };

        [TempleDllLocation(0x102f7d68)]
        private ChargenStages uiPcCreationActiveStageIdx;

        [TempleDllLocation(0x10bdb8e4)]
        private int dword_10BDB8E4 = 1000;

        [TempleDllLocation(0x102f7938)]
        private readonly List<IChargenSystem> chargenSystems = new List<IChargenSystem>();

        [TempleDllLocation(0x11e72f00)]
        private CharEditorSelectionPacket charEdSelPkt = new CharEditorSelectionPacket();

        [TempleDllLocation(0x10bddd18)]
        private WidgetContainer _mainWindow;

        [TempleDllLocation(0x11e741b4)]
        private ScrollBox uiPcCreationScrollBox;

        [TempleDllLocation(0x10bdd5d4)]
        private ChargenStages uiPcCreationStagesCompleted;

        [TempleDllLocation(0x102f7bf0)]
        public bool uiPcCreationIsHidden = true;

        [TempleDllLocation(0x1011b750)]
        public bool IsVisible => !uiPcCreationIsHidden;

        [TempleDllLocation(0x10bddd1c)]
        public int defaultPartyNum { get; set; }

        [TempleDllLocation(0x10bd3a48)]
        [TempleDllLocation(0x10111120)]
        public int startMap { get; set; }

        [TempleDllLocation(0x1011b730)]
        public bool IsPointBuy =>
            charEdSelPkt.isPointbuy
            && uiPcCreationActiveStageIdx == ChargenStages.CG_Stage_Stats;

        [TempleDllLocation(0x11e741a0)]
        private GameObjectBody charEditorObjHnd;

        private readonly PartyAlignmentUi _partyAlignmentUi = new PartyAlignmentUi();

        private readonly IComparer<SpellStoreData> _spellPriorityComparer;

        [TempleDllLocation(0x10bddd20)]
        private bool ironmanSaveNamePopupActive;

        private StatBlockWidget _statBlockWidget;

        private WidgetContent _activeButtonBorder;

        [TempleDllLocation(0x10BDB100)]
        private WidgetText _descriptionLabel;

        [TempleDllLocation(0x10120420)]
        public PCCreationUi()
        {
            Stub.TODO();

            /*
             * TODO
             * uiPromptType3.idx = 3;
                      uiPromptType3.styleIdx = 0;
                      uiPromptType3.bodyText = dword_10BDB8D4;
                      uiPromptType3.textRect.x = 23;
                      uiPromptType3.textRect.y = 19;
                      uiPromptType3.textRect.width = 297;
                      uiPromptType3.textRect.height = 122;
                      UI_art_manager__get_image(3, 2, (ImgFile **)&uiPromptType3.image);
                      uiPromptType3.wndRect.x = (conf->width - 341) / 2;
                      uiPromptType3.wndRect.y = (conf->height - 158) / 2;
                      uiPromptType3.wndRect.width = 341;
                      uiPromptType3.wndRect.height = 158;
             */

            var doc = WidgetDoc.Load("ui/pc_creation/pc_creation_ui.json");
            _mainWindow = doc.TakeRootContainer();
            _mainWindow.OnBeforeRender += BeforeRenderMainWindow;
            uiPcCreationScrollBox = new ScrollBox(new Rectangle(219, 295, 433, 148), new ScrollBoxSettings
            {
                TextArea = new Rectangle(3, 3, 415, 142),
                DontAutoScroll = true,
                Indent = 15,
                ScrollBarHeight = 148,
                ScrollBarPos = new Point(420, 0),
                Font = PredefinedFont.ARIAL_10
            });
            _mainWindow.Add(uiPcCreationScrollBox);

            _descriptionLabel = doc.GetTextContent("playerDescriptionLine");

            _activeButtonBorder = doc.GetContent("activeButtonBorder");
            _activeButtonBorder.Visible = false;

            // TODO: 0x1011c1c0 contains logic to animate / rotate the char model

            _partyAlignmentUi.OnCancel += Cancel;
            _partyAlignmentUi.OnConfirm += AlignmentSelected;

            _spellPriorityComparer = new SpellPriorityComparer(LoadSpellPriorities());

            chargenSystems.Add(new AbilityScoreSystem());
            chargenSystems.Add(new RaceSystem());
            chargenSystems.Add(new GenderSystem());
            chargenSystems.Add(new Systems.HeightSystem());
            chargenSystems.Add(new HairSystem());
            chargenSystems.Add(new ClassSystem());
            chargenSystems.Add(new AlignmentSystem());
            chargenSystems.Add(new Systems.DeitySystem());
            chargenSystems.Add(new AbilitiesSystem());
            chargenSystems.Add(new FeatsSystem());
            chargenSystems.Add(new SkillsSystem());
            chargenSystems.Add(new SpellsSystem());
            chargenSystems.Add(new Systems.PortraitSystem());
            chargenSystems.Add(new VoiceSystem());

            var stateButtonsContainer = doc.GetContainer("stateButtons");
            var y = 0;
            foreach (var system in chargenSystems)
            {
                _mainWindow.Add(system.Container);

                var stageButton = CreateStageButton(system);
                stageButton.SetPos(0, y);
                y += stageButton.Height;
                stateButtonsContainer.Add(stageButton);
            }

            _statBlockWidget = new StatBlockWidget();
            doc.GetContainer("statBlock").Add(_statBlockWidget.Container);

            var modelPreviewContainer = doc.GetContainer("modelPreview");
            _modelPreview = new MiniatureWidget();
            _modelPreview.SetSize(modelPreviewContainer.GetSize());
            modelPreviewContainer.Add(_modelPreview);
        }

        private WidgetButton CreateStageButton(IChargenSystem system)
        {
            var stageButton = new WidgetButton();
            stageButton.SetStyle("chargen-active-button");
            stageButton.SetText(system.ButtonLabel);
            stageButton.OnBeforeRender += () =>
            {
                stageButton.SetActive(uiPcCreationActiveStageIdx == system.Stage);
                stageButton.SetDisabled(uiPcCreationStagesCompleted < system.Stage);

                // Render the blue outline for the active stage
                if (stageButton.IsActive())
                {
                    var contentArea = stageButton.GetContentArea(true);
                    _activeButtonBorder.ContentArea = contentArea;
                    _activeButtonBorder.Render();
                }
            };
            stageButton.SetClickHandler(() => ShowStage(system.Stage));
            stageButton.OnMouseEnter += msg => { ShowHelpTopic(system.HelpTopic); };
            stageButton.OnMouseExit += msg => { system.ButtonExited(); };
            return stageButton;
        }

        [TempleDllLocation(0x1011ed80)]
        [TemplePlusLocation("ui_pc_creation.cpp")]
        private void BeforeRenderMainWindow()
        {
            ChargenStages stage;
            var nextStage = uiPcCreationStagesCompleted + 1;
            // TODO TEMPORARY
            if ((int) nextStage >= chargenSystems.Count)
            {
                nextStage = (ChargenStages) (chargenSystems.Count - 1);
            }
            // TODO END TEMPORARY

            if (nextStage > ChargenStages.CG_STAGE_COUNT)
            {
                nextStage = ChargenStages.CG_STAGE_COUNT;
            }

            for (stage = 0; stage < nextStage; stage++)
            {
                if (!chargenSystems[(int) stage].CheckComplete())
                {
                    break;
                }
            }

            if (stage != uiPcCreationStagesCompleted)
            {
                uiPcCreationStagesCompleted = stage;
                if (uiPcCreationActiveStageIdx > stage)
                {
                    uiPcCreationActiveStageIdx = stage;
                }

                // reset the next stages
                ResetSystemsAfter(stage);
                UpdatePlayerDescription();
            }

            // TODO var wnd = uiManager->GetWindow(id);
            // TODO RenderHooks::RenderImgFile(temple::GetRef<ImgFile*>(0x10BDAFE0), wnd->x, wnd->y);
            UpdateStatBlock();
            // TODO UiRenderer::PushFont(PredefinedFont::PRIORY_12);

            // TODO UiRenderer::DrawTextInWidget(id,
            // TODO temple::GetRef<char[] >(0x10BDB100),
            // TODO temple::GetRef<TigRect>(0x10BDB004),
            // TODO temple::GetRef<TigTextStyle>(0x10BDDCC8));
            // TODO UiRenderer::PopFont();

            // TODO var renderCharModel = temple::GetRef<void(__cdecl)(int)>(0x1011C320);
            // TODO renderCharModel(id);
        }

        [TempleDllLocation(0x1011e740)]
        private void UpdateStatBlock()
        {
            _statBlockWidget.Update(charEdSelPkt, charEditorObjHnd, uiPcCreationStagesCompleted);
        }

        [TempleDllLocation(0x1011e160)]
        private static Dictionary<int, int> LoadSpellPriorities()
        {
            // Load spell var-memorization rules
            var spellPriorityRules = Tig.FS.ReadMesFile("rules/spell_priority.mes");
            var priorities = new Dictionary<int, int>(spellPriorityRules.Count);
            foreach (var (priority, spellName) in spellPriorityRules)
            {
                if (!GameSystems.Spell.GetSpellEnumByEnglishName(spellName, out var spellEnum))
                {
                    Logger.Warn("Unknown spell name in spell priority list: '{0}'", spellName);
                    continue;
                }

                priorities[spellEnum] = priority;
            }

            return priorities;
        }

        private void AlignmentSelected(Alignment alignment)
        {
            GameSystems.Party.PartyAlignment = alignment;
            UiSystems.PartyPool.Show(false);
        }

        [TempleDllLocation(0x1011ebc0)]
        public void Dispose()
        {
            _partyAlignmentUi.Dispose();

            Stub.TODO();
        }

        [TempleDllLocation(0x1011fdc0)]
        public void Start()
        {
            if (defaultPartyNum > 0)
            {
                for (var i = 0; i < defaultPartyNum; i++)
                {
                    var protoId = 13100 + i;
                    charEditorObjHnd = GameSystems.MapObject.CreateObject(protoId, new locXY(640, 480));
                    GameSystems.Critter.GenerateHp(charEditorObjHnd);
                    GameSystems.Party.AddToPCGroup(charEditorObjHnd);
                    GameSystems.Item.GiveStartingEquipment(charEditorObjHnd);

                    var model = charEditorObjHnd.GetOrCreateAnimHandle();
                    charEditorObjHnd.UpdateRenderHeight(model);
                    charEditorObjHnd.UpdateRadius(model);
                }

                UiChargenFinalize();
            }
            else
            {
                _partyAlignmentUi.Reset();
                StartNewParty();
            }
        }

        [TempleDllLocation(0x1011e200)]
        [TemplePlusLocation("ui_pc_creation_hooks.cpp:216")]
        private void StartNewParty()
        {
            uiPcCreationIsHidden = false;
            UiPcCreationSystemsResetAll();

//           TODO uiPromptType3/*0x10bdd520*/.bodyText = (string )uiPcCreationText_SelectAPartyAlignment/*0x10bdb018*/;
//
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 0);
//          TODO  WidgetBringToFront/*0x101f8e40*/(uiPcCreationMainWndId/*0x10bdd690*/);

            _partyAlignmentUi.Show();
            UiSystems.UtilityBar.Hide();
        }

        [TempleDllLocation(0x1011ddd0)]
        private void UiPcCreationSystemsResetAll()
        {
//          TODO  WidgetSetHidden/*0x101f9100*/(uiPcCreationWndId/*0x10bddd18*/, 1);

// TODO
//            v0 = &chargenSystems/*0x102f7938*/[0].hide;
//            do
//            {
//                if ( *v0 )
//                {
//                    (*v0)();
//                }
//                v0 += 11;
//            }
//            while ( (int)v0 < (int)&dword_102F7BB8/*0x102f7bb8*/ );
        }

        [TempleDllLocation(0x1011fc30)]
        public void UiChargenFinalize()
        {
            if (CheckPartySpareSpellSlotsForLevel1Caster())
            {
                if (defaultPartyNum == 0)
                {
                    PcCreationAutoAddSpellsMemorized();
                }
            }

            if (!Globals.GameLib.IsIronmanGame || Globals.GameLib.IronmanSaveName != null)
            {
                UiSystems.PartyPool.UiPartypoolClose(false);
                UiSystems.PartyPool.ClearAvailable();

                MoveToStartMap();
                UiSystems.Party.UpdateAndShowMaybe();

                foreach (var partyMember in GameSystems.Party.PartyMembers)
                {
                    GameSystems.Item.GiveStartingEquipment(partyMember);
                    GameSystems.Spell.PendingSpellsToMemorized(partyMember);
                    partyMember.ClearArray(obj_f.critter_spells_cast_idx);

                    var model = partyMember.GetOrCreateAnimHandle();
                    partyMember.UpdateRenderHeight(model);
                    partyMember.UpdateRadius(model);
                }

                // Cancel all popups
                UiSystems.Popup.CloseAll();

                // TODO
//    hider = (void (**)(void))&chargenSystems/*0x102f7938*/[0].hide;
//    do
//    {
//      if ( *hider )
//      {
//        (*hider)();
//      }
//      hider += 11;
//    }
//    while ( (int)hider < (int)&dword_102F7BB8/*0x102f7bb8*/ );

                UiSystems.PCCreation.uiPcCreationIsHidden = true;
                UiSystems.UtilityBar.Show();
                UiSystems.MainMenu.Hide();
                UiSystems.Party.Update();
            }
            else if (!ironmanSaveNamePopupActive)
            {
                ironmanSaveNamePopupActive = true;
                UiSystems.Popup.RequestTextEntry("#{pc_creation:600}", "#{pc_creation:601}", ConfirmedIronmanSaveName);
            }
        }

        [TempleDllLocation(0x10111a80)]
        private void MoveToStartMap()
        {
            if (startMap != 0 && GameSystems.Map.IsValidMapId(startMap))
            {
                UiSystems.MainMenu.TransitionToMap(startMap);
            }
            else
            {
                var mapId = StartMaps[GameSystems.Party.PartyAlignment];
                UiSystems.MainMenu.TransitionToMap(mapId);
            }
        }

        [TempleDllLocation(0x1011bcb0)]
        private void ConfirmedIronmanSaveName(string name, bool sthg)
        {
            ironmanSaveNamePopupActive = false;
            if (!sthg && !string.IsNullOrEmpty(name))
            {
                Globals.GameLib.SetIronmanSaveName(name);
                UiChargenFinalize();
                GameSystems.Party.AddPartyMoney(0, 500, 0,
                    0); // TODO: FISHY! Why does this not allow continously adding money to the party???
            }
            else
            {
                GameSystems.Party.AddPartyMoney(0, 500, 0,
                    0); // TODO: FISHY! Why does this not allow continously adding money to the party???
            }
        }

        [TempleDllLocation(0x1011e8e0)]
        private bool CheckPartySpareSpellSlotsForLevel1Caster()
        {
            return GameSystems.Party.PlayerCharacters.Any(CheckSpareSpellSlotsForLevel1Caster);
        }

        [TempleDllLocation(0x1011e0b0)]
        private static bool CheckSpareSpellSlotsForLevel1Caster(GameObjectBody player)
        {
            var bonusSpells = 0;
            var firstClass = (Stat) player.GetInt32(obj_f.critter_level_idx, 0);
            var numMemo = player.GetArrayLength(obj_f.critter_spells_memorized_idx);
            if (firstClass == Stat.level_cleric)
            {
                bonusSpells = 1;
            }
            else if (firstClass != Stat.level_druid && firstClass != Stat.level_wizard)
            {
                return false;
            }

            var numSpells = D20ClassSystem.GetNumSpellsFromClass(player, firstClass, 0, 1) + bonusSpells;
            if (numMemo >= numSpells + D20ClassSystem.GetNumSpellsFromClass(player, firstClass, 1, 1))
            {
                return false;
            }

            return true;
        }

        [TempleDllLocation(0x1011f290)]
        private void PcCreationAutoAddSpellsMemorized()
        {
            foreach (var player in GameSystems.Party.PlayerCharacters)
            {
                AutoAddSpellsMemorized(player);
            }
        }

        [TempleDllLocation(0x1011e920)]
        private void AutoAddSpellsMemorized(GameObjectBody handle)
        {
            var firstClass = (Stat) handle.GetInt32(obj_f.critter_level_idx, 0);
            var spellClass = SpellSystem.GetSpellClass(firstClass);

            // Memorize 0th and 1st level spells
            for (var spellLevel = 0; spellLevel <= 1; spellLevel++)
            {
                // Build a prioritized list of cantrips that can be memorized
                var spellsKnown = handle.GetSpellArray(obj_f.critter_spells_known_idx)
                    .Where(sp => sp.classCode == spellClass)
                    .Where(sp => GameSystems.Spell.GetSpellLevelBySpellClass(sp.spellEnum, spellClass) == spellLevel)
                    .ToList();
                spellsKnown.Sort(_spellPriorityComparer);

                // TODO: Needs rework by using the new slot based query system for spells memorized / castable
                var additionalMemos = 0;
                var initialMemoCount = handle.GetArrayLength(obj_f.critter_spells_memorized_idx);
                var canCastCount = D20ClassSystem.GetNumSpellsFromClass(handle, firstClass, 0, 1);

                while (handle.GetArrayLength(obj_f.critter_spells_memorized_idx) < initialMemoCount + canCastCount)
                {
                    var spellToMemo = spellsKnown[additionalMemos % spellsKnown.Count];
                    var state = new SpellStoreState();
                    state.spellStoreType = SpellStoreType.spellStoreMemorized;
                    GameSystems.Spell.SpellMemorizedAdd(handle, spellToMemo.spellEnum, spellClass,
                        spellToMemo.spellLevel,
                        state);
                    ++additionalMemos;
                }
            }

            // Handle domain spells for clerics
            if (firstClass == Stat.level_cleric)
            {
                var domain = (DomainId) handle.GetInt32(obj_f.critter_domain_1);
                var domainClassCode = SpellSystem.GetSpellClass(domain);

                var spellsKnown = handle.GetSpellArray(obj_f.critter_spells_known_idx)
                    .Where(sp => sp.classCode == domainClassCode)
                    .Where(sp => GameSystems.Spell.GetSpellLevelBySpellClass(sp.spellEnum, domainClassCode) != 0)
                    .ToList();
                spellsKnown.Sort(_spellPriorityComparer);

                if (spellsKnown.Count > 0)
                {
                    var spellToMemo = spellsKnown[0];
                    var state = new SpellStoreState();
                    state.spellStoreType = SpellStoreType.spellStoreMemorized;
                    GameSystems.Spell.SpellMemorizedAdd(handle, spellToMemo.spellEnum, spellClass,
                        spellToMemo.spellLevel,
                        state);
                }
            }
        }

        [TempleDllLocation(0x1011e270)]
        public void Hide()
        {
            // TODO ScrollboxHideWindow/*0x1018cac0*/(uiPcCreationScrollBox/*0x11e741b4*/);
            // TODO UiPcCreationPortraitsMainHide/*0x10163030*/();
            UiSystems.Popup.CloseAll();
            // TODO WidgetSetHidden/*0x101f9100*/(uiPcCreationMainWndId/*0x10bdd690*/, 1);
            _partyAlignmentUi.Hide();
            UiPcCreationSystemsResetAll();
            uiPcCreationIsHidden = true;
            UiSystems.UtilityBar.Show();
        }

        private void Cancel()
        {
            ClearParty();
            Hide();
            UiSystems.MainMenu.Show(MainMenuPage.Difficulty);
        }

        [TempleDllLocation(0x1011b6f0)]
        public void ClearParty()
        {
            while (GameSystems.Party.PartySize > 0)
            {
                var player = GameSystems.Party.GetPartyGroupMemberN(0);
                GameSystems.Party.RemoveFromAllGroups(player);
            }

            // TODO PcPortraitsDeactivate/*0x10163060*/();
        }

        [TempleDllLocation(0x1011ec60)]
        public void Begin()
        {
            // TODO: This seems weird and kills encapsulation
            UiSystems.PartyPool.BeginAdventuringButton.Visible = true;
            UiSystems.PartyPool.BeginAdventuringButton.SetDisabled(false);

            StartNewParty();
            uiPcCreationActiveStageIdx = 0;
            charEditorObjHnd = null;
            dword_10BDB8E4 = 1000;

            foreach (var system in chargenSystems)
            {
                system.Reset(charEdSelPkt);
            }

            UiSystems.PCCreation._partyAlignmentUi.Hide();
            Globals.UiManager.RootElement.Append(_mainWindow);
            _mainWindow.BringToFront();

            ShowStage(ChargenStages.CG_Stage_Stats);
            UpdatePlayerDescription();
        }

        [TempleDllLocation(0x1011c470)]
        public void UpdatePlayerDescription()
        {
            var desc = new StringBuilder();

            // Alignment
            if (charEdSelPkt.alignment.HasValue)
            {
                var s = GameSystems.Stat.GetAlignmentName(charEdSelPkt.alignment.Value);
                desc.Append(s);
                desc.Append(" ");
            }

            // Gender
            if (charEdSelPkt.genderId.HasValue)
            {
                var s = GameSystems.Stat.GetGenderName(charEdSelPkt.genderId.Value);
                desc.Append(s);
                desc.Append(" ");
            }

            // Race
            if (charEdSelPkt.raceId.HasValue)
            {
                var s = GameSystems.Stat.GetRaceName(charEdSelPkt.raceId.Value);
                desc.Append(s);
                desc.Append(" ");
            }

            // Deity
            if (charEdSelPkt.deityId.HasValue)
            {
                desc.Append("@1");
                desc.Append("#{pc_creation:500}"); // "Worships"
                desc.Append("@0");
                desc.Append(GameSystems.Deity.GetName(charEdSelPkt.deityId.Value));
            }

            _descriptionLabel.SetText(desc.ToString());
        }

        [TempleDllLocation(0x1011e3b0)]
        private void ShowStage(ChargenStages stage)
        {
            if (stage > uiPcCreationStagesCompleted)
            {
                return;
            }

            chargenSystems[(int) uiPcCreationActiveStageIdx].Hide();

            if (stage == uiPcCreationStagesCompleted && stage > ChargenStages.CG_Stage_Stats)
            {
                for (var i = ChargenStages.CG_Stage_Stats; i < stage; i++)
                {
                    chargenSystems[(int) i].Finalize(charEdSelPkt, ref charEditorObjHnd);
                }
            }

            // This has to be set here because Finalize on the systems called above may replace the handle
            _modelPreview.Object = charEditorObjHnd;
            _modelPreview.Visible = charEditorObjHnd != null
                                    && stage > ChargenStages.CG_Stage_Gender
                                    && (stage < ChargenStages.CG_Stage_Portrait || charEdSelPkt.portraitId == 0);

            uiPcCreationActiveStageIdx = stage;

            if (stage >= ChargenStages.CG_STAGE_COUNT)
            {
                UiSystems.PCCreation.UiPcCreationSystemsResetAll();
                if (dword_10BDB8E4 == 1000)
                {
                    UiSystems.PartyPool.BeginAdventuringButton.Visible = false;
                    UiSystems.PartyPool.Add(UiSystems.PCCreation.charEditorObjHnd);
                    if (GameSystems.Map.GetCurrentMapId() == GameSystems.Map.GetMapIdByType(MapType.ShoppingMap))
                    {
                        UiSystems.PartyPool.Show(false);
                    }
                    else
                    {
                        UiSystems.PartyPool.Show(true);
                    }

                    _partyAlignmentUi.Hide();
                }
                else
                {
                    // TODO UiSystems.Popup.OnClickButton(3, 0);
                    // TODO UiSystems.Popup.UiPopupShow_Impl(&uiPromptType3 /*0x10bdd520*/, 3, 0);
                    GameSystems.Party.AddToPCGroup(UiSystems.PCCreation.charEditorObjHnd);
                    GameSystems.Item.GiveStartingEquipment(UiSystems.PCCreation.charEditorObjHnd);
                    // TODO PcPortraitsButtonActivateNext /*0x10163090*/();
                }

                UiSystems.PCCreation.charEditorObjHnd = null;
                // TODO ScrollboxHideWindow /*0x1018cac0*/(uiPcCreationScrollBox /*0x11e741b4*/);
            }
            else
            {
                chargenSystems[(int) stage].Activate();
                // TODO: Probably no longer needed UiPcCreationStatTitleUpdateMeasures/*0x1011bd10*/(stage);
                var systemNameId = chargenSystems[(int) stage].HelpTopic;
                ShowHelpTopic(systemNameId);
                chargenSystems[(int) stage].Show();
            }
        }

        public void SkipToStageForTesting(ChargenStages stage, Dictionary<string, object> props)
        {
            while (uiPcCreationStagesCompleted < stage &&
                   chargenSystems[(int) uiPcCreationStagesCompleted].CompleteForTesting(props))
            {
                BeforeRenderMainWindow();
                ShowStage(uiPcCreationStagesCompleted);
            }
        }

        private MiniatureWidget _modelPreview;

        [TempleDllLocation(0x1011b890)]
        internal void ShowHelpTopic(string systemName)
        {
            if (GameSystems.Help.TryGetTopic(systemName, out var topic))
            {
                uiPcCreationScrollBox.DontAutoScroll = true;
                uiPcCreationScrollBox.Indent = 15;
                uiPcCreationScrollBox.SetEntries(new List<D20RollHistoryLine>
                {
                    D20RollHistoryLine.Create(topic.Title),
                    D20RollHistoryLine.Create("\n"),
                    new D20RollHistoryLine(topic.Text, topic.Links)
                });
            }
            else
            {
                uiPcCreationScrollBox.ClearLines();
            }
        }

        [TempleDllLocation(0x1011bae0)]
        internal void ShowHelpText(string text)
        {
            uiPcCreationScrollBox.DontAutoScroll = true;
            uiPcCreationScrollBox.Indent = 15;
            uiPcCreationScrollBox.SetEntries(new List<D20RollHistoryLine>
            {
                D20RollHistoryLine.Create(text)
            });
        }

        [TempleDllLocation(0x1011bc70)]
        internal void ResetSystemsAfter(ChargenStages stage)
        {
            for (var i = (int) stage + 1; i < chargenSystems.Count; i++)
            {
                chargenSystems[i].Reset(charEdSelPkt);
            }
        }

        public GameObjectBody EditedChar => charEditorObjHnd;
    }

    public enum ChargenStages
    {
        CG_Stage_Stats = 0,
        CG_Stage_Race,
        CG_Stage_Gender,
        CG_Stage_Height,
        CG_Stage_Hair,
        CG_Stage_Class,
        CG_Stage_Alignment,
        CG_Stage_Deity,
        CG_Stage_Abilities,
        CG_Stage_Feats,
        CG_Stage_Skills,
        CG_Stage_Spells,
        CG_Stage_Portrait,
        CG_Stage_Voice,

        CG_STAGE_COUNT
    }

    public class CharEditorSelectionPacket
    {
        public int[] abilityStats = new int[6];
        public int numRerolls; // number of rerolls
        public bool isPointbuy;
        public string rerollString;
        public Stat statBeingRaised;
        public RaceId? raceId; // RACE_INVALID is considered invalid
        public Gender? genderId; // 2 is considered invalid
        public int height;
        public int weight;
        public float modelScale; // 0.0 is considered invalid
        public HairStyle? hairStyle;
        public HairColor? hairColor;
        public Stat classCode;
        public DeityId? deityId;
        public DomainId domain1;
        public DomainId domain2;
        public Alignment? alignment;
        public AlignmentChoice alignmentChoice; // 1 is for Positive Energy, 2 is for Negative Energy
        public FeatId? feat0;
        public FeatId? feat1;
        public FeatId? feat2;
        public Dictionary<SkillId, int> skillPointsAdded = new Dictionary<SkillId, int>(); // idx corresponds to skill enum
        public int skillPointsSpent;
        public int availableSkillPoints;
        public int[] spellEnums = new int[SpellSystem.SPELL_ENUM_MAX_VANILLA];
        public int spellEnumsAddedCount;
        public int spellEnumToRemove; // for sorcerers who swap out spells
        public SchoolOfMagic wizSchool;
        public SchoolOfMagic forbiddenSchool1;
        public SchoolOfMagic forbiddenSchool2;
        public FeatId? feat3;
        public FeatId? feat4;
        public int portraitId;
        public string voiceFile;
        public int voiceId; // -1 is considered invalid
    };

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

        void Hide()
        {
            Container.Visible = false;
        }

        void Show()
        {
            Container.Visible = true;
        }

        // checks if the char editing stage is complete (thus allowing you to move on to the next stage). This is checked at every render call.
        bool CheckComplete()
        {
            return true;
        }

        void Finalize(CharEditorSelectionPacket charSpec, ref GameObjectBody playerObj)
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
}