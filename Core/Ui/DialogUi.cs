using System;
using System.Collections.Generic;
using System.Drawing;
using SpicyTemple.Core.GameObject;
using SpicyTemple.Core.GFX;
using SpicyTemple.Core.Systems;
using SpicyTemple.Core.Systems.Anim;
using SpicyTemple.Core.Systems.Dialog;
using SpicyTemple.Core.Systems.ObjScript;
using SpicyTemple.Core.TigSubsystems;
using SpicyTemple.Core.Ui.UtilityBar;
using SpicyTemple.Core.Ui.WidgetDocs;
using SpicyTemple.Core.Utils;

namespace SpicyTemple.Core.Ui
{
    public class DialogUi
    {
        private const PredefinedFont Font = PredefinedFont.PRIORY_12;

        public const int MaxDialogLines = 200;

        [TempleDllLocation(0x10bec280)]
        private static readonly TigTextStyle uiDialogTextStyle2 =
            new TigTextStyle(new ColorRect(new PackedLinearColorA(0xFFFFFF00)))
            {
                kerning = 1,
                tracking = 3
            };

        [TempleDllLocation(0x10bea290)]
        private static readonly TigTextStyle uiDlgTextStyle =
            new TigTextStyle(new ColorRect(new PackedLinearColorA(0xFF666666)))
            {
                kerning = 1,
                tracking = 3
            };

        [TempleDllLocation(0x1014bb50)]
        public bool IsActive => (uiDialogFlags & 1) == 0 || uiDialogWndId.IsVisible();

        [TempleDllLocation(0x1014bac0)]
        [TempleDllLocation(0x10BEC348)]
        public bool IsActive2 { get; set; }

        [TempleDllLocation(0x10bea2e4)]
        private WidgetContainer uiDialogWndId;

        [TempleDllLocation(0x10bec19c)]
        private WidgetScrollBar uiDialogScrollbarId;

        [TempleDllLocation(0x10bec210)]
        private WidgetButton uiDialogButton1Id;

        [TempleDllLocation(0x10bec198)]
        private WidgetContainer uiDialogWnd2Id;

        public DialogUi()
        {
            Stub.TODO();

            GameSystems.AI.SetDialogFunctions(CancelDialog, ShowTextBubble);

            CreateWidgets();
        }

        [TempleDllLocation(0x10bec204)]
        private WidgetContainer uiDialogResponseWndId;

        [TempleDllLocation(0x10BEA8A8)]
        private WidgetButton[] uiDialogResponseBtnIds = new WidgetButton[5];

