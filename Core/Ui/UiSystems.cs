using SpicyTemple.Core.Config;
using SpicyTemple.Core.Ui.MainMenu;

namespace SpicyTemple.Core.Ui
{
    public static class UiSystems
    {
        public static MainMenuUi MainMenu { get; private set; }

        // UiMM is unused

        public static LoadGameUi LoadGame { get; private set; }

        public static SaveGameUi SaveGame { get; private set; }

        public static InGameUi InGame { get; private set; }

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

        public static LogbookUi LogbookUi { get; private set; }

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

        public static HelpInventoryUi HelpInventory { get; private set; }

        public static PartyQuickviewUi PartyQuickview { get; private set; }

        public static OptionsUi Options { get; private set; }

        public static KeyManagerUi KeyManager { get; private set; }

        public static HelpManagerUi HelpManager { get; private set; }

        public static SliderUi Slider { get; private set; }

        public static WrittenUi Written { get; private set; }

        public static CharmapUi Charmap { get; private set; }

        public static void Startup(GameConfig config)
        {
            MainMenu = new MainMenuUi();
            SaveGame = new SaveGameUi();
            LoadGame = new LoadGameUi();
            UtilityBar = new UtilityBarUi();
            DungeonMaster = new DungeonMasterUi();
            CharSheet = new CharSheetUi();
            InGame = new InGameUi();
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
    }

    public class SliderUi
    {
    }

    public class HelpManagerUi
    {
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

    public class HelpInventoryUi
    {
    }

    public class CampingUi
    {
    }

    public class FormationUi
    {
    }

    public class PartyUi
    {
        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateAndShowMaybe()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PccPortraitUi
    {
    }

    public class PartyPoolUi
    {
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
    }

    public class HelpUi
    {
    }

    public class RandomEncounterUi
    {
    }

    public class WorldMapUi
    {
    }

    public class FocusManagerUi
    {
    }

    public class TextDialogUi
    {
    }

    public class PopupUi
    {
    }

    public class TownMapUi
    {
    }

    public class ScrollpaneUi
    {
    }

    public class LogbookUi
    {
    }

    public class TooltipUi
    {
    }

    public class CharSheetUi
    {
        public void Hide()
        {
            // TODO throw new System.NotImplementedException();
        }
    }

    public class PCCreationUi
    {
        public void Start()
        {
            throw new System.NotImplementedException(); // TODO
        }
    }

    public class DialogUi
    {
    }

    public class SlideUi
    {
    }

    public class CombatUi
    {
    }

    public class WorldMapRandomEncounterUi
    {
        public void StartRandomEncounterTimer()
        {
            throw new System.NotImplementedException();
        }
    }

    public class TBUi
    {
    }

    public class AnimUi
    {
    }

    public class TurnBasedUi
    {
    }

    public class InGameSelectUi
    {
    }

    public class InGameUi
    {
        public void ResetInput()
        {
            // TODO throw new System.NotImplementedException();
        }

        [TempleDllLocation(0x10113CD0)]
        public int sub_10113CD0()
        {
            // TODO
            return 0;
        }

        [TempleDllLocation(0x10113D40)]
        public int sub_10113D40(int unk)
        {
            // TODO
            return 1;
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