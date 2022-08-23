﻿
using OpenTemple.Core.Startup.Discovery;
using static SDL2.SDL;

namespace OpenTemple.Core.Hotkeys;

[TempleDllLocation(0x102f9c88)]
[AutoRegister]
public static class InGameHotKey
{
    public static readonly Hotkey TogglePartySelection1 = Hotkey.Build("toggle_select_party_1").Primary(SDL_Scancode.SDL_SCANCODE_1, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection2 = Hotkey.Build("toggle_select_party_2").Primary(SDL_Scancode.SDL_SCANCODE_2, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection3 = Hotkey.Build("toggle_select_party_3").Primary(SDL_Scancode.SDL_SCANCODE_3, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection4 = Hotkey.Build("toggle_select_party_4").Primary(SDL_Scancode.SDL_SCANCODE_4, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection5 = Hotkey.Build("toggle_select_party_5").Primary(SDL_Scancode.SDL_SCANCODE_5, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection6 = Hotkey.Build("toggle_select_party_6").Primary(SDL_Scancode.SDL_SCANCODE_6, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection7 = Hotkey.Build("toggle_select_party_7").Primary(SDL_Scancode.SDL_SCANCODE_7, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection8 = Hotkey.Build("toggle_select_party_8").Primary(SDL_Scancode.SDL_SCANCODE_8, KeyModifier.Shift).Build();
    public static readonly Hotkey TogglePartySelection9 = Hotkey.Build("toggle_select_party_9").Primary(SDL_Scancode.SDL_SCANCODE_9, KeyModifier.Shift).Build();
    public static readonly Hotkey SelectChar1 = Hotkey.Build("select_char_1").Primary(SDL_Scancode.SDL_SCANCODE_1).Build();
    public static readonly Hotkey SelectChar2 = Hotkey.Build("select_char_2").Primary(SDL_Scancode.SDL_SCANCODE_2).Build();
    public static readonly Hotkey SelectChar3 = Hotkey.Build("select_char_3").Primary(SDL_Scancode.SDL_SCANCODE_3).Build();
    public static readonly Hotkey SelectChar4 = Hotkey.Build("select_char_4").Primary(SDL_Scancode.SDL_SCANCODE_4).Build();
    public static readonly Hotkey SelectChar5 = Hotkey.Build("select_char_5").Primary(SDL_Scancode.SDL_SCANCODE_5).Build();
    public static readonly Hotkey SelectChar6 = Hotkey.Build("select_char_6").Primary(SDL_Scancode.SDL_SCANCODE_6).Build();
    public static readonly Hotkey SelectChar7 = Hotkey.Build("select_char_7").Primary(SDL_Scancode.SDL_SCANCODE_7).Build();
    public static readonly Hotkey SelectChar8 = Hotkey.Build("select_char_8").Primary(SDL_Scancode.SDL_SCANCODE_8).Build();
    public static readonly Hotkey SelectChar9 = Hotkey.Build("select_char_9").Primary(SDL_Scancode.SDL_SCANCODE_9).Build();

    public static readonly Hotkey AssignGroup1 = Hotkey.Build("assign_group_1").Primary(SDL_Scancode.SDL_SCANCODE_F1, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup2 = Hotkey.Build("assign_group_2").Primary(SDL_Scancode.SDL_SCANCODE_F2, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup3 = Hotkey.Build("assign_group_3").Primary(SDL_Scancode.SDL_SCANCODE_F3, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup4 = Hotkey.Build("assign_group_4").Primary(SDL_Scancode.SDL_SCANCODE_F4, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup5 = Hotkey.Build("assign_group_5").Primary(SDL_Scancode.SDL_SCANCODE_F5, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup6 = Hotkey.Build("assign_group_6").Primary(SDL_Scancode.SDL_SCANCODE_F6, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup7 = Hotkey.Build("assign_group_7").Primary(SDL_Scancode.SDL_SCANCODE_F7, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup8 = Hotkey.Build("assign_group_8").Primary(SDL_Scancode.SDL_SCANCODE_F8, KeyModifier.Control).Build();
    public static readonly Hotkey AssignGroup9 = Hotkey.Build("assign_group_9").Primary(SDL_Scancode.SDL_SCANCODE_F9, KeyModifier.Control).Build();

    public static readonly Hotkey RecallGroup1 = Hotkey.Build("select_group_1").Primary(SDL_Scancode.SDL_SCANCODE_F1).Build();
    public static readonly Hotkey RecallGroup2 = Hotkey.Build("select_group_2").Primary(SDL_Scancode.SDL_SCANCODE_F2).Build();
    public static readonly Hotkey RecallGroup3 = Hotkey.Build("select_group_3").Primary(SDL_Scancode.SDL_SCANCODE_F3).Build();
    public static readonly Hotkey RecallGroup4 = Hotkey.Build("select_group_4").Primary(SDL_Scancode.SDL_SCANCODE_F4).Build();
    public static readonly Hotkey RecallGroup5 = Hotkey.Build("select_group_5").Primary(SDL_Scancode.SDL_SCANCODE_F5).Build();
    public static readonly Hotkey RecallGroup6 = Hotkey.Build("select_group_6").Primary(SDL_Scancode.SDL_SCANCODE_F6).Build();
    public static readonly Hotkey RecallGroup7 = Hotkey.Build("select_group_7").Primary(SDL_Scancode.SDL_SCANCODE_F7).Build();
    public static readonly Hotkey RecallGroup8 = Hotkey.Build("select_group_8").Primary(SDL_Scancode.SDL_SCANCODE_F8).Build();
    public static readonly Hotkey RecallGroup9 = Hotkey.Build("select_group_9").Primary(SDL_Scancode.SDL_SCANCODE_F9).Build();

    public static readonly Hotkey SelectAll = Hotkey.Build("select_all").Primary(SDL_Scancode.SDL_SCANCODE_GRAVE).Build();
    public static readonly Hotkey CenterOnChar = Hotkey.Build("center_on_char").Primary(SDL_Scancode.SDL_SCANCODE_HOME).Build();

    public static readonly Hotkey ToggleConsole = Hotkey.Build("toggle_console").Primary(SDL_Scancode.SDL_SCANCODE_GRAVE, KeyModifier.Shift).Build();
    public static readonly Hotkey ToggleMainMenu = Hotkey.Build("toggle_mainmenu").Primary(SDL_Scancode.SDL_SCANCODE_ESCAPE).Build();
    public static readonly Hotkey QuickLoad = Hotkey.Build("quickload").Primary(SDL_Scancode.SDL_SCANCODE_F9).Build();
    public static readonly Hotkey QuickSave = Hotkey.Build("quicksave").Primary(SDL_Scancode.SDL_SCANCODE_F12).Secondary(SDL_Scancode.SDL_SCANCODE_F11).Build();
    public static readonly Hotkey Quit = Hotkey.Build("quit").Primary(SDL_Scancode.SDL_SCANCODE_Q, KeyModifier.Alt).Build();
    public static readonly Hotkey Screenshot = Hotkey.Build("screenshot").Primary(SDL_Scancode.SDL_SCANCODE_PRINTSCREEN).Build();

    public static readonly Hotkey ScrollUp = Hotkey.Build("scroll_up").Primary(SDL_Scancode.SDL_SCANCODE_UP).Held().Build();
    public static readonly Hotkey ScrollDown = Hotkey.Build("scroll_down").Primary(SDL_Scancode.SDL_SCANCODE_DOWN).Held().Build();
    public static readonly Hotkey ScrollLeft = Hotkey.Build("scroll_left").Primary(SDL_Scancode.SDL_SCANCODE_LEFT).Held().Build();
    public static readonly Hotkey ScrollRight = Hotkey.Build("scroll_right").Primary(SDL_Scancode.SDL_SCANCODE_RIGHT).Held().Build();
    public static readonly Hotkey ObjectHighlight = Hotkey.Build("object_highlight").Primary(SDL_Scancode.SDL_SCANCODE_TAB).Held().Build();

    public static readonly Hotkey ShowInventory = Hotkey.Build("show_inventory").Primary(SDL_Scancode.SDL_SCANCODE_I).Build();
    public static readonly Hotkey ShowLogbook = Hotkey.Build("show_logbook").Primary(SDL_Scancode.SDL_SCANCODE_L).Build();
    public static readonly Hotkey ShowMap = Hotkey.Build("show_map").Primary(SDL_Scancode.SDL_SCANCODE_M).Build();
    public static readonly Hotkey ShowFormation = Hotkey.Build("show_formation").Primary(SDL_Scancode.SDL_SCANCODE_F).Build();
    public static readonly Hotkey Rest = Hotkey.Build("rest").Primary(SDL_Scancode.SDL_SCANCODE_R).Build();
    public static readonly Hotkey ShowHelp = Hotkey.Build("show_help").Primary(SDL_Scancode.SDL_SCANCODE_H).Build();
    public static readonly Hotkey ShowOptions = Hotkey.Build("show_options").Primary(SDL_Scancode.SDL_SCANCODE_O).Build();
    public static readonly Hotkey ToggleCombat = Hotkey.Build("toggle_combat").Primary(SDL_Scancode.SDL_SCANCODE_C).Build();
    public static readonly Hotkey EndTurn = Hotkey.Build("end_turn").Primary(SDL_Scancode.SDL_SCANCODE_SPACE).Secondary(SDL_Scancode.SDL_SCANCODE_RETURN).Build();
    public static readonly Hotkey EndTurnNonParty = Hotkey.Build("end_turn_non_party").Primary(SDL_Scancode.SDL_SCANCODE_RETURN, KeyModifier.Shift).Build();

    public static readonly Hotkey ToggleRun = Hotkey.Build("toggle_run").Primary(KeyModifier.Control).Build();
}