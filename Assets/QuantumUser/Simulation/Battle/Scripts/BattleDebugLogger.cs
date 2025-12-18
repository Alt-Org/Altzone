/// @file BattleDebugLogger.cs
/// <summary>
/// Contains @cref{Battle.QSimulation,BattleDebugLogger} class which provides custom debug logging methods for use in %Battle.
/// </summary>

//#define DEBUG_ASSERT_ENABLED_OVERRIDE
#define DEBUG_ASSERT_DISABLED_OVERRIDE

#if (!DEBUG_ASSERT_DISABLED_OVERRIDE && (UNITY_EDITOR || DEBUG_ASSERT_ENABLED_OVERRIDE))
#define DEBUG_ASSERT
#endif

// System usings
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

// Unity usings
using Debug = UnityEngine.Debug;

// Quantum usings
using Quantum;

namespace Battle.QSimulation
{
    /// <summary>
    /// Provides custom debug logging methods for use in %Battle to apply consistant formatting to all Log messages.
    /// </summary>
    ///
    /// @anchor BattleDebugLogger-LogMethodsDoc
    /// @bigtext{**Log Methods Overview**}
    ///
    /// **Log message format**:<br/>
    ///
    /// Unity console messages are formatted as such:<br/>
    /// `[%Battle] [(Source)] (%Log message)`<br/>
    /// Or with the %Quantum frame:<br/>
    /// `[%Battle] [(%Quantum frame number)] [(Source)] (%Log message)`
    ///
    /// On-screen console messages are formatted as such:<br/>
    /// `[(Source)] (%Log message)`<br/>
    /// Or with the %Quantum frame:<br/>
    /// `[(%Quantum frame number)] [(Source)] (%Log message)`
    ///
    /// The %Quantum frame number is always presented as 6 digits:<br/>
    /// `[001234]`
    ///
    /// **Log methods:**<br/>
    /// The Log method name structure consists of first the **Log type**, optionally followed by **"Format"**.<br/>
    /// `(%Log type)(Format?)`
    ///
    /// Methods have both a static and non-static version for use in different contexts.<br/>
    /// Other classes can choose to either create an instance of this, or opt to use static method calls.
    ///
    /// **Log method types:**<br/>
    /// |         |                                             |
    /// | :------ | :------------------------------------------ |
    /// | Log     | Logs a regular Log message in console.<br/> |
    /// | Warning | Logs a Warning Log message in console.<br/> |
    /// | Error   | Logs an Error Log message in console.       |
    ///
    /// **Log method arguments:**<br/>
    /// |                  |                                                                              |
    /// | :--------------- | :--------------------------------------------------------------------------- |
    /// | f                | %Quantum frame for use in message formatting.<br/>                           |
    /// | source           | Source text for use in message formatting.<br/>                              |
    /// | message / format | The message or message format for the formatted message that is Logged.<br/> |
    /// | logTarget        | Target output console for log messages.                                      |
    ///
    /// **Static and Instance methods:**<br/>
    /// All Static Log methods take source as an argument.<br/>
    /// Instance methods use the source given when creating the instance.
    ///
    /// See @ref BattleDebugLogger-CreateMethods "Create Methods"
    ///
    /// **Passing %Quantum frame:**<br/>
    /// All Log methods have two versions:<br/>
    /// One that takes a %Quantum frame as an argument.<br/>
    /// One that doesn't.<br/>
    /// If the %Quantum frame is available it can be printed in the log message.
    ///
    /// **Formatted Log messages:**<br/>
    /// Methods that have the **"Format"** suffix in the method name take a format string in place of the message
    /// followed by an arbitrary amount of arguments for formatting.<br/>
    /// The format string and arguments are formatted same as when using [System.String.Format@u-exlink](https://learn.microsoft.com/en-us/dotnet/api/system.string.format).
    ///
    /// **Log Target**<br/>
    /// Methods that takes the @cref{LogTarget} as an argument have the option to choose which console output will be used.<br/>
    /// Default output console for logs of type Log and Warning will be Unity console and logs of type Error will be logged to both.<br/>
    /// With @cref{LogTarget} you can choose to manually use on-screen console with all log types.
    ///
    /// **Examples:**<br/>
    /// ```cs
    /// BattleDebugLogger.Log(f, nameof(ExampleClass), "Example message");
    /// BattleDebugLogger.WarningFormat(nameof(ExampleClass), "Example format {0}", value);
    ///
    /// BattleDebugLogger debugLogger = BattleDebugLogger.Create<ExampleClass>();
    /// debugLogger.ErrorFormat(f, "Example format {0} {1}", value1, value2);
    /// debugLogger.Log("Example message");
    /// debugLogger.Log("Example message", BattleDebugLogger.LogTarget.OnScreenConsole);
    /// ```
    ///
    /// @bigtext{@ref BattleDebugLogger-StaticLogMethods "All Static Log Methods"}<br/>
    /// @bigtext{@ref BattleDebugLogger-LogMethods "All Instance Log Methods"}
    ///
    /// @anchor BattleDebugLogger-AssertMethodsDoc
    /// @bigtext{**Assert Methods Overview**}
    ///
    /// Like Log methods, there are both a static and non-static version for use in different contexts.<br/>
    /// Like Log methods, there's also assert format methods.<br/>
    /// Assert methods take same arguments as Log methods but have additional `condition` or `assertCode` argument which is before `message` or `format`.<br/>
    /// Assert methods that take in `condition` arguments simply assert that condition is true otherwise logs an error.<br/>
    /// Assert methods that take in `assertCode` arguments runs the assertion code and asserts that it returns true otherwise logs an error.<br/>
    /// Assert methods are only compiled and assertion code is ran when `DEBUG_ASSERT` is defined.
    ///
    /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview"
    ///
    /// **Examples:**<br/>
    /// ```cs
    /// BattleDebugLogger.DevAssert(nameof(ExampleClass), exampleValue > -1, "Example message");
    ///
    /// BattleDebugLogger debugLogger = BattleDebugLogger.Create<ExampleClass>();
    /// debugLogger.DevAssert(f, exampleValue > -1, "Example message");
    /// debugLogger.DevAssert(() =>
    /// {
    ///     for (int i = 0; i < exampleArray.Length; i++)
    ///     {
    ///         if (exampleArray[i] < 0) return false;
    ///     }
    ///     return true;
    /// }, "Example message");
    /// ```
    ///
    /// @bigtext{@ref BattleDebugLogger-StaticAssertMethods "All Static Assert Methods"}<br/>
    /// @bigtext{@ref BattleDebugLogger-AssertMethods "All Instance Assert Methods"}
    public class BattleDebugLogger
    {
        /// <summary>
        /// Enum used to define log message types.
        /// </summary>
        public enum LogType
        {
            /// <summary>Log message of type Log.</summary>
            Log,