        [TempleDllLocation(0x1014d9b0)]
        [TempleDllLocation(0x1014d5d0)]
        [TempleDllLocation(0x1014c50c)]
        private void CreateWidgets()
        {
            // Begin top level window
            // Created @ 0x1014dacc
            uiDialogWndId = new WidgetContainer(new Rectangle(9, 394, 611, 292));
            // uiDialogWndId.OnHandleMessage += 0x1014bd00;
            // uiDialogWndId.OnBeforeRender += 0x1014bbb0;
            uiDialogWndId.SetVisible(false);

            uiDialogScrollbarId = new WidgetScrollBar(new Rectangle(592, 28, 13, 126));
            // uiDialogScrollbarId.OnHandleMessage += 0x101fa410;
            // uiDialogScrollbarId.OnBeforeRender += 0x101fa1b0;
            uiDialogWndId.Add(uiDialogScrollbarId);

            uiDialogButton1Id = new WidgetButton(new Rectangle(581, 1, 23, 18));
            // uiDialogButton1Id.OnHandleMessage += 0x1014cb20;
            // uiDialogButton1Id.OnBeforeRender += 0x1014be30;
            // uiDialogButton1Id.OnRenderTooltip += 0x100027f0;
            uiDialogWndId.Add(uiDialogButton1Id);

            // Begin top level window
            var uiDialogResponseWndId = new WidgetContainer(new Rectangle(16, 547, 611, 139));
            // uiDialogResponseWndId.OnHandleMessage += 0x1014d510;
            // uiDialogResponseWndId.OnBeforeRender += 0x1014bf00;
            uiDialogResponseWndId.SetVisible(false);

            for (var i = 0; i < 5; i++)
            {
                var responseButton = new WidgetButton(new Rectangle(1, 9 + i * 25, 594, 23));
                // responseButton.OnHandleMessage += 0x1014d560;
                // responseButton.OnBeforeRender += 0x1014c520;
                uiDialogResponseWndId.Add(responseButton);
                uiDialogResponseBtnIds[i] = responseButton;
            }

            uiDialogWnd2Id = new WidgetContainer(new Rectangle(0, 0, 1024, 768));
            // uiDialogWnd2Id.OnHandleMessage += 0x1014bdc0;
            // uiDialogWnd2Id.OnBeforeRender += 0x100027f0;
            uiDialogWnd2Id.SetVisible(false);
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

        [TempleDllLocation(0x1014cac0)]
        public void ToggleHistory()
        {
            if (UiSystems.Dialog.IsActive2)
            {
                DialogFlag2 = !DialogFlag2;
                sub_1014C030 /*0x1014c030*/();
                return;
            }

            if (!DialogFlag2)
            {
                UiDialogBegin();
                uiDialogWnd2Id.SetVisible(false);
                DialogFlag2 = true;
                sub_1014C030();
                return;
            }

            UiSystems.Dialog.Hide();
            sub_1014C030();
        }

        [TempleDllLocation(0x10bea918)]
        private DialogState dialog_slot_idx;

        [TempleDllLocation(0x1014ced0)]
        public void InitiateDialog(GameObjectBody pc, GameObjectBody npc, int scriptId, int i, int lineNumber)
        {
            float rotation;

            if (!GameSystems.Critter.IsDeadNullDestroyed(pc)
                && !GameSystems.Critter.IsDeadOrUnconscious(pc)
                && GameSystems.AI.GetCannotTalkReason(npc, pc) == 0
                && (!GameSystems.Party.IsInParty(pc) || UiSystems.InGame.sub_10113CD0() != 3))
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
                    dialog_slot_idx.dialogScript = dialogScript;
                    dialog_slot_idx.npc = npc;
                    dialog_slot_idx.pc = pc;
                    dialog_slot_idx.reqNpcLineId = lineNumber;
                    dialog_slot_idx.dialogScriptId = scriptId;
                    if (!GameSystems.Dialog.BeginDialog(dialog_slot_idx))
                    {
                        GameSystems.Dialog.Free(ref dialog_slot_idx.dialogScript);
                        return;
                    }

                    if (dialog_slot_idx.actionType == 3)
                    {
                        DialogCreateBubble(
                            dialog_slot_idx.npc,
                            dialog_slot_idx.pc,
                            0,
                            -1,
                            dialog_slot_idx.npcLineText,
                            dialog_slot_idx.speechId);
                        GameSystems.Dialog.EndDialog(dialog_slot_idx);
                        GameSystems.Dialog.Free(ref dialog_slot_idx.dialogScript);
                        return;
                    }

                    if (GameSystems.Party.IsInParty(pc))
                    {
                        sub_101151A0();

                        UiSystems.Dialog.IsActive2 = true;
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

                    // TODO dialog_slot_idx.scriptId = scriptId;
                    // TODO dialog_slot_idx.dialogEngaged = 1;
                    dialog_slot_idx.unk = i;
                    UiDialogGetResponsesFromDialogState(dialog_slot_idx);
                }
            }
        }

        [TempleDllLocation(0x10bec228)] [TempleDllLocation(0x10be9b38)] [TempleDllLocation(0x10bec214)]
        private List<ResponseLine> uiDialogResponses = new List<ResponseLine>();

        private readonly struct ResponseLine
        {
            public readonly string Text;

            public readonly int SkillUsed;

            public ResponseLine(string text, int skillUsed)
            {
                Text = text;
                SkillUsed = skillUsed;
            }
        }

