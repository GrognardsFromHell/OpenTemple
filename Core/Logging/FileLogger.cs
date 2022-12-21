using System;
using System.Globalization;
using System.IO;
using System.Text;
using OpenTemple.Core.Time;

namespace OpenTemple.Core.Logging;

public sealed class FileLogger : LoggerBase
{
    private readonly StreamWriter _writer;

    private readonly TimePoint _startupPoint;

    public FileLogger(string path)
    {
        _startupPoint = TimePoint.Now;
        _writer = new StreamWriter(path, false, Encoding.UTF8);

        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        _writer.Close();
    }

    private void WriteElapsedTime()
    {
        var elapsedSeconds = (TimePoint.Now - _startupPoint).TotalSeconds;
        _writer.Write(string.Format(CultureInfo.InvariantCulture, "{0:0000.000} ", elapsedSeconds));
    }

    public override void Error(ReadOnlySpan<char> message)
    {
        WriteElapsedTime();
        _writer.Write("[e] ");
        _writer.WriteLine(message);
        _writer.Flush();
    }

    public override void Warn(ReadOnlySpan<char> message)
    {
        WriteElapsedTime();
        _writer.Write("[w] ");
        _writer.WriteLine(message);
        _writer.Flush();
    }

    public override void Info(ReadOnlySpan<char> message)
    {
        WriteElapsedTime();
        _writer.Write("[i] ");
        _writer.WriteLine(message);
        _writer.Flush();
    }

    public override void Debug(ReadOnlySpan<char> message)
    {
        WriteElapsedTime();
        _writer.Write("[d] ");
        _writer.WriteLine(message);
        _writer.Flush();
    }
}