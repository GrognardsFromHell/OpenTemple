using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using OpenTemple.Core.GameObjects;
using OpenTemple.Core.GFX.TextRendering;
using OpenTemple.Core.IO.SaveGames.UiState;
using OpenTemple.Core.Platform;
using OpenTemple.Core.Systems;
using OpenTemple.Core.Systems.Anim;
using OpenTemple.Core.Systems.Dialog;
using OpenTemple.Core.Systems.ObjScript;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Ui.CharSheet;
using OpenTemple.Core.Ui.Events;
using OpenTemple.Core.Ui.FlowModel;
using OpenTemple.Core.Ui.Widgets;
using OpenTemple.Core.Utils;
using static SDL2.SDL;

namespace OpenTemple.Core.Ui.Dialog;

public class DialogUi : IResetAwareSystem, ISaveGameAwareUi
{
    private const int ContentWidth = 550;

    private const int MaxDialogLines = 200;

    [TempleDllLocation(0x10bec280)]
    private const string NPCLineTextStyle = "dialog-ui-line-npc";

    [TempleDllLocation(0x10bea290)]
    private const string PcLineTextStyle = "dialog-ui-line-player";

    [TempleDllLocation(0x1014bb50)]
    public bool IsVisible => (uiDialogFlags & 1) == 0 || _mainWindow.IsInTree;

    [TempleDllLocation(0x1014bac0)]
    [TempleDllLocation(0x10BEC348)]
    public bool IsConversationOngoing { get; private set; }

    private static readonly WidgetButtonStyle HeadButtonStyle = new()
    {
        NormalImagePath = "art/interface/dialog/head_normal.tga",
        HoverImagePath = "art/interface/dialog/head_hovered.tga",
        PressedImagePath = "art/interface/dialog/head_pressed.tga",
        DisabledImagePath = "art/interface/dialog/head_disabled.tga"
    };

    [TempleDllLocation(0x10bea2e4)]
    private WidgetContainer _mainWindow;

    [TempleDllLocation(0x10bec19c)]
    private WidgetScrollBar _historyScollbar;

    [TempleDllLocation(0x10bec210)]
    private WidgetButton _showHistoryButton;

    [TempleDllLocation(0x10bec198)]
    private WidgetContainer uiDialogWnd2Id;

    [TempleDllLocation(0x10bec20c)]
    private int dword_10BEC20C;

    [TempleDllLocation(0x10be9ff0)]
    private int maximizedBackdropY;

    private WidgetContainer _dialogLinesContainer;
    private readonly TextEngine _textEngine;

    public DialogUi()
    {
        Stub.TODO();

        _textEngine = Tig.RenderingDevice.TextEngine;

        GameSystems.AI.SetDialogFunctions(CancelDialog, ShowTextBubble);
        GameSystems.Script.SetDialogFunctions(ShowTextBubble);

        CreateWidgets();

        Globals.UiManager.OnCanvasSizeChanged += UpdatePosition;
        UpdatePosition(Globals.UiManager.CanvasSize);
    }

    [TempleDllLocation(0x10bec204)]
    private WidgetContainer _responseContainer;

    [TempleDllLocation(0x10bec228)]
    [TempleDllLocation(0x10be9b38)]
    [TempleDllLocation(0x10bec214)]
    [TempleDllLocation(0x10BEA8A8)]
    private DialogResponseList _responseList;

    private WidgetImage _backdropHistory;
    private WidgetImage _backdrop1Line;
    private WidgetImage _backdrop2Line;
    private WidgetImage _backdrop3Line;
    private WidgetImage _splitter;