        [TempleDllLocation(0x1014ce30)]
        public void UiDialogGetResponsesFromDialogState(DialogState state)
        {
            DialogCreateBubble(
                state.npc,
                state.pc,
                0,
                -2,
                state.npcLineText,
                state.speechId);

            if (GameSystems.Party.IsInParty(state.pc))
            {
                UiDialogResetResponses();

                for (var i = 0; i < state.pcLineText.Length; i++)
                {
                    uiDialogResponses.Add(new ResponseLine(state.pcLineText[i], state.pcLineSkillUse[i]));
                }
            }
        }

        [TempleDllLocation(0x10113db0)]
        [TempleDllLocation(0x1014bb80)]
        public void UiDialogResetResponses()
        {
            uiDialogResponses.Clear();
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
            UiSystems.InGame.SetScene(0);
            UiSystems.InGame.SetScene(3);
        }

        [TempleDllLocation(0x10bec344)]
        private bool dword_10BEC344;

        // This is the previous music volume before going into the dialog...
        [TempleDllLocation(0x10bea8a4)]
        private int uiDialogMusicVolume;

        [TempleDllLocation(0x10bea5f4)]
        private int uiDialogFlags = 1;

        public bool DialogFlag2
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

        [TempleDllLocation(0x10bec1a4)]
        private List<DisplayedDialogLine> dlgLineList = new List<DisplayedDialogLine>();

        [TempleDllLocation(0x1014c8f0)]
        public void UiDialogBegin()
        {
            if (dialog_slot_idx.npc != null)
            {
                var script = dialog_slot_idx.npc.GetScript(obj_f.scripts_idx, (int) ObjScriptEvent.Dialog);
                if (script.scriptId > 0)
                {
                    var soundDir = $"data/sound/speech/{script.scriptId:D5}";
                    if (Tig.FS.DirectoryExists(soundDir))
                    {
                        dword_10BEC344 = true;
                        uiDialogMusicVolume = GameSystems.SoundGame.MusicVolume;
                        if (uiDialogMusicVolume > 24)
                        {
                            Tig.Sound.SetVolume(1, 19);
                        }
                    }
                }
            }

            uiDialogFlags = 0;
            UiDialogWidgetsShow();
        }

