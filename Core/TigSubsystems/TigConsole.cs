using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using ImGuiNET;
using SpicyTemple.Core.IO;
using SpicyTemple.Core.Scripting;

namespace SpicyTemple.Core.TigSubsystems
{
    public class TigConsole
    {
        public bool IsVisible { get; set; }

        public List<string> AvailableScripts
        {
            get
            {
                var files = Tig.FS.Search("scripts/*.csx");
                return files.Select(f =>
                {
                    var scriptName = Path.GetFileName(Path.ChangeExtension(f, null));
                    return scriptName;
                }).ToList();
            }
        }

        public void RunScript(string name)
        {
            var path = "scripts/" + name + ".csx";
            _devScripting.RunScriptAsync(path);
        }

        private readonly IDevScripting _devScripting;

        private string _lastCompletionQuery = "";

        private List<string> _lastCompletionsResult = new List<string>();

        [TempleDllLocation(0x101df7c0)]
        public void ToggleVisible()
        {
            IsVisible = !IsVisible;
        }

        private readonly List<string> _log = new List<string>();

        private bool _scrollToBottom;

        private string _commandBuffer = "";

        private bool mIsInputActive;

        private bool mJustOpened;

        private int mCommandHistoryPos;

        private readonly List<string> mCommandHistory = new List<string>();

        // Cache the delegate because it'll be passed to native code
        private readonly ImGuiInputTextCallback _commandEditCallbackDelegate;

        public TigConsole(IDevScripting devScripting)
        {
            _devScripting = devScripting;
            unsafe
            {
                _commandEditCallbackDelegate = CommandEditCallback;
            }
        }

        public void Append(string text)
        {
            _log.Add(text.TrimEnd());
            ScrollToBottom();
        }

        public void ScrollToBottom()
        {
            _scrollToBottom = true;
        }

        public void Render()
        {
            if (!IsVisible)
            {
                return;
            }

            var consoleWidgeFlags = ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove
                                    | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoSavedSettings |
                                    ImGuiWindowFlags.MenuBar;

            var size = ImGui.GetIO().DisplaySize;
            var consPos = Vector2.Zero;

            var sceneRect = Tig.RenderingDevice.GetCamera().ScreenSize;
            size.X = sceneRect.Width;
            size.Y = sceneRect.Height;
            consPos.X = 0;
            consPos.Y = 0;
            size.Y = MathF.Max(300.0f, size.Y * 0.4f);
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(consPos);
            var open = IsVisible;
            if (!ImGui.Begin("Console", ref open, consoleWidgeFlags))
            {
                ImGui.End();
                return;
            }

            ImGui.BeginChild("ScrollingRegion", new Vector2(0, -ImGui.GetTextLineHeightWithSpacing()), false,
                ImGuiWindowFlags.HorizontalScrollbar);
            if (ImGui.BeginPopupContextWindow())
            {
                if (ImGui.Selectable("Clear"))
                    Clear();
                ImGui.EndPopup();
            }

            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 1)); // Tighten spacing

            foreach (var item in _log)
            {
                var col = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                if (item.Contains("[error]"))
                    col = new Vector4(1.0f, 0.4f, 0.4f, 1.0f);
                else if (item.StartsWith("# "))
                    col = new Vector4(1.0f, 0.78f, 0.58f, 1.0f);
                ImGui.PushStyleColor(ImGuiCol.Text, col);
                ImGui.TextUnformatted(item);
                ImGui.PopStyleColor();
            }

            if (_scrollToBottom)
                ImGui.SetScrollHereY();
            _scrollToBottom = false;
            ImGui.PopStyleVar();
            ImGui.EndChild();
            ImGui.Separator();


            ImGui.PushItemWidth(12);
            if (ImGui.Button("X"))
            {
                ImGui.End();
                Hide();
                return;
            }

            ImGui.SameLine();
            ImGui.PushItemWidth(-1);

            if (ImGui.InputText("Input", ref _commandBuffer, 1000,
                ImGuiInputTextFlags.EnterReturnsTrue | ImGuiInputTextFlags.CallbackCompletion |
                ImGuiInputTextFlags.CallbackHistory, _commandEditCallbackDelegate))
            {
                var command = _commandBuffer.Trim();
                Execute(command);

                _commandBuffer = "";

                // Refocus the control
                ImGui.SetKeyboardFocusHere(-1);
            }

            mIsInputActive = ImGui.IsItemActive();

            if (mJustOpened)
            {
                ImGui.SetKeyboardFocusHere(-1);
                mJustOpened = false;
            }

            ImGui.End();
        }

        private void Execute(string command, bool skipHistory = false)
        {
            if (!skipHistory)
            {
                // Remove it from the history first
                var it = mCommandHistory.Remove(command);

                // Append it at the end
                mCommandHistory.Add(command);

                // Reset cur pos within the list
                mCommandHistoryPos = -1;
            }

            var result = _devScripting.EvaluateExpression(command);
            if (result != null)
            {
                Append(result.ToString());
            }
        }

        private void Hide()
        {
            IsVisible = false;
        }

        private void Clear()
        {
            throw new NotImplementedException();
        }

        private unsafe int CommandEditCallback(ImGuiInputTextCallbackData* data)
        {
            string GetText()
            {
                var buffer = new Span<byte>(data->Buf, data->BufTextLen);
                return Encoding.UTF8.GetString(buffer);
            }

            void SetText(string text)
            {
                var buffer = new Span<byte>(data->Buf, data->BufSize);
                data->CursorPos = data->SelectionStart = data->SelectionEnd =
                    data->BufTextLen = Encoding.UTF8.GetBytes(text, buffer);
                data->BufDirty = 1;
            }

            if (data->EventFlag == ImGuiInputTextFlags.CallbackCompletion)
            {
                var completedText = _devScripting.Complete(GetText());
                SetText(completedText);
            }

            if (data->EventFlag == ImGuiInputTextFlags.CallbackHistory)
            {
                int prevHistoryPos = mCommandHistoryPos;
                if (data->EventKey == ImGuiKey.UpArrow)
                {
                    if (mCommandHistoryPos == -1)
                        mCommandHistoryPos = mCommandHistory.Count - 1;
                    else if (mCommandHistoryPos > 0)
                        mCommandHistoryPos--;
                }
                else if (data->EventKey == ImGuiKey.DownArrow)
                {
                    if (mCommandHistoryPos != -1)
                        if (++mCommandHistoryPos >= mCommandHistory.Count)
                            mCommandHistoryPos = -1;
                }

                // A better implementation would preserve the data on the current input line along with cursor position.
                if (prevHistoryPos != mCommandHistoryPos)
                {
                    var commandText = mCommandHistoryPos >= 0 ? mCommandHistory[mCommandHistoryPos] : "";
                    SetText(commandText);
                }
            }

            return 0;
        }
    }
}