            /// <summary>Log message of type Warning.</summary>
            Warning,

            /// <summary>Log message of type Error.</summary>
            Error
        }

        /// <summary>
        /// Enum used to define where to display log messages.
        /// </summary>
        [Flags]
        public enum LogTarget
        {
            /// <summary>Logs messages to Unity console.</summary>
            UnityConsole,

            /// <summary>Logs messages to on-screen console.</summary>
            OnScreenConsole
        }

        /// <summary>
        /// Establishes link between <see cref="Battle.View.UI.BattleUiDebugConsoleHandler">BattleUiDebugConsoleHandler</see>.
        /// </summary>
        ///
        /// <param name="addLogToOnScreenConsoleFnRef"><see cref="Battle.View.UI.BattleUiDebugConsoleHandler.AddLog">BattleUiDebugConsoleHandler.AddLog</see> function reference.</param>
        public static void InitOnScreenConsoleLink(Action<LogType, string, string> addLogToOnScreenConsoleFnRef)
        {
            s_addLogToOnScreenConsoleFnRef = addLogToOnScreenConsoleFnRef;
        }

        /// @anchor BattleDebugLogger-CreateMethods
        /// @name Create Methods
        /// Methods for creating a BattleDebugLogger instance.
        /// @{
        #region Public Create Methods

        /// <summary>
        /// Creates a BattleDebugLogger instance using the given type <typeparamref name="T"/> as a source.
        /// </summary>
        /// @ref BattleDebugLogger-CreateMethods "Create Methods"
        ///
        /// <typeparam name="T">The type used as source when creating the instance.</typeparam>
        ///
        /// <returns>The created BattleDebugLogger instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create<T>()
        {
            return new BattleDebugLogger(typeof(T).Name);
        }