    [TempleDllLocation(0x1014d9b0)]
    [TempleDllLocation(0x1014d5d0)]
    [TempleDllLocation(0x1014c50c)]
    private void CreateWidgets()
    {
        // Primarily we just do this for loading styles
        var doc = WidgetDoc.Load("ui/dialog_ui.json");

        // Begin top level window
        // Created @ 0x1014dacc
        _mainWindow = doc.GetRootContainer();
        // uiDialogWndId.OnHandleMessage += 0x1014bd00;
        // uiDialogWndId.OnBeforeRender += 0x1014bbb0;
        _mainWindow.OnBeforeRender += UpdateLayout;
        _mainWindow.OnKeyUp += SelectResponseByKey;

        // This renders the NPC's dialog lines
        _dialogLinesContainer = new WidgetContainer(14, 0, ContentWidth, 0);
        _dialogLinesContainer.OnBeforeRender += RenderDialogLines;
        _mainWindow.Add(_dialogLinesContainer);

        _backdropHistory = new WidgetImage("art/interface/dialog/dialog_backdrop.img");
        _backdropHistory.Y = 18;
        _mainWindow.AddContent(_backdropHistory);
        _backdrop1Line = new WidgetImage("art/interface/dialog/dialog_backdrop_mini_1.img");
        _backdrop1Line.Y = Globals.Config.Co8 ? 77 : 126;
        _mainWindow.AddContent(_backdrop1Line);
        _backdrop2Line = new WidgetImage("art/interface/dialog/dialog_backdrop_mini_2.img");
        _backdrop2Line.Y = 112;
        _mainWindow.AddContent(_backdrop2Line);
        _backdrop3Line = new WidgetImage("art/interface/dialog/dialog_backdrop_mini_3.img");
        _backdrop3Line.Y = 94;
        _mainWindow.AddContent(_backdrop3Line);

        _historyScollbar = new WidgetScrollBar(new Rectangle(592, 28, 13, 126));
        // uiDialogScrollbarId.OnHandleMessage += 0x101fa410;
        // uiDialogScrollbarId.OnBeforeRender += 0x101fa1b0;
        _mainWindow.Add(_historyScollbar);

        _showHistoryButton = new WidgetButton(new Rectangle(581, 1, 23, 18));
        _showHistoryButton.SetStyle(HeadButtonStyle);
        _showHistoryButton.AddClickListener(OnHeadButtonClicked);
        // uiDialogButton1Id.OnBeforeRender += 0x1014be30;
        // uiDialogButton1Id.OnRenderTooltip += 0x100027f0;
        _mainWindow.Add(_showHistoryButton);

        _responseContainer = new WidgetContainer(new Rectangle(7, 153, 594, 139));

        _splitter = new WidgetImage("art/interface/dialog/response_split_bar.img");
        _splitter.FixedSize = new Size(599, 9);
        _responseContainer.AddContent(_splitter);

        _responseList = new DialogResponseList(new RectangleF(1, 9, 594, _responseContainer.ComputePreferredBorderAreaSize().Height - 9));
        _responseList.Name = "dialog_response_list";
        _responseList.OnResponseSelected += OnResponseClicked;
        _responseContainer.Add(_responseList);

        _mainWindow.Add(_responseContainer);

        uiDialogWnd2Id = new WidgetContainer(new Rectangle(0, 0, 1024, 768));
        // uiDialogWnd2Id.OnHandleMessage += 0x1014bdc0;
    }

    private void UpdatePosition(Size screenSize)
    {
        _mainWindow.X = 9;
        _mainWindow.Y = screenSize.Height - 374;
    }

    [TempleDllLocation(0x1014cb20)]
    private void OnHeadButtonClicked()
    {
        if (UiSystems.HelpManager.IsSelectingHelpTarget)
        {
            UiSystems.HelpManager.ShowPredefinedTopic(19);
        }
        else
        {
            ToggleHistory();
        }
    }

    [TempleDllLocation(0x1014d560)]
    private void OnResponseClicked(int responseIndex)
    {
        if (UiSystems.HelpManager.IsSelectingHelpTarget)
        {
            UiSystems.HelpManager.ShowPredefinedTopic(19);
        }
        else
        {
            if (responseIndex < dialog_slot_idx.PcLineText.Length)
            {
                UiDialogPcReplyLineExecute(dialog_slot_idx, responseIndex);
            }
        }
    }

