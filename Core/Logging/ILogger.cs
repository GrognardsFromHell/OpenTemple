using System;
using JetBrains.Annotations;

namespace OpenTemple.Core.Logging
{
    public interface ILogger
    {
        void Error(ReadOnlySpan<char> message);

        [StringFormatMethod("format")]
        void Error<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Error<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        void Warn(ReadOnlySpan<char> message);

        [StringFormatMethod("format")]
        void Warn<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Warn<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Warn<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Warn(string format, params object[] args);

        void Info(ReadOnlySpan<char> message);

        [StringFormatMethod("format")]
        void Info<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Info<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Info(string format, params object[] args);

        void Debug(ReadOnlySpan<char> message);

        [StringFormatMethod("format")]
        void Debug<T1>(string format, T1 arg1);

        [StringFormatMethod("format")]
        void Debug<T1, T2>(string format, T1 arg1, T2 arg2);

        [StringFormatMethod("format")]
        void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3);

        [StringFormatMethod("format")]
        void Debug(string format, params object[] args);
    }
}