        /// <summary>
        /// Creates a BattleDebugLogger instance using the specified type <paramref name="source"/> as a source.
        /// </summary>
        /// @ref BattleDebugLogger-CreateMethods "Create Methods"
        ///
        /// <param name="source">The type used as source when creating the instance.</param>
        ///
        /// <returns>The created BattleDebugLogger instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create(System.Type source)
        {
            return new BattleDebugLogger(source.Name);
        }

        /// <summary>
        /// Creates a BattleDebugLogger instance using the specified string <paramref name="source"/> as a source.
        /// </summary>
        /// @ref BattleDebugLogger-CreateMethods "Create Methods"
        ///
        /// <param name="source">The name of the source used when creating the instance.</param>
        ///
        /// <returns>The created BattleDebugLogger instance.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BattleDebugLogger Create(string source)
        {
            return new BattleDebugLogger(source);
        }

        #endregion Create Methods
        /// @}

        /// @anchor BattleDebugLogger-StaticLogMethods
        /// @name Static Log Methods
        /// Static Log methods that can be used without a BattleDebugLogger instance.
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        /// @{
        #region Public Static Log Methods

        /// <summary>
        /// Logs the given <paramref name="message"/>
        /// using the given <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string source, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogFormat(StaticFormatUnityNoFrame, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Log, string.Format(StaticSourceFormatOnScreenNoFrame, source), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/>
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(Frame f, string source, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogFormat(StaticFormatUnity, f.Number, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Log, string.Format(StaticSourceFormatOnScreen, f.Number, source), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/>
        /// using the given <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(string source, string format, params object[] args)
        {
            Log(source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/>
        /// using the given <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(string source, string format, LogTarget logTarget, params object[] args)
        {
            Log(source, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/>
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(Frame f, string source, string format, params object[] args)
        {
            Log(f, source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/>
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void LogFormat(Frame f, string source, string format, LogTarget logTarget, params object[] args)
        {
            Log(f, source, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning
        /// using the given <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string source, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogWarningFormat(StaticFormatUnityNoFrame, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Warning, string.Format(StaticSourceFormatOnScreenNoFrame, source), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Frame f, string source, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogWarningFormat(StaticFormatUnity, f.Number, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Warning, string.Format(StaticSourceFormatOnScreen, f.Number, source), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning
        /// using the given <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(string source, string format, params object[] args)
        {
            Warning(source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning
        /// using the given <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(string source, string format, LogTarget logTarget, params object[] args)
        {
            Warning(source, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(Frame f, string source, string format, params object[] args)
        {
            Warning(f, source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void WarningFormat(Frame f, string source, string format, LogTarget logTarget, params object[] args)
        {
            Warning(f, source, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error
        /// using the given <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string source, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogErrorFormat(StaticFormatUnityNoFrame, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Error, string.Format(StaticSourceFormatOnScreenNoFrame, source), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Frame f, string source, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogErrorFormat(StaticFormatUnity, f.Number, source, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Error, string.Format(StaticSourceFormatOnScreen, f.Number, source), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error
        /// using the given <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(string source, string format, params object[] args)
        {
            Error(source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error
        /// using the given <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(string source, string format, LogTarget logTarget, params object[] args)
        {
            Error(source, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(Frame f, string source, string format, params object[] args)
        {
            Error(f, source, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods "Static Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ErrorFormat(Frame f, string source, string format, LogTarget logTarget, params object[] args)
        {
            Error(f, source, string.Format(format, args), logTarget);
        }

        #endregion Public Static Log Methods
        /// @}

        /// @anchor BattleDebugLogger-StaticAssertMethods
        /// @name Static Assert Methods
        /// Static Assert Methods that can be used without a BattleDebugLogger instance.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        /// @{
        #region Public Static Assert Methods

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="Error(string, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using @cref{BattleDebugLogger.Error(string\, string\, LogTarget)}.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssert(string source, bool condition, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!condition)
            {
                Error(source, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="Error(Frame, string, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="Error(Frame, string, string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssert(Frame f, string source, bool condition, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!condition)
            {
                Error(f, source, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="Error(string, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="Error(string, string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssert(string source, Func<bool> assertCode, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!assertCode())
            {
                Error(source, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="Error(Frame, string, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="Error(Frame, string, string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssert(Frame f, string source, Func<bool> assertCode, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!assertCode())
            {
                Error(f, source, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(string, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(string, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(string source, bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(source, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(string, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(string, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(string source, bool condition, string format, LogTarget logTarget, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(source, format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(Frame, string, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(Frame f, string source, bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(f, source, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(Frame, string, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(Frame f, string source, bool condition, string format, LogTarget logTarget, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(f, source, format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(string, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(string, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(string source, Func<bool> assertCode, string format, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(source, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(string, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(string, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(string source, Func<bool> assertCode, string format, LogTarget logTarget, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(source, format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(Frame, string, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(Frame f, string source, Func<bool> assertCode, string format, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(f, source, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-StaticAssertMethods "Static Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(Frame, string, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DevAssertFormat(Frame f, string source, Func<bool> assertCode, string format, LogTarget logTarget, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(f, source, format, logTarget, args);
            }
        }

        #endregion Public Static Assert Methods
        /// @}

        /// @anchor BattleDebugLogger-LogMethods
        /// @name Log Methods
        /// Public Log methods that can only be used with a BattleDebugLogger instance.
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        /// @{
        #region Public Log Methods

        /// <summary>
        /// Logs the given <paramref name="message"/>.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogFormat(_instanceFormatUnityNoFrame, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Log, string.Format(_instanceSourceFormatOnScreenNoFrame), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/>.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(Frame f, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogFormat(_instanceFormatUnity, f.Number, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Log, string.Format(_instanceSourceFormatOnScreen, f.Number), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/><br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/><br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(string format, LogTarget logTarget, params object[] args)
        {
            Log(string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/><br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(Frame f, string format, params object[] args)
        {
            Log(f, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/><br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void LogFormat(Frame f, string format, LogTarget logTarget, params object[] args)
        {
            Log(f, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogWarningFormat(_instanceFormatUnityNoFrame, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Warning, string.Format(_instanceSourceFormatOnScreenNoFrame), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(Frame f, string message, LogTarget logTarget = LogTarget.UnityConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogWarningFormat(_instanceFormatUnity, f.Number, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Warning, string.Format(_instanceSourceFormatOnScreen, f.Number), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(string format, params object[] args)
        {
            Warning(string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(string format, LogTarget logTarget, params object[] args)
        {
            Warning(string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(Frame f, string format, params object[] args)
        {
            Warning(f, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void WarningFormat(Frame f, string format, LogTarget logTarget, params object[] args)
        {
            Warning(f, string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogErrorFormat(_instanceFormatUnityNoFrame, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Error, string.Format(_instanceSourceFormatOnScreenNoFrame), message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Frame f, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (logTarget.HasFlag(LogTarget.UnityConsole)) Debug.LogErrorFormat(_instanceFormatUnity, f.Number, message);
            if (logTarget.HasFlag(LogTarget.OnScreenConsole)) s_addLogToOnScreenConsoleFnRef(LogType.Error, string.Format(_instanceSourceFormatOnScreen, f.Number), message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(string format, LogTarget logTarget, params object[] args)
        {
            Error(string.Format(format, args), logTarget);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(Frame f, string format, params object[] args)
        {
            Error(f, string.Format(format, args));
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)<br/>
        /// Output console can be chosen by using <paramref name="logTarget"/>.
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods "Log Methods"
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        /// <param name="logTarget">The target output console.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(Frame f, string format, LogTarget logTarget, params object[] args)
        {
            Error(f, string.Format(format, args), logTarget);
        }

        #endregion Public Log Methods
        /// @}

        /// @anchor BattleDebugLogger-AssertMethods
        /// @name Assert Methods
        /// Public Assert methods that can only be used with a BattleDebugLogger instance.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        /// @{
        #region Public Assert Methods

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="Error(string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="Error(string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssert(bool condition, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!condition)
            {
                Error(message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="Error(Frame, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="Error(Frame, string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssert(Frame f, bool condition, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!condition)
            {
                Error(f, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="Error(string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="Error(string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssert(Func<bool> assertCode, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!assertCode())
            {
                Error(message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="Error(Frame, string, LogTarget)"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="Error(Frame, string, LogTarget)"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="message">The message that is Logged.</param>
        /// <param name="logTarget">The target output console.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssert(Frame f, Func<bool> assertCode, string message, LogTarget logTarget = LogTarget.UnityConsole | LogTarget.OnScreenConsole)
        {
            if (!assertCode())
            {
                Error(f, message, logTarget);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(bool condition, string format, LogTarget logTarget, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(Frame, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Frame f, bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(f, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="condition"/> is true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// When assertion fails, logs an error using <see cref="ErrorFormat(Frame, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="condition">Condition must be true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Frame f, bool condition, string format, LogTarget logTarget, params object[] args)
        {
            if (!condition)
            {
                ErrorFormat(f, format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Func<bool> assertCode, string format, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Func<bool> assertCode, string format, LogTarget logTarget, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(format, logTarget, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(Frame, string, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Frame f, Func<bool> assertCode, string format, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(f, format, args);
            }
        }

        /// <summary>
        /// Asserts that <paramref name="assertCode"/> returns true during debugging
        /// and logs an <see cref="ErrorFormat(Frame, string, LogTarget, object[])"/> log message if it is false.<br/>
        /// This method is only compiled and assertion code is ran when <c>DEBUG_ASSERT</c> is defined.
        /// </summary>
        /// @ref BattleDebugLogger-AssertMethods "Assert Methods"
        ///
        /// Runs assertion code which can be any complicated check.<br/>
        /// When assertion code fails, logs an error using <see cref="ErrorFormat(Frame, string, LogTarget, object[])"/> method.
        ///
        /// See @ref BattleDebugLogger-AssertMethodsDoc "Assert Methods Overview" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="assertCode">Assertion code must return true, otherwise logs an error.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="logTarget">The target output console.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [Conditional("DEBUG_ASSERT")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void DevAssertFormat(Frame f, Func<bool> assertCode, string format, LogTarget logTarget, params object[] args)
        {
            if (!assertCode())
            {
                ErrorFormat(f, format, logTarget, args);
            }
        }

        #endregion Public Assert Methods
        /// @}

        /// <summary>Constant format for Log messages when called through static methods.</summary>
        private const string StaticFormatUnity                          = "[Battle] [{0:D6}] [{1}] {2}";
        /// <summary>Constant format for Log messages when called through static methods with no %Quantum frame given.</summary>
        private const string StaticFormatUnityNoFrame                   = "[Battle] [{0}] {1}";
        /// <summary>Constant format template for Log messages used to create the message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceFormatUnityTemplate                = "[Battle] [{{0:D6}}] [{0}] {{1}}";
        /// <summary>Constant format template for Log messages used to create the no %Quantum frame message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceFormatUnityNoFrameTemplate         = "[Battle] [{0}] {{0}}";


        /// <summary>Constant format for on-screen Log messages when called through static methods.</summary>
        private const string StaticSourceFormatOnScreen                  = "[{0:D6}] [{1}]";
        /// <summary>Constant format for on-screen Log messages when called through static methods with no %Quantum frame given.</summary>
        private const string StaticSourceFormatOnScreenNoFrame           = "[{0}]";
        /// <summary>Constant format template for on-screen Log messages used to create the message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceSourceFormatOnScreenTemplate        = "[{{0:D6}}] [{0}]";
        /// <summary>Constant format template for on-screen Log messages used to create the no %Quantum frame message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceSourceFormatOnScreenNoFrameTemplate = "[{0}]";

        /// <summary>Saved Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _instanceFormatUnity;
        /// <summary>Saved no %Quantum frame Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _instanceFormatUnityNoFrame;

        /// <summary>Saved on-screen Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _instanceSourceFormatOnScreen;
        /// <summary>Saved no %Quantum frame on-screen Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _instanceSourceFormatOnScreenNoFrame;

        /// <summary>Function reference to <see cref="Battle.View.UI.BattleUiDebugConsoleHandler.AddLog">BattleUiDebugConsoleHandler.AddLog</see>.</summary>
        private static Action<LogType, string, string> s_addLogToOnScreenConsoleFnRef;

        /// <summary>
        /// Private constructor method called when any of the <b>Create methods</b> are called.<br/>
        /// Creates the Log message formats used when calling Log methods based on constant templates.
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-CreateMethods "Create Methods"
        ///
        /// <param name="source">The name of the source used when creating the Log message formats.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BattleDebugLogger(string source)
        {
            _instanceFormatUnity                 = string.Format(InstanceFormatUnityTemplate, source);
            _instanceFormatUnityNoFrame          = string.Format(InstanceFormatUnityNoFrameTemplate, source);
            _instanceSourceFormatOnScreen        = string.Format(InstanceSourceFormatOnScreenTemplate, source);
            _instanceSourceFormatOnScreenNoFrame = string.Format(InstanceSourceFormatOnScreenNoFrameTemplate, source);
        }
    }
}
