using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using SpicyTemple.Core.Config;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.D20;
using SpicyTemple.Core.Systems.D20.Actions;
using SpicyTemple.Core.Ui.CharSheet;
using SpicyTemple.Core.Ui.CharSheet.HelpInventory;
using SpicyTemple.Core.Ui.InGame;
using SpicyTemple.Core.Ui.InGameSelect;
using SpicyTemple.Core.Ui.MainMenu;
using SpicyTemple.Core.Ui.Party;
using SpicyTemple.Core.Ui.RadialMenu;
using SpicyTemple.Core.Ui.WidgetDocs;

namespace SpicyTemple.Core.Ui
{
    public static class UiSystems
    {
        public static MainMenuUi MainMenu { get; private set; }

        // UiMM is unused

        public static LoadGameUi LoadGame { get; private set; }

        public static SaveGameUi SaveGame { get; private set; }

        public static InGameUi InGame { get; private set; }

        public static RadialMenuUi RadialMenu { get; private set; }

        public static InGameSelectUi InGameSelect { get; private set; }

        public static TurnBasedUi TurnBased { get; private set; }

        public static AnimUi Anim { get; private set; }

        public static TBUi TB { get; private set; }

        public static WorldMapRandomEncounterUi WorldMapRandomEncounter { get; private set; }

        public static CombatUi Combat { get; private set; }

        public static SlideUi Slide { get; private set; }

        public static DialogUi Dialog { get; private set; }

        public static PCCreationUi PCCreation { get; private set; }

        public static CharSheetUi CharSheet { get; private set; }

        public static TooltipUi Tooltip { get; private set; }

        public static LogbookUi Logbook { get; private set; }

        public static ScrollpaneUi Scrollpane { get; private set; }

        public static TownMapUi TownMap { get; private set; }

        public static PopupUi Popup { get; private set; }

        public static TextDialogUi TextDialog { get; private set; }

        public static FocusManagerUi FocusManager { get; private set; }

        public static WorldMapUi WorldMap { get; private set; }

        public static RandomEncounterUi RandomEncounter { get; private set; }

        public static HelpUi Help { get; private set; }

        public static ItemCreationUi ItemCreation { get; private set; }

        public static SkillMasteryUi SkillMastery { get; private set; }

        public static UtilityBarUi UtilityBar { get; private set; }

        public static DungeonMasterUi DungeonMaster { get; private set; }

        public static TrackUi Track { get; private set; }

        public static PartyPoolUi PartyPool { get; private set; }

        public static PccPortraitUi PccPortrait { get; private set; }

        public static PartyUi Party { get; private set; }

        public static FormationUi Formation { get; private set; }

        public static CampingUi Camping { get; private set; }

        public static PartyQuickviewUi PartyQuickview { get; private set; }

        public static OptionsUi Options { get; private set; }

        public static KeyManagerUi KeyManager { get; private set; }

        public static HelpManagerUi HelpManager { get; private set; }

        public static SliderUi Slider { get; private set; }

        public static WrittenUi Written { get; private set; }

        public static CharmapUi Charmap { get; private set; }

        public static ManagerUi Manager { get; private set; }

        public static void Startup(GameConfig config)
        {
            Tooltip = new TooltipUi();
            MainMenu = new MainMenuUi();
            SaveGame = new SaveGameUi();
            LoadGame = new LoadGameUi();
            UtilityBar = new UtilityBarUi();
            DungeonMaster = new DungeonMasterUi();
            CharSheet = new CharSheetUi();
            InGame = new InGameUi();
            HelpManager = new HelpManagerUi();
            WorldMapRandomEncounter = new WorldMapRandomEncounterUi();
            InGameSelect = new InGameSelectUi();
            ItemCreation = new ItemCreationUi();
            Party = new PartyUi();
            TextDialog = new TextDialogUi();
            RadialMenu = new RadialMenuUi();
            Dialog = new DialogUi();
            Manager = new ManagerUi();
            Logbook = new LogbookUi();
            TB = new TBUi();
            Combat = new CombatUi();
            PartyPool = new PartyPoolUi();
            Popup = new PopupUi();
            Help = new HelpUi();
            TurnBased = new TurnBasedUi();
            Written = new WrittenUi();
            TownMap = new TownMapUi();
            WorldMap = new WorldMapUi();
        }