    [TempleDllLocation(0x1014d510)]
    private void SelectResponseByKey(KeyboardEvent e)
    {
        if (!IsConversationOngoing)
        {
            return;
        }

        void SelectResponse(int responseIdx)
        {
            if (responseIdx < dialog_slot_idx.PcLineText.Length)
            {
                UiDialogPcReplyLineExecute(dialog_slot_idx, responseIdx);
                e.StopPropagation();
            }
        }

        // Allows selecting PC responses by pressing the associated number
        switch (e.VirtualKey)
        {
            case SDL_Keycode.SDLK_1:
                SelectResponse(0);
                break;
            case SDL_Keycode.SDLK_2:
                SelectResponse(1);
                break;
            case SDL_Keycode.SDLK_3:
                SelectResponse(2);
                break;
            case SDL_Keycode.SDLK_4:
                SelectResponse(3);
                break;
            case SDL_Keycode.SDLK_5:
                SelectResponse(4);
                break;
            case SDL_Keycode.SDLK_6:
                SelectResponse(5);
                break;
            case SDL_Keycode.SDLK_7:
                SelectResponse(6);
                break;
            case SDL_Keycode.SDLK_8:
                SelectResponse(7);
                break;
            case SDL_Keycode.SDLK_9:
                SelectResponse(8);
                break;
            default:
                break;
        }
    }

    [TempleDllLocation(0x1014d1a0)]
    private void UiDialogPcReplyLineExecute(DialogState state, int responseIdx)
    {
        GameSystems.TextBubble.Remove(state.NPC);
        GameSystems.Dialog.StopCurrentVoiceLine();
        DialogLineAppend(state.Pc, state.PcLineText[responseIdx]);
        GameSystems.Dialog.DialogChoiceParse(state, responseIdx);

        switch (state.ActionType)
        {
            case 0:
                UiDialogGetResponsesFromDialogState(state);
                break;
            case 1:
                CancelDialog(state.Pc);
                break;
            case 2:
                UiSystems.CharSheet.State = CharInventoryState.Bartering;
                UiSystems.CharSheet.Looting.Show(state.NPC);
                UiSystems.CharSheet.Show(state.Pc);
                DialogCreateBubble(state.NPC, state.Pc, 0, -1, state.NPCLineText, state.SpeechId);
                CancelDialog(state.Pc);
                break;
            case 3:
                DialogCreateBubble(state.NPC, state.Pc, 0, -1, state.NPCLineText, state.SpeechId);
                CancelDialog(state.Pc);
                break;
            case 7:
                if (GameSystems.Party.IsInParty(state.Pc))
                {
                    UiDialogResetResponses();
                }

                break;
            case 4:
            case 5:
            case 6:
            case 8:
                if (GameSystems.Party.IsInParty(state.Pc))
                {
                    UiDialogResetResponses();
                }

                break;
            default:
                break;
        }
    }

    [TempleDllLocation(0x1014bbb0)]
    private void RenderDialogLines()
    {
        var rect = _dialogLinesContainer.GetContentArea();
        rect.Y += rect.Height - 4;

        var scrollMax = _historyScollbar.Max;
        var scrollValue = _historyScollbar.GetValue();

        var v4 = _lineHistory.Count - 1;
        if (ShowDialogHistory && scrollMax > scrollValue)
        {
            var v5 = scrollMax;

            while (v4 > 0)
            {
                v4--;
                if (--v5 <= scrollValue)
                {
                    break;
                }
            }
        }

        for (; v4 >= 0; v4--)
        {
            var line = _lineHistory[v4];

            var paragraph = new Paragraph();
            paragraph.AddStyle(line.IsPcLine ? PcLineTextStyle : NPCLineTextStyle);
            paragraph.AppendContent(line.Text);

            using var layout = _textEngine.CreateTextLayout(
                paragraph,
                550,
                1000
            );
            rect.Height = (int) Math.Ceiling(layout.OverallHeight);
            rect.Y -= (int) Math.Ceiling(layout.OverallHeight);
            if (rect.Y < dword_10BEC20C)
            {
                break;
            }

            _textEngine.RenderTextLayout(
                rect.X,
                rect.Y,
                layout
            );

            if (!ShowDialogHistory)
            {
                break;
            }
        }
    }


    [TempleDllLocation(0x1014ca20)]
    public void Hide()
    {
        if (dword_10BEC344)
        {
            dword_10BEC344 = false;
            Tig.Sound.SetVolume(tig_sound_type.TIG_ST_MUSIC, 80 * uiDialogMusicVolume / 100);
        }

        uiDialogFlags = 1;
        Globals.UiManager.RemoveWindow(uiDialogWnd2Id);
        Globals.UiManager.RemoveWindow(_mainWindow);
        UiSystems.UtilityBar.HistoryUi.UpdateWidgetVisibility(); // Show the dialog history button again
    }

