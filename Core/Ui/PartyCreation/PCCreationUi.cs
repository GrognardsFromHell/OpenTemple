using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Location;
using SpicyTemple.Core.Logging;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Classes;
using SpicyTemple.Core.Systems.Script;
using SpicyTemple.Core.Systems.Script.Extensions;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.MainMenu;

namespace SpicyTemple.Core.Ui.PartyCreation
{
    public class PCCreationUi : IDisposable
    {
        private static readonly ILogger Logger = new ConsoleLogger();

        [TempleDllLocation(0x102f7bf0)]
        public bool uiPcCreationIsHidden = true;

        [TempleDllLocation(0x1011b750)]
        public bool IsVisible => !uiPcCreationIsHidden;

        [TempleDllLocation(0x10bddd1c)]
        public int defaultPartyNum { get; set; }

        [TempleDllLocation(0x10bd3a48)]
        [TempleDllLocation(0x10111120)]
        public int startMap { get; set; }

        [TempleDllLocation(0x11e741a0)]
        private GameObjectBody charEditorObjHnd;

        private readonly PartyAlignmentUi _partyAlignmentUi = new PartyAlignmentUi();

        private readonly IComparer<SpellStoreData> _spellPriorityComparer;

        [TempleDllLocation(0x10bddd20)]
        private bool ironmanSaveNamePopupActive;

        [TempleDllLocation(0x10120420)]
        public PCCreationUi()
        {
            Stub.TODO();

            _partyAlignmentUi.OnCancel += Cancel;
            _partyAlignmentUi.OnConfirm += AlignmentSelected;

            _spellPriorityComparer = new SpellPriorityComparer(LoadSpellPriorities());
        }

        [TempleDllLocation(0x1011e160)]
        private static Dictionary<int, int> LoadSpellPriorities()
        {
            // Load spell auto-memorization rules
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
            var spellClass = GameSystems.Spell.GetSpellClass(firstClass);

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
                var domainClassCode = GameSystems.Spell.GetSpellClass(domain);

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
    }
}