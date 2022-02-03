namespace OpenTemple.Core.Platform;

/// <summary>
/// Enumeration for virtual keys.
/// </summary>
public enum VirtualKey
    : uint
{
    VK_LBUTTON = 0x1, // [LButton] = 001
    VK_RBUTTON = 0x2, // [RButton] = 002
    VK_CANCEL = 0x3, // [Cancel] = 003
    VK_MBUTTON = 0x4, // [MButton] = 004    ' NOT contiguous with L & RBUTTON
    VK_XBUTTON1 = 0x5, // [XButton1] = 005   ' NOT contiguous with L & RBUTTON
    VK_XBUTTON2 = 0x6, // [XButton2] = 006   ' NOT contiguous with L & RBUTTON
    // UNASSIGNED // = 0x7,    // ''UNASSIGNED = 007

    VK_BACK = 0x8, // [Back] = 008
    VK_TAB = 0x9, // [Tab] = 009

    // RESERVED // = 0xA,        // [LineFeed] = 010
    // RESERVED // = 0xB,        // ''UNASSIGNED = 011
    VK_CLEAR = 0xC, // [Clear] = 012
    VK_RETURN = 0xD, // [Return] = 013

    // UNDEFINED //       // [Enter] = 013
    VK_SHIFT = 0x10, // [ShiftKey] = 016
    VK_CONTROL = 0x11, // [ControlKey] = 017
    VK_MENU = 0x12, // [Menu] = 018
    VK_PAUSE = 0x13, // [Pause] = 019
    VK_CAPITAL = 0x14, // [Capital] = 020
    // UNDEFINED //       // [CapsLock] = 020

    VK_HANGUL = 0x15, // [HangulMode] = 021
    VK_HANGEUL = 0x15, // [HanguelMode] = 021 ' old name (compatibility)
    VK_KANA = 0x15, // [KanaMode] = 021
    VK_JUNJA = 0x17, // [JunjaMode] = 023
    VK_FINAL = 0x18, // [FinalMode] = 024
    VK_KANJI = 0x19, // [KanjiMode] = 025
    VK_HANJA = 0x19, // [HanjaMode] = 025

    VK_ESCAPE = 0x1B, // [Escape] = 027

    VK_CONVERT = 0x1C, // [IMEConvert] = 028
    VK_NONCONVERT = 0x1D, // [IMENonconvert] = 029
    VK_ACCEPT = 0x1E, // [IMEAccept] = 030
    VK_MODECHANGE = 0x1F, // [IMEModeChange] = 031

    VK_SPACE = 0x20, // [Space] = 032
    VK_PRIOR = 0x21, // [Prior] = 033

    // UNDEFINED //       // [PageUp] = 033
    VK_NEXT = 0x22, // [Next] = 034

    // UNDEFINED //       // [PageDown] = 034
    VK_END = 0x23, // [End] = 035
    VK_HOME = 0x24, // [Home] = 036

    VK_LEFT = 0x25, // [Left] = 037
    VK_UP = 0x26, // [Up] = 038
    VK_RIGHT = 0x27, // [Right] = 039
    VK_DOWN = 0x28, // [Down] = 040

    VK_SELECT = 0x29, // [Select] = 041
    VK_PRINT = 0x2A, // [Print] = 042
    VK_EXECUTE = 0x2B, // [Execute] = 043
    VK_SNAPSHOT = 0x2C, // [Snapshot] = 044

    // UNDEFINED //       // [PrintScreen] = 044
    VK_INSERT = 0x2D, // [Insert] = 045
    VK_DELETE = 0x2E, // [Delete] = 046
    VK_HELP = 0x2F, // [Help] = 047

    VK_0 = 0x30, // [D0] = 048
    VK_1 = 0x31, // [D1] = 049
    VK_2 = 0x32, // [D2] = 050
    VK_3 = 0x33, // [D3] = 051
    VK_4 = 0x34, // [D4] = 052
    VK_5 = 0x35, // [D5] = 053
    VK_6 = 0x36, // [D6] = 054
    VK_7 = 0x37, // [D7] = 055
    VK_8 = 0x38, // [D8] = 056
    VK_9 = 0x39, // [D9] = 057

    // UNASSIGNED // = 0x40, to 0x4F, (058 to 064)

    VK_A = 0x41, // [A] = 065
    VK_B = 0x42, // [B] = 066
    VK_C = 0x43, // [C] = 067
    VK_D = 0x44, // [D] = 068
    VK_E = 0x45, // [E] = 069
    VK_F = 0x46, // [F] = 070
    VK_G = 0x47, // [G] = 071
    VK_H = 0x48, // [H] = 072
    VK_I = 0x49, // [I] = 073
    VK_J = 0x4A, // [J] = 074
    VK_K = 0x4B, // [K] = 075
    VK_L = 0x4C, // [L] = 076
    VK_M = 0x4D, // [M] = 077
    VK_N = 0x4E, // [N] = 078
    VK_O = 0x4F, // [O] = 079
    VK_P = 0x50, // [P] = 080
    VK_Q = 0x51, // [Q] = 081
    VK_R = 0x52, // [R] = 082
    VK_S = 0x53, // [S] = 083
    VK_T = 0x54, // [T] = 084
    VK_U = 0x55, // [U] = 085
    VK_V = 0x56, // [V] = 086
    VK_W = 0x57, // [W] = 087
    VK_X = 0x58, // [X] = 088
    VK_Y = 0x59, // [Y] = 089
    VK_Z = 0x5A, // [Z] = 090

    VK_LWIN = 0x5B, // [LWin] = 091
    VK_RWIN = 0x5C, // [RWin] = 092
    VK_APPS = 0x5D, // [Apps] = 093

    // RESERVED // = 0x5E,        // ''UNASSIGNED = 094
    VK_SLEEP = 0x5F, // [Sleep] = 095

    VK_NUMPAD0 = 0x60, // [NumPad0] = 096
    VK_NUMPAD1 = 0x61, // [NumPad1] = 097
    VK_NUMPAD2 = 0x62, // [NumPad2] = 098
    VK_NUMPAD3 = 0x63, // [NumPad3] = 099
    VK_NUMPAD4 = 0x64, // [NumPad4] = 100
    VK_NUMPAD5 = 0x65, // [NumPad5] = 101
    VK_NUMPAD6 = 0x66, // [NumPad6] = 102
    VK_NUMPAD7 = 0x67, // [NumPad7] = 103
    VK_NUMPAD8 = 0x68, // [NumPad8] = 104
    VK_NUMPAD9 = 0x69, // [NumPad9] = 105

    VK_MULTIPLY = 0x6A, // [Multiply] = 106
    VK_ADD = 0x6B, // [Add] = 107
    VK_SEPARATOR = 0x6C, // [Separator] = 108
    VK_SUBTRACT = 0x6D, // [Subtract] = 109
    VK_DECIMAL = 0x6E, // [Decimal] = 110
    VK_DIVIDE = 0x6F, // [Divide] = 111

    VK_F1 = 0x70, // [F1] = 112
    VK_F2 = 0x71, // [F2] = 113
    VK_F3 = 0x72, // [F3] = 114
    VK_F4 = 0x73, // [F4] = 115
    VK_F5 = 0x74, // [F5] = 116
    VK_F6 = 0x75, // [F6] = 117
    VK_F7 = 0x76, // [F7] = 118
    VK_F8 = 0x77, // [F8] = 119
    VK_F9 = 0x78, // [F9] = 120
    VK_F10 = 0x79, // [F10] = 121
    VK_F11 = 0x7A, // [F11] = 122
    VK_F12 = 0x7B, // [F12] = 123

    VK_F13 = 0x7C, // [F13] = 124
    VK_F14 = 0x7D, // [F14] = 125
    VK_F15 = 0x7E, // [F15] = 126
    VK_F16 = 0x7F, // [F16] = 127
    VK_F17 = 0x80, // [F17] = 128
    VK_F18 = 0x81, // [F18] = 129
    VK_F19 = 0x82, // [F19] = 130
    VK_F20 = 0x83, // [F20] = 131
    VK_F21 = 0x84, // [F21] = 132
    VK_F22 = 0x85, // [F22] = 133
    VK_F23 = 0x86, // [F23] = 134
    VK_F24 = 0x87, // [F24] = 135

    // UNASSIGNED // = 0x88, to 0x8F, (136 to 143)

    VK_NUMLOCK = 0x90, // [NumLock] = 144
    VK_SCROLL = 0x91, // [Scroll] = 145

    VK_OEM_NEC_EQUAL = 0x92, // [NEC_Equal] = 146    ' NEC PC-9800 kbd definitions "=" key on numpad
    VK_OEM_FJ_JISHO = 0x92, // [Fujitsu_Masshou] = 146    ' Fujitsu/OASYS kbd definitions "Dictionary" key
    VK_OEM_FJ_MASSHOU = 0x93, // [Fujitsu_Masshou] = 147    ' Fujitsu/OASYS kbd definitions "Unregister word" key
    VK_OEM_FJ_TOUROKU = 0x94, // [Fujitsu_Touroku] = 148    ' Fujitsu/OASYS kbd definitions "Register word" key
    VK_OEM_FJ_LOYA = 0x95, // [Fujitsu_Loya] = 149    ' Fujitsu/OASYS kbd definitions "Left OYAYUBI" key
    VK_OEM_FJ_ROYA = 0x96, // [Fujitsu_Roya] = 150    ' Fujitsu/OASYS kbd definitions "Right OYAYUBI" key

    // UNASSIGNED // = 0x97, to 0x9F, (151 to 159)

    // NOTE :: 0xA0, to 0xA5, (160 to 165) = left and right Alt, Ctrl and Shift virtual keys.
    // NOTE :: Used only as parameters to GetAsyncKeyState() and GetKeyState().
    // NOTE :: No other API or message will distinguish left and right keys in this way.
    VK_LSHIFT = 0xA0, // [LShiftKey] = 160
    VK_RSHIFT = 0xA1, // [RShiftKey] = 161
    VK_LCONTROL = 0xA2, // [LControlKey] = 162
    VK_RCONTROL = 0xA3, // [RControlKey] = 163
    VK_LMENU = 0xA4, // [LMenu] = 164
    VK_RMENU = 0xA5, // [RMenu] = 165

    VK_BROWSER_BACK = 0xA6, // [BrowserBack] = 166
    VK_BROWSER_FORWARD = 0xA7, // [BrowserForward] = 167
    VK_BROWSER_REFRESH = 0xA8, // [BrowserRefresh] = 168
    VK_BROWSER_STOP = 0xA9, // [BrowserStop] = 169
    VK_BROWSER_SEARCH = 0xAA, // [BrowserSearch] = 170
    VK_BROWSER_FAVORITES = 0xAB, // [BrowserFavorites] = 171
    VK_BROWSER_HOME = 0xAC, // [BrowserHome] = 172

    VK_VOLUME_MUTE = 0xAD, // [VolumeMute] = 173
    VK_VOLUME_DOWN = 0xAE, // [VolumeDown] = 174
    VK_VOLUME_UP = 0xAF, // [VolumeUp] = 175

    VK_MEDIA_NEXT_TRACK = 0xB0, // [MediaNextTrack] = 176
    VK_MEDIA_PREV_TRACK = 0xB1, // [MediaPreviousTrack] = 177
    VK_MEDIA_STOP = 0xB2, // [MediaStop] = 178
    VK_MEDIA_PLAY_PAUSE = 0xB3, // [MediaPlayPause] = 179

    VK_LAUNCH_MAIL = 0xB4, // [LaunchMail] = 180
    VK_LAUNCH_MEDIA_SELECT = 0xB5, // [SelectMedia] = 181
    VK_LAUNCH_APP1 = 0xB6, // [LaunchApplication1] = 182
    VK_LAUNCH_APP2 = 0xB7, // [LaunchApplication2] = 183
    // UNASSIGNED // = 0xB8,   // ''UNASSIGNED = 184
    // UNASSIGNED // = 0xB9,   // ''UNASSIGNED = 185

    VK_OEM_1 = 0xBA, // [Oem1] = 186           ' ";:" for USA

    // UNDEFINED //       // [OemSemicolon] = 186       ' ";:" for USA
    VK_OEM_PLUS = 0xBB, // [Oemplus] = 187        ' "+" any country
    VK_OEM_COMMA = 0xBC, // [Oemcomma] = 188       ' "," any country
    VK_OEM_MINUS = 0xBD, // [OemMinus] = 189       ' "-" any country
    VK_OEM_PERIOD = 0xBE, // [OemPeriod] = 190      ' "." any country
    VK_OEM_2 = 0xBF, // [Oem2] = 191           ' "/?" for USA

    // UNDEFINED //       // [OemQuestion] = 191    ' "/?" for USA
    // UNDEFINED //       // [Oemtilde] = 192       ' "'~" for USA
    VK_OEM_3 = 0xC0, // [Oem3] = 192           ' "'~" for USA

    // RESERVED // = 0xC1, to 0xD7, (193 to 215)
    // UNASSIGNED // = 0xD8, to 0xDA, (216 to 218)

    VK_OEM_4 = 0xDB, // [Oem4] = 219           ' "[{" for USA

    // UNDEFINED //       // [OemOpenBrackets] = 219    ' "[{" for USA
    // UNDEFINED //       // [OemPipe] = 220        ' "\|" for USA
    VK_OEM_5 = 0xDC, // [Oem5] = 220           ' "\|" for USA
    VK_OEM_6 = 0xDD, // [Oem6] = 221           ' "]}" for USA

    // UNDEFINED //       // [OemCloseBrackets] = 221   ' "]}" for USA
    // UNDEFINED //       // [OemQuotes] = 222      ' "'"" for USA
    VK_OEM_7 = 0xDE, // [Oem7] = 222           ' "'"" for USA
    VK_OEM_8 = 0xDF, // [Oem8] = 223

    // RESERVED // = 0xE0,        // ''UNASSIGNED = 224
    VK_OEM_AX = 0xE1, // [OEMAX] = 225          ' "AX" key on Japanese AX kbd

    // UNDEFINED //       // [OemBackslash] = 226       ' "<>" or "\|" on RT 102-key kbd
    VK_OEM_102 = 0xE2, // [Oem102] = 226         ' "<>" or "\|" on RT 102-key kbd
    VK_ICO_HELP = 0xE3, // [ICOHelp] = 227        ' Help key on ICO
    VK_ICO_00 = 0xE4, // [ICO00] = 228          ' 00 key on ICO

    VK_PROCESSKEY = 0xE5, // [ProcessKey] = 229
    VK_ICO_CLEAR = 0xE6, // [ICOClear] = 230
    VK_PACKET = 0xE7, // [Packet] = 231
    // UNASSIGNED // = 0xE8,   // ''UNASSIGNED = 232

    // NOTE :: Nokia/Ericsson definitions
    VK_OEM_RESET = 0xE9, // [OEMReset] = 233
    VK_OEM_JUMP = 0xEA, // [OEMJump] = 234
    VK_OEM_PA1 = 0xEB, // [OEMPA1] = 235
    VK_OEM_PA2 = 0xEC, // [OEMPA2] = 236
    VK_OEM_PA3 = 0xED, // [OEMPA3] = 237
    VK_OEM_WSCTRL = 0xEE, // [OEMWSCtrl] = 238
    VK_OEM_CUSEL = 0xEF, // [OEMCUSel] = 239
    VK_OEM_ATTN = 0xF0, // [OEMATTN] = 240
    VK_OEM_FINISH = 0xF1, // [OEMFinish] = 241
    VK_OEM_COPY = 0xF2, // [OEMCopy] = 242
    VK_OEM_AUTO = 0xF3, // [OEMAuto] = 243
    VK_OEM_ENLW = 0xF4, // [OEMENLW] = 244
    VK_OEM_BACKTAB = 0xF5, // [OEMBackTab] = 245

    VK_ATTN = 0xF6, // [Attn] = 246
    VK_CRSEL = 0xF7, // [Crsel] = 247
    VK_EXSEL = 0xF8, // [Exsel] = 248
    VK_EREOF = 0xF9, // [EraseEof] = 249
    VK_PLAY = 0xFA, // [Play] = 250
    VK_ZOOM = 0xFB, // [Zoom] = 251
    VK_NONAME = 0xFC, // [NoName] = 252
    VK_PA1 = 0xFD, // [Pa1] = 253
    VK_OEM_CLEAR = 0xFE, // [OemClear] = 254
}