    [TempleDllLocation(0x1014cde0)]
    public void ShowTextBubble(GameObject critter, GameObject speakingto, string text, int speechid)
    {
        DialogCreateBubble(critter, speakingto, critter.IsNPC() ? 2 : 0, -1, text, speechid);
    }

    // This replaces calling 0x10115040 with 0 (popping the current dialog scene)
    public void CancelDialog()
    {
        if (dialog_slot_idx != null)
        {
            CancelDialog(dialog_slot_idx.Pc);
        }
    }

    [TempleDllLocation(0x1014BA40)]
    public void CancelDialog(GameObject obj)
    {
        if (dialog_slot_idx != null && dialog_slot_idx.Pc == obj)
        {
            var currentDialog = dialog_slot_idx;
            dialog_slot_idx = null;

            if (GameSystems.Party.IsInParty(obj))
            {
                UiDialogHide_10115210 /*0x10115210*/();
                IsConversationOngoing = false;
            }

            GameSystems.Dialog.Free(ref currentDialog.DialogScript);
            GameSystems.TextBubble.SetDuration(currentDialog.NPC, -1);
            GameSystems.Dialog.EndDialog(currentDialog);
        }
    }

    [TempleDllLocation(0x10115210)]
    private void UiDialogHide_10115210()
    {
        UiSystems.Tooltip.TooltipsEnabled = true;
        Hide();
        CancelDialog();
    }

    [TempleDllLocation(0x1014bad0)]
    public void DialogHideForPartyMember(GameObject obj)
    {
        dialog_slot_idx = null;
        if (GameSystems.Party.IsInParty(obj))
        {
            UiDialogHide_10115210();
            IsConversationOngoing = false;
        }
    }

    [TempleDllLocation(0x1014BFF0)]
    public void sub_1014BFF0(GameObject obj)
    {
        if (GameSystems.Party.IsInParty(obj))
        {
            sub_101151A0();
            IsConversationOngoing = true;
        }
    }

    [TempleDllLocation(0x1014bb20)]
    public void PlayVoiceLine(GameObject speaker, GameObject listener, int soundId)
    {
        GameSystems.Dialog.PlayVoiceLine(speaker, listener, soundId);
    }

    [TempleDllLocation(0x1014cac0)]
    public void ToggleHistory()
    {
        if (IsConversationOngoing)
        {
            ShowDialogHistory = !ShowDialogHistory;
            UpdateLayout();
            return;
        }

        if (!ShowDialogHistory)
        {
            UiDialogBegin();
            Globals.UiManager.RemoveWindow(uiDialogWnd2Id);
            ShowDialogHistory = true;
            UpdateLayout();
            return;
        }

        Hide();
        UpdateLayout();
    }

    [TempleDllLocation(0x10bea918)]
    private DialogState dialog_slot_idx;

