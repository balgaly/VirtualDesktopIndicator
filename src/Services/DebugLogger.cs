using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace VirtualDesktopIndicator.Services
{
    /// <summary>
    /// Secure logging wrapper that eliminates debug output in release builds
    /// to prevent information disclosure vulnerabilities
    /// </summary>
    internal static class DebugLogger
    {
        /// <summary>
        /// Writes debug information only in DEBUG builds. 
        /// In RELEASE builds, this method does nothing to prevent information leakage.
        /// </summary>
        /// <param name="message">The debug message</param>
        /// <param name="memberName">Auto-populated calling member name</param>
        /// <param name="sourceFilePath">Auto-populated source file path</param>
        /// <param name="sourceLineNumber">Auto-populated source line number</param>
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

        /// <summary>
        /// Writes debug information with formatting only in DEBUG builds
        /// </summary>
        /// <param name="format">Format string</param>
        /// <param name="args">Arguments for formatting</param>
        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            Debug.WriteLine(string.Format(format, args));
        }

        /// <summary>
        /// Writes security-related debug information with special handling
        /// </summary>
        /// <param name="message">Security-related message</param>
        [Conditional("DEBUG")]
        public static void WriteSecurityInfo(string message)
        {
            Debug.WriteLine($"[SECURITY] {message}");
        }
    }
}