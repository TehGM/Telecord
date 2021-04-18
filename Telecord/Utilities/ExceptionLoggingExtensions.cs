using System;
using Microsoft.Extensions.Logging;

namespace TehGM.Telecord
{
    public static class ExceptionLoggingExtensions
    {
        /// <summary>Logs exception with critical log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsCritical(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogCritical(exception, message, args);
            return true;
        }
        /// <summary>Logs exception with error log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsError(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogError(exception, message, args);
            return true;
        }
        /// <summary>Logs exception with warning log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsWarning(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogWarning(exception, message, args);
            return true;
        }
        /// <summary>Logs exception with information log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsInformation(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogInformation(exception, message, args);
            return true;
        }
        /// <summary>Logs exception with debug log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsDebug(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogDebug(exception, message, args);
            return true;
        }
        /// <summary>Logs exception with trace log level.</summary>
        /// <remarks><para>See <see cref="ExceptionLoggingHelper"/> for more information.</para>
        /// <para>This method is null-logger-safe - if <paramref name="log"/> is null, message and exception won't be logged.</para></remarks>
        /// <param name="exception">Exception message to log.</param>
        /// <param name="log">Logger instance.</param>
        /// <param name="message">Log message template.</param>
        /// <param name="args">Structured log message arguments.</param>
        /// <returns>Always returns true.</returns>
        public static bool LogAsTrace(this Exception exception, ILogger log, string message, params object[] args)
        {
            log?.LogTrace(exception, message, args);
            return true;
        }
    }
}