    [TempleDllLocation(0x1014ced0)]
    public void InitiateDialog(GameObject pc, GameObject npc, int scriptId, int i, int lineNumber)
    {
        if (!GameSystems.Critter.IsDeadNullDestroyed(pc)
            && !GameSystems.Critter.IsDeadOrUnconscious(pc)
            && GameSystems.AI.GetCannotTalkReason(npc, pc) == 0
            && (!GameSystems.Party.IsInParty(pc) || !IsVisible))
        {
            GameSystems.Anim.InterruptAllExceptFidgetOrIdle();
            if (GameSystems.Combat.IsCombatModeActive(pc))
            {
                GameSystems.Combat.CritterLeaveCombat(pc);
            }

            if (scriptId == 0 || !GameSystems.Dialog.TryLoadDialog(scriptId, out var dialogScript))
            {
                SayDefaultResponse(pc, npc);
            }
            else
            {
                dialog_slot_idx = new DialogState(npc, pc);
                dialog_slot_idx.DialogScript = dialogScript;
                dialog_slot_idx.ReqNpcLineId = lineNumber;
                dialog_slot_idx.DialogScriptId = scriptId;
                if (!GameSystems.Dialog.BeginDialog(dialog_slot_idx))
                {
                    GameSystems.Dialog.Free(ref dialog_slot_idx.DialogScript);
                    return;
                }

                if (dialog_slot_idx.ActionType == 3)
                {
                    DialogCreateBubble(
                        dialog_slot_idx.NPC,
                        dialog_slot_idx.Pc,
                        0,
                        -1,
                        dialog_slot_idx.NPCLineText,
                        dialog_slot_idx.SpeechId);
                    GameSystems.Dialog.EndDialog(dialog_slot_idx);
                    GameSystems.Dialog.Free(ref dialog_slot_idx.DialogScript);
                    return;
                }

                if (GameSystems.Party.IsInParty(pc))
                {
                    sub_101151A0();

                    IsConversationOngoing = true;
                    UiSystems.InGame.ResetInput();

                    // TODO: ToEE previously called the "onExamine" SAN here, but no scripts use it
                    GameSystems.MapObject.GlobalStashedObject = npc;
                }

                TurnTowards(pc, npc);

                if (GameSystems.Critter.IsMovingSilently(pc))
                {
                    GameSystems.Critter.SetMovingSilently(pc, false);
                }

                if (GameSystems.Critter.IsMovingSilently(npc))
                {
                    GameSystems.Critter.SetMovingSilently(npc, false);
                }

                dialog_slot_idx.Unk = i;
                UiDialogGetResponsesFromDialogState(dialog_slot_idx);
            }
        }
    }

    [TempleDllLocation(0x1014ce30)]
    public void UiDialogGetResponsesFromDialogState(DialogState state)
    {
        DialogCreateBubble(
            state.NPC,
            state.Pc,
            0,
            -2,
            state.NPCLineText,
            state.SpeechId);

        if (GameSystems.Party.IsInParty(state.Pc))
        {
            var lines = new ResponseLine[state.PcLineText.Length];

            for (var i = 0; i < state.PcLineText.Length; i++)
            {
                lines[i] = new ResponseLine(state.PcLineText[i], state.PcLineSkillUse[i]);
            }

            _responseList.SetResponses(lines);
        }
    }

    [TempleDllLocation(0x10113db0)]
    [TempleDllLocation(0x1014bb80)]
    public void UiDialogResetResponses()
    {
        _responseList.ClearResponses();
    }

    [TempleDllLocation(0x101151a0)]
    public void sub_101151A0()
    {
        if (UiSystems.CharSheet.HasCurrentCritter)
        {
            UiSystems.CharSheet.CurrentPage = 0;
            UiSystems.CharSheet.Hide(0);
        }

        if (UiSystems.Logbook.IsVisible)
        {
            UiSystems.Logbook.Hide();
        }

        if (UiSystems.WorldMap.IsVisible)
        {
            UiSystems.WorldMap.Hide();
        }

        UiSystems.Tooltip.TooltipsEnabled = false;
        UiDialogBegin();
    }

    [TempleDllLocation(0x10bec344)]
    private bool dword_10BEC344;

    // This is the previous music volume before going into the dialog...
    [TempleDllLocation(0x10bea8a4)]
    private int uiDialogMusicVolume;

    [TempleDllLocation(0x10bea5f4)]
    private int uiDialogFlags = 1;

    // TODO: This is "history is being shown"
    public bool ShowDialogHistory
    {
        get => (uiDialogFlags & 2) != 0;
        set
        {
            if (value)
            {
                uiDialogFlags |= 2;
            }
            else
            {
                uiDialogFlags &= ~2;
            }
        }
    }

    /// <summary>
    /// This includes both NPC and PC lines. The last line in this list is the current NPC line being shown.
    /// </summary>
    [TempleDllLocation(0x10bec1a4)]
    private readonly List<DisplayedDialogLine> _lineHistory = new();

    [TempleDllLocation(0x1014c8f0)]
    public void UiDialogBegin()
    {
        if (dialog_slot_idx?.NPC != null)
        {
            var script = dialog_slot_idx.NPC.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);
            if (script.scriptId > 0)
            {
                var soundDir = $"data/sound/speech/{script.scriptId:D5}";
                if (Tig.FS.DirectoryExists(soundDir))
                {
                    dword_10BEC344 = true;
                    uiDialogMusicVolume = GameSystems.SoundGame.MusicVolume;
                    if (uiDialogMusicVolume > 24)
                    {
                        Tig.Sound.SetVolume(tig_sound_type.TIG_ST_MUSIC, 19);
                    }
                }
            }
        }