        [TempleDllLocation(0x10115270)]
        public static void Reset()
        {
            // TODO
            throw new System.NotImplementedException();
        }

        public static void ResizeViewport(int width, int height)
        {
            throw new System.NotImplementedException();
        }
    }

    public class CharmapUi
    {
    }

    public class WrittenUi
    {
        [TempleDllLocation(0x10160f50)]
        public void Show(GameObjectBody item)
        {
            throw new NotImplementedException();
        }
    }

    public struct SliderParams
    {
        public int Amount { get; set; }

        public int MinAmount { get; set; }

        public int field_8 { get; set; }

        public int field_c { get; set; }

        public Action<GameObjectBody> callback { get; set; }

        public string header { get; set; }

        public string icon { get; set; }

        public int transferType { get; set; }

        public GameObjectBody obj { get; set; }

        public GameObjectBody parent { get; set; }

        public int sum { get; set; }

        public int invIdx { get; set; }

        public Action<int> textDrawCallback { get; set; }
    }

    public class SliderUi
    {
        public void Show(ref SliderParams sliderParams)
        {
            throw new NotImplementedException();
        }
    }

    public class HelpManagerUi : IDisposable
    {
        [TempleDllLocation(0x10124a10)]
        [TempleDllLocation(0x10BDE3DC)]
        public bool IsTutorialActive { get; private set; }

        [TempleDllLocation(0x10BDE3D8)]
        [TempleDllLocation(0x10124a00)]
        public bool IsSelectingHelpTarget { get; private set; }

