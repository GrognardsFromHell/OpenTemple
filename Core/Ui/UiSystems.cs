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
    }

    public class KeyManagerUi
    {
    }

    public class OptionsUi
    {
        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }
    }

    public class PartyQuickviewUi
    {
    }

    public class CampingUi
    {
    }

    public class FormationUi
    {
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
        public void Hide()
        {
            // TODO throw new System.NotImplementedException(); // TODO
        }

        [TempleDllLocation(0x101156b0)]
        public void HideOpenedWindows(bool b)
        {
            // TODO  throw new System.NotImplementedException();
        }

        public bool IsVisible()
        {
            // throw new System.NotImplementedException();
            return false;
        }

        public void Show()
        {
            throw new System.NotImplementedException();
        }
    }

    public class SkillMasteryUi
    {
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
        [TempleDllLocation(0x1015f140)]
        public void Show(int mode)
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

        public bool IsVisible => false;

    }

    public class ScrollpaneUi
    {
    }

    // Was part of CharSheetUi

    public class PCCreationUi
    {
        public void Start()
        {
            throw new System.NotImplementedException(); // TODO
        }
    }

    public class DialogUi
    {
        [TempleDllLocation(0x1014bb50)]
        public bool IsActive { get; }

        [TempleDllLocation(0x10BEC348)]
        public bool IsActive2 { get; set; }

        public DialogUi()
        {
            Stub.TODO();

            GameSystems.AI.SetDialogFunctions(CancelDialog, ShowTextBubble);
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

    public class TurnBasedUi
    {
        [TempleDllLocation(0x101749D0)]
        public void sub_101749D0()
        {
            Stub.TODO();
        }

        [TempleDllLocation(0x10174970)]
        public void sub_10174970(GameObjectBody obj)
        {
            Stub.TODO();
        }
    }

    public class SaveGameUi
    {
        public void Show(bool unk)
        {
            throw new System.NotImplementedException(); // TODO
        }

        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }

    public class LoadGameUi
    {
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