        ShowDialogHistory = false;
        uiDialogFlags = 0;
        UiDialogWidgetsShow();
    }

    [TempleDllLocation(0x1014c340)]
    public void UiDialogWidgetsShow()
    {
        UiSystems.InGame.ResetInput();
        Globals.UiManager.AddWindow(uiDialogWnd2Id);
        uiDialogWnd2Id.BringToFront();

        Globals.UiManager.AddWindow(_mainWindow);
        _mainWindow.BringToFront();

        UpdateLayout();
    }

    private void TurnTowards(GameObject pc, GameObject npc)
    {
        GameSystems.Anim.Interrupt(pc, AnimGoalPriority.AGP_3);
        GameSystems.Anim.PushRotate(pc, pc.RotationTo(npc));
    }

    [TempleDllLocation(0x1014cd70)]
    public void SayDefaultResponse(GameObject pc, GameObject npc)
    {
        GameSystems.Reaction.DialogReaction_10053FE0(npc, pc);
        var text = GameSystems.Dialog.Dialog_10037AF0(npc, pc);
        if (text != null)
        {
            DialogCreateBubble(npc, pc, 0, -1, text, -1);
        }

        GameSystems.Reaction.NpcReactionUpdate(npc, pc);
    }

    [TempleDllLocation(0x1014c840)]
    public void DialogCreateBubble(GameObject actor, GameObject tgt, int a3, int textDuration, string text,
        int soundId)
    {
        if (tgt != null
            && !GameSystems.Critter.IsDeadNullDestroyed(actor)
            && !GameSystems.Anim.GetFirstRunSlotId(actor).IsNull)
        {
            TurnTowards(actor, tgt);
        }

        GameSystems.TextBubble.Remove(actor);
        GameSystems.TextBubble.FloatText_100A3420(actor, a3, text);
        GameSystems.TextBubble.SetDuration(actor, textDuration);
        GameSystems.Dialog.PlayVoiceLine(actor, tgt, soundId);
        DialogLineAppend(actor, text);
    }

    [TempleDllLocation(0x1014c720)]
    public void DialogLineAppend(GameObject speaker, string text)
    {
        var speakerName = GameSystems.MapObject.GetDisplayNameForParty(speaker);

        var dlgListEntry = new DisplayedDialogLine();
        dlgListEntry.IsPcLine = speaker.IsPC();

        dlgListEntry.Text = $"({speakerName})  {text}";
        dlgListEntry.Paragraph = FlowContentParser.ParseParagraph(dlgListEntry.Text);
        _lineHistory.Add(dlgListEntry);

        UiDialogScrollbarRefresh /*0x1014bdf0*/();

        if ((uiDialogFlags & 1) == 0)
        {
            UpdateLayout();
        }
    }

    /// <summary>
    /// Measure how high the current NPC line is in pixels.
    /// </summary>
    private int MeasureCurrentLineHeight()
    {
        if (_lineHistory.Count == 0)
        {
            return 0;
        }

        var lastLine = _lineHistory[^1];

        using var layout = _textEngine.CreateTextLayout(lastLine.Paragraph, ContentWidth, 1000);
        return (int) Math.Ceiling(layout.OverallHeight);
    }

    [TempleDllLocation(0x1014c030)]
    private void UpdateLayout()
    {
        _backdrop1Line.Visible = false;
        _backdrop2Line.Visible = false;
        _backdrop3Line.Visible = false;
        _backdropHistory.Visible = false;
        _splitter.Visible = false;

        if (IsConversationOngoing)
        {
            if (ShowDialogHistory)
            {
                dword_10BEC20C = 26;
                _backdropHistory.Visible = true;
                _splitter.Visible = true;
                _historyScollbar.Height = Dimension.Pixels(126);
                _historyScollbar.Visible = true;
                _responseContainer.Visible = true;
                UiSystems.UtilityBar.HistoryUi.HideDialogButton();
                _showHistoryButton.Y = 1;
            }
            else
            {
                _historyScollbar.Visible = false;
                _responseContainer.Visible = true;
                UiSystems.UtilityBar.HistoryUi.HideDialogButton();

                var measuredHeight = MeasureCurrentLineHeight();
                if (Globals.Config.Co8)
                {
                    dword_10BEC20C = -90;
                    _backdrop1Line.Visible = true;
                    _showHistoryButton.Y = 59;
                    _dialogLinesContainer.Y = 77;
                    _dialogLinesContainer.Height = Dimension.Pixels(15);
                }
                else if (measuredHeight > 26)
                {
                    dword_10BEC20C = -58;
                    _backdrop3Line.Visible = true;
                    _showHistoryButton.Y = 77;
                    _dialogLinesContainer.Y = 94 + 10;
                    _dialogLinesContainer.Height = Dimension.Pixels(45);
                }
                else if (measuredHeight > 13)
                {
                    dword_10BEC20C = -77;
                    _backdrop2Line.Visible = true;
                    _showHistoryButton.Y = 96;
                    _dialogLinesContainer.Y = 112 + 10;
                    _dialogLinesContainer.Height = Dimension.Pixels(30);
                }
                else
                {
                    dword_10BEC20C = -90;
                    _backdrop1Line.Visible = true;
                    _showHistoryButton.Y = 109;
                    _dialogLinesContainer.Y = 126 + 10;
                    _dialogLinesContainer.Height = Dimension.Pixels(15);
                }
            }
        }
        else if (ShowDialogHistory)
        {
            _backdropHistory.Visible = true;
            dword_10BEC20C = 26;
            _historyScollbar.Height = Dimension.Pixels(261);
            _historyScollbar.Visible = true;
            _showHistoryButton.Y = 1;
            _responseContainer.Visible = false;
            UiSystems.UtilityBar.HistoryUi.HideDialogButton();
        }
        else
        {
            UiSystems.UtilityBar.HistoryUi.UpdateWidgetVisibility();
        }
    }

    [TempleDllLocation(0x1014bdf0)]
    private void UiDialogScrollbarRefresh()
    {
        var uiDialogScrollbarYMax = _lineHistory.Count - 1;
        _historyScollbar.Max = uiDialogScrollbarYMax;
        _historyScollbar.SetValue(uiDialogScrollbarYMax);
    }

    [TempleDllLocation(0x1014ccf0)]
    public void Reset()
    {
        uiDialogMusicVolume = GameSystems.SoundGame.MusicVolume;
        if (IsConversationOngoing)
        {
            UiDialogHide_10115210();
            IsConversationOngoing = false;
        }

        if (dialog_slot_idx != null)
        {
            GameSystems.Dialog.Free(ref dialog_slot_idx.DialogScript);
            GameSystems.Dialog.EndDialog(dialog_slot_idx);
            dialog_slot_idx = null;
        }

        ClearLineHistory();
    }

    [TempleDllLocation(0x1014cba0)]
    private void ClearLineHistory()
    {
        _lineHistory.Clear();
    }

    [TempleDllLocation(0x1014c830)]
    public void SaveGame(SavedUiState savedUiState)
    {
        savedUiState.DialogState = new SavedDialogUiState
        {
            Lines = _lineHistory.Select(line => new SavedDialogUiLine
            {
                Text = line.Text,
                IsPcLine = line.IsPcLine
            }).ToList()
        };
    }

    [TempleDllLocation(0x1014cd50)]
    public void LoadGame(SavedUiState savedUiState)
    {
        ClearLineHistory();

        foreach (var savedLine in savedUiState.DialogState.Lines)
        {
            _lineHistory.Add(new DisplayedDialogLine
            {
                Text = savedLine.Text,
                Paragraph = FlowContentParser.ParseParagraph(savedLine.Text),
                IsPcLine = savedLine.IsPcLine
            });
        }
    }
}

public class DisplayedDialogLine
{
    public string Text { get; set; }

    public Paragraph Paragraph { get; set; }

    public bool IsPcLine { get; set; }
}

internal readonly struct ResponseLine
{
    public readonly string Text;

    public readonly DialogSkill SkillUsed;

    public ResponseLine(string text, DialogSkill skillUsed)
    {
        Text = text;
        SkillUsed = skillUsed;
    }
}