        [TempleDllLocation(0x1014c340)]
        public void UiDialogWidgetsShow()
        {
            UiSystems.InGame.ResetInput();
            uiDialogWnd2Id.SetVisible(true);
            uiDialogWnd2Id.BringToFront();

            uiDialogWndId.SetVisible(true);
            uiDialogWndId.BringToFront();
            if ((uiDialogFlags & 2) != 0)
            {
                uiDialogScrollbarId.SetVisible(true);
            }
            else
            {
                uiDialogScrollbarId.SetVisible(false);
            }

            if ((uiDialogFlags & 2) == 0)
            {
                Tig.Fonts.PushFont(Font);
                if (dlgLineList.Count > 0)
                {
                    var metrics = Tig.Fonts.MeasureTextSize(dlgLineList[0].Text, uiDialogTextStyle2, 550);

                    if (metrics.Height <= 12)
                    {
                        uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 109);
                    }
                    else if (metrics.Height <= 24)
                    {
                        uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 96);
                    }
                    else
                    {
                        uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 77);
                    }
                }
                else
                {
                    uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 109);
                }

                Tig.Fonts.PopFont();
            }
            else
            {
                uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 1);
            }

            uiDialogResponseWndId.SetVisible(true);
            uiDialogResponseWndId.BringToFront();
            UiSystems.UtilityBar.HistoryUi.HideDialogButton();
        }

        private void TurnTowards(GameObjectBody pc, GameObjectBody npc)
        {
            GameSystems.Anim.Interrupt(pc, AnimGoalPriority.AGP_3);
            GameSystems.Anim.PushRotate(pc, pc.RotationTo(npc));
        }

        [TempleDllLocation(0x1014cd70)]
        public void SayDefaultResponse(GameObjectBody pc, GameObjectBody npc)
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
        public void DialogCreateBubble(GameObjectBody actor, GameObjectBody tgt, int a3, int textDuration, string text,
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
        public void DialogLineAppend(GameObjectBody speaker, string text)
        {
            var speakerName = GameSystems.MapObject.GetDisplayNameForParty(speaker);

            var dlgListEntry = new DisplayedDialogLine();
            dlgListEntry.IsPcLine = speaker.IsPC();

            dlgListEntry.Text = $"({speakerName})  {text}";
            dlgLineList.Insert(0, dlgListEntry);

            UiDialogScrollbarRefresh /*0x1014bdf0*/();

            if ((uiDialogFlags & 1) == 0)
            {
                sub_1014C030();
            }
        }

        [TempleDllLocation(0x10bec20c)]
        private int dword_10BEC20C;

        [TempleDllLocation(0x10bea2e0)]
        private int dword_10BEA2E0;

        [TempleDllLocation(0x10be9ff0)]
        private int dword_10BE9FF0;

        [TempleDllLocation(0x1014c030)]
        private void sub_1014C030()
        {
            if (UiSystems.Dialog.IsActive2)
            {
                if (DialogFlag2)
                {
                    dword_10BEC20C = 26;
                    uiDialogScrollbarId.SetHeight(126);
                    uiDialogScrollbarId.SetVisible(true);
                    uiDialogResponseWndId.SetVisible(true);
                    uiDialogResponseWndId.BringToFront(); // TODO: Fishy!
                    UiSystems.UtilityBar.HistoryUi.HideDialogButton();
                    uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 1);
                }
                else
                {
                    uiDialogScrollbarId.SetVisible(false);
                    uiDialogResponseWndId.SetVisible(true);
                    uiDialogResponseWndId.BringToFront(); // TODO: Fishy!
                    UiSystems.UtilityBar.HistoryUi.HideDialogButton();
                    int buttonY;
                    if (dlgLineList.Count > 0)
                    {
                        Tig.Fonts.PushFont(Font);
                        var metrics = Tig.Fonts.MeasureTextSize(dlgLineList[0].Text, uiDialogTextStyle2, 550);
                        Tig.Fonts.PopFont();
                        dword_10BEA2E0 /*0x10bea2e0*/ = uiDialogWndId /*0x10bea2e8*/.GetX();
                        buttonY = uiDialogWndId.GetY() + 59;
                        dword_10BE9FF0 /*0x10be9ff0*/ = uiDialogWndId.GetY() + 77;
                        // TODO v1 = (ImgFile*) uiDialogBackdropMini1 /*0x10bec1a8*/;
                        dword_10BEC20C = -90;
                    }
                    else
                    {
                        buttonY = uiDialogWndId.GetY() + 109;
                        dword_10BE9FF0 /*0x10be9ff0*/ = uiDialogWndId.GetY() + 126;
                        // TODO v1 = (ImgFile*) uiDialogBackdropMini1 /*0x10bec1a8*/;
                        dword_10BEC20C = -90;
                        dword_10BEA2E0 /*0x10bea2e0*/ = uiDialogWndId.GetX();
                    }

                    uiDialogButton1Id.SetY(buttonY);
                    // TODO uiDialogBackdropMaximized /*0x10be9b4c*/ = v1;
                }
            }
            else if (DialogFlag2)
            {
                dword_10BEC20C = 26;
                uiDialogScrollbarId.SetHeight(261);
                uiDialogScrollbarId.SetVisible(true);
                uiDialogButton1Id.SetY(uiDialogWndId.GetY() + 1);
                uiDialogResponseWndId.SetVisible(false);
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
            var uiDialogScrollbarYMax = dlgLineList.Count - 1;
            uiDialogScrollbarId.SetMax(uiDialogScrollbarYMax);
            uiDialogScrollbarId.SetValue(uiDialogScrollbarYMax);
        }
    }

    public class DisplayedDialogLine
    {
        public int Flags { get; set; }

        public string Text { get; set; }

        public bool IsPcLine { get; set; }
    }
}