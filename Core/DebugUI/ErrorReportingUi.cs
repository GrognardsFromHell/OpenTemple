using System;
using System.Numerics;
using ImGuiNET;
using OpenTemple.Core.Platform;
using OpenTemple.Core.TigSubsystems;
using OpenTemple.Core.Utils;
using OpenTemple.Interop;

namespace OpenTemple.Core.DebugUI
{
    public static class ErrorReportingUi
    {
        private static int _currentErrorIndex = 0;

        public static void Render()
        {
            // If the queue is empty, we have nothing to report
            if (ErrorReporting.Queue.Count == 0)
            {
                return;
            }

            // Clamp _currentErrorIndex
            _currentErrorIndex = Math.Clamp(_currentErrorIndex, 0, ErrorReporting.Queue.Count - 1);

            var screenSize = ImGui.GetIO().DisplaySize;
            var dialogSize = new Vector2(800, 600);
            ImGui.SetNextWindowSize(dialogSize, ImGuiCond.Appearing);
            ImGui.SetNextWindowPos((screenSize - dialogSize) * 0.5f, ImGuiCond.Appearing);
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0.4f, 0.2f, 0.2f, 1.0f));
            if (!ImGui.Begin("Unhandled Errors"))
            {
                ImGui.PopStyleColor();
                return;
            }

            ImGui.PopStyleColor();

            ImGui.SetWindowFontScale(1.5f);
            ImGui.Text("Unhandled Exception");

            ImGui.SetWindowFontScale(1.25f);
            var currentErrorText = ErrorReporting.Queue[_currentErrorIndex].Error.ToString();
            ImGui.TextWrapped(currentErrorText);
            ImGui.SetWindowFontScale(1.0f);

            ImGui.Text($"Error {_currentErrorIndex + 1} of {ErrorReporting.Queue.Count}");

            if (ImGui.Button("Dismiss"))
            {
                ErrorReporting.Queue.RemoveAt(_currentErrorIndex);
                if (_currentErrorIndex >= ErrorReporting.Queue.Count)
                {
                    _currentErrorIndex = ErrorReporting.Queue.Count - 1;
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Dismiss All"))
            {
                ErrorReporting.Queue.Clear();
            }

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();

            // Navigate to the previous error in the queue
            if (_currentErrorIndex > 0)
            {
                if (ImGui.ArrowButton("Prev", ImGuiDir.Left))
                {
                    _currentErrorIndex--;
                }
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                ImGui.ArrowButton("Prev", ImGuiDir.Left);
                ImGui.PopStyleVar();
            }

            ImGui.SameLine();
            if (_currentErrorIndex + 1 < ErrorReporting.Queue.Count)
            {
                if (ImGui.ArrowButton("Next", ImGuiDir.Right))
                {
                    _currentErrorIndex++;
                }
            }
            else
            {
                ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                ImGui.ArrowButton("Next", ImGuiDir.Right);
                ImGui.PopStyleVar();
            }

            ImGui.SameLine();
            ImGui.Spacing();
            ImGui.SameLine();
            if (ImGui.Button("Copy to Clipboard"))
            {
                if (Tig.MainWindow is MainWindow nativeMainWindow)
                {
                    NativePlatform.SetClipboardText(nativeMainWindow.NativeHandle, currentErrorText);
                }
            }

            ImGui.End(); // Window
        }
    }
}