        [TempleDllLocation(0x10124840)]
        public HelpManagerUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10124870)]
        public void Dispose()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10124870)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10124880)]
        public void SaveGame()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101248b0)]
        public void LoadGame()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101249e0)]
        public void ToggleTutorial()
        {
            IsTutorialActive = !IsTutorialActive;
        }

        [TempleDllLocation(0x10124be0)]
        public void ShowTopic(int topicId)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x101249d0)]
        public CursorType? GetCursor()
        {
            if (IsSelectingHelpTarget)
            {
                return CursorType.IdentifyCursor2;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10124a40)]
        public void ShowPredefinedTopic(int id)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101249b0)]
        public void  ClickForHelpToggle()
        {
            IsSelectingHelpTarget = !IsSelectingHelpTarget;
        }

    }

    public class KeyManagerUi
    {
    }

    public class OptionsUi
    {

        [TempleDllLocation(0x10bda728)]
        private bool dword_10BDA728;

        [TempleDllLocation(0x101177d0)]
        public bool IsVisible => dword_10BDA728;

        [TempleDllLocation(0x10bda724)]
        public bool dword_10BDA724; // Prolly means shown from utility bar

        [TempleDllLocation(0x10119d20)]
        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x10117780)]
        public void Hide()
        {
            if ( dword_10BDA728 )
            {
                GameSystems.TimeEvent.PopDisableFidget();
            }
            dword_10BDA728 = false;
            // TODO WidgetSetHidden/*0x101f9100*/(widIdOut/*0x10bd9624*/, 1);
            // TODO WidgetSetHidden/*0x101f9100*/(dword_10BD98E0/*0x10bd98e0*/, 1);
            if ( dword_10BDA724 )
            {
                UiSystems.UtilityBar.Show();
            }
        }

    }

    public class PartyQuickviewUi
    {
    }

    public class CampingUi
    {

        [TempleDllLocation(0x1012e440)]
        public bool IsHidden => true; // TODO

        public bool IsVisible => !IsHidden;

        [TempleDllLocation(0x1012edf0)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1012f0c0)]
        public void Show()
        {
            throw new NotImplementedException();
        }

    }

    public class FormationUi
    {

        [TempleDllLocation(0x10124de0)]
        public bool IsVisible => false; // TODO

        [TempleDllLocation(0x10124d90)]
        public void Show()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10124dc0)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

    }

    public class PccPortraitUi
    {
    }

    public class PartyPoolUi
    {
        [TempleDllLocation(0x10163720)]
        public bool IsVisible
        {
            get
            {
                return false; // TODO
            }
        }
    }

    public class TrackUi
    {
    }

    public class DungeonMasterUi
    {
        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }

    public class UtilityBarUi
    {
        [TempleDllLocation(0x10bd33f8)]
        private bool uiUtilityBarVisible;

        public void Hide()
        {
            // TODO throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x101156b0)]
        public void HideOpenedWindows(bool b)
        {
            // TODO  throw new System.NotImplementedException();
        }

        public bool IsVisible() => uiUtilityBarVisible;

        [TempleDllLocation(0x1010ee80)]
        [TemplePlusLocation("ui_utility_bar.cpp:12")]
        public void Show()
        {
            // TODO WidgetSetHidden/*0x101f9100*/(uiUtilityBarWndId/*0x10bd2ee8*/, 0);
            // TODO j_WidgetCopy/*0x101f87a0*/(uiUtilityBarWndId/*0x10bd2ee8*/, (LgcyWidget *)&uiUtilityBarWnd/*0x10bd3120*/);
            // TODO WidgetBringToFront/*0x101f8e40*/(uiUtilityBarWndId/*0x10bd2ee8*/);
            // TODO UiHistoryShow/*0x101221c0*/();
            uiUtilityBarVisible = true;
        }

    }

    public class SkillMasteryUi
    {
        [TempleDllLocation(0x10bf3548)]
        private GameObjectBody skillMasteryObj;

        [TempleDllLocation(0x10bf3538)]
        private int skillMasteryIdx;

        [TempleDllLocation(0x10bf34c0)]
        private int[] skillIdx;

        [TempleDllLocation(0x1016a0b0)]
        public void SkillMasteryCallback(GameObjectBody objHnd)
        {
            if (objHnd == skillMasteryObj)
            {
                var bitfield = 0;
                for (var i = 0; i < skillMasteryIdx; ++i)
                {
                    bitfield |= 1 << skillIdx[i];
                }

                GameSystems.D20.D20SendSignal(objHnd, D20DispatcherKey.SIG_Rogue_Skill_Mastery_Init, bitfield);
            }
        }
    }

    public class ItemCreationUi
    {
        [TempleDllLocation(0x1014f180)]
        public bool IsVisible { get; set; } // TODO

        [TempleDllLocation(0x101536c0)]
        public void CreateItem(GameObjectBody creator, int actionData1)
        {
            throw new NotImplementedException();
        }
    }

    public class RandomEncounterUi
    {
    }

    public class WorldMapUi
    {

        [TempleDllLocation(0x10bef7dc)]
        private bool uiWorldmapIsVisible;

        [TempleDllLocation(0x10159b10)]
        public bool IsVisible => uiWorldmapIsVisible;

        [TempleDllLocation(0x1015f140)]
        public void Show(int mode)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1015e210)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x101595d0)]
        [TempleDllLocation(0x10bef7fc)]
        public bool IsMakingTrip { get; private set; }
    }

    public class FocusManagerUi
    {
    }

    public class TextDialogUi
    {
        [TempleDllLocation(0x1014e8e0)]
        public bool IsVisible { get; set; }
    }

    public class TownMapUi
    {

        [TempleDllLocation(0x10be1f74)]
        private bool uiTownmapIsAvailable;

        [TempleDllLocation(0x10be1f28)]
        private bool uiTownmapVisible;

        [TempleDllLocation(0x10128b60)]
        public bool IsVisible => uiTownmapVisible;

        [TempleDllLocation(0x1012bcb0)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1012c6a0)]
        public void Show()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x10128420)]
        public bool IsTownMapAvailable
        {
            get
            {
                var result = uiTownmapIsAvailable;
                if (!uiTownmapIsAvailable)
                {
                    var curmap = GameSystems.Map.GetCurrentMapId();
                    if (GameSystems.Map.IsVignetteMap(curmap))
                    {
                        result = uiTownmapIsAvailable;
                    }
                    else
                    {
                        result = true;
                        uiTownmapIsAvailable = true;
                    }
                }

                return result;
            }
        }
    }

    public class ScrollpaneUi
    {
    }

    // Was part of CharSheetUi

    public class PCCreationUi
    {

        [TempleDllLocation(0x102f7bf0)]
        public bool uiPcCreationIsHidden = true;

        [TempleDllLocation(0x1011b750)]
        public bool IsVisible => !uiPcCreationIsHidden;

        public void Start()
        {
            throw new System.NotImplementedException(); // TODO
        }
    }

    public class DialogUi
    {
        [TempleDllLocation(0x1014bb50)]
        public bool IsActive { get; }

        [TempleDllLocation(0x1014bac0)]
        [TempleDllLocation(0x10BEC348)]
        public bool IsActive2 { get; set; }

        public DialogUi()
        {
            Stub.TODO();

            GameSystems.AI.SetDialogFunctions(CancelDialog, ShowTextBubble);
        }

        [TempleDllLocation(0x1014ca20)]
        public void Hide()
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1014cde0)]
        public void ShowTextBubble(GameObjectBody critter, GameObjectBody speakingto, string text, int speechid)
        {
            throw new NotImplementedException();
        }

        [TempleDllLocation(0x1014BA40)]
        public void CancelDialog(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1014bad0)]
        public void sub_1014BAD0(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1014BFF0)]
        public void sub_1014BFF0(GameObjectBody obj)
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x1014bb20)]
        public void PlayVoiceLine(GameObjectBody speaker, GameObjectBody listener, int soundId)
        {
            Stub.TODO();
        }
    }

    public class SlideUi
    {
    }

    public class CombatUi
    {
        [TempleDllLocation(0x10BE700C)]
        private int dword_10BE700C;

        [TempleDllLocation(0x10BE7010)]
        private int dword_10BE7010;

        [TempleDllLocation(0x10173690)]
        public CombatUi()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10172E70)]
        public void Reset()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10142740)]
        public void Update()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10141760)]
        public CursorType? GetCursor()
        {
            if (dword_10BE700C != 0)
            {
                return (dword_10BE7010 != 0) ? CursorType.SlidePortraits : CursorType.InvalidSelection;
            }
            else
            {
                return null;
            }
        }

        [TempleDllLocation(0x10172e80)]
        public void SthCallback()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10173440)]
        public void   UiActionBarGetValuesFromMovement()
        {
//            TurnBasedStatus *v0;
//            float startDist;
//            float endDist;
//            TurnBasedStatus tbStat;
//
//            v0 = GameSystems.D20.Actions.curSeqGetTurnBasedStatus();
//            startDist = UiCombatActionBarGetRemainingMoveDistance/*0x10172fb0*/(v0);
//            GameSystems.D20.Actions.seqCheckFuncs(&tbStat);
//            endDist = UiCombatActionBarGetRemainingMoveDistance/*0x10172fb0*/(&tbStat);
//            GameSystems.Vagrant.ActionBarSetMovementValues(uiCombatActionBar/*0x10c040b8*/, startDist, endDist, uiCombatDepletionSpeed/*0x10c0407c*/);
//            actionBarActor/*0x10c040c0*/ = GameSystems.D20.Initiative.get_CurrentActor();
//            actionBarEndingMoveDist/*0x10c040c8*/ = LODWORD(endDist);
        }

    }

    public class WorldMapRandomEncounterUi
    {
        public void StartRandomEncounterTimer()
        {
            Stub.TODO();
        }
    }

    public class AnimUi
    {
    }

    public class SaveGameUi
    {

        [TempleDllLocation(0x10174e60)]
        public bool IsVisible => false; // TODO

        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x10175a40)]
        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }

    public class LoadGameUi
    {

        [TempleDllLocation(0x10176b00)]
        public bool IsVisible => false; // TODO

        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }

        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }
}