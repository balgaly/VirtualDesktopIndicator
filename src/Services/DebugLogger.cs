using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VirtualDesktopIndicator.Services
{
    /// <summary>
    /// SECURITY: Eliminates debug output in release builds to prevent information disclosure
    /// </summary>
    internal static class DebugLogger
    {
        [Conditional("DEBUG")]
        public static void WriteLine(
            string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            var fileName = System.IO.Path.GetFileName(sourceFilePath);
            Debug.WriteLine($"[{fileName}:{sourceLineNumber}] {memberName}: {message}");
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(string.Format(format, args));
        }

        [Conditional("DEBUG")]
        public static void WriteSecurityInfo(string message)
        {
            Debug.WriteLine($"[SECURITY] {message}");
        }
    }
}