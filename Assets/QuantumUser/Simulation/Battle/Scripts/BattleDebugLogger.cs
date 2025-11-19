/// @file BattleDebugLogger.cs
/// <summary>
/// Contains @cref{Battle.QSimulation,BattleDebugLogger} class which provides custom debug logging methods for use in %Battle.
/// </summary>

// System usings
using System.Runtime.CompilerServices;

// Unity usings
using UnityEngine;

// Quantum usings
using Quantum;

namespace Battle.QSimulation
{
    /// <summary>
    /// Provides custom debug logging methods for use in %Battle to apply consistant formatting to all Log messages.
    /// </summary>
    ///
    ///  **Log message format**:<br/>
    /// Log messages are formatted as such:<br/>
    /// `[%Battle] [(Source)] (Log message)`<br/>
    /// Or with the %Quantum frame:<br/>
    /// `[%Battle] [(%Quantum frame number)] [(Source)] (Log message)`<br/>
    /// The %Quantum frame number is always presented as 6 digits:<br/>
    /// `[001234]`
    ///
    /// Methods have both a static and non-static version for use in different contexts.<br/>
    /// Other classes can choose to either create an instance of this, or opt to use static method calls.
    ///
    /// @anchor BattleDebugLogger-LogMethodsDoc
    /// **Log methods:**<br/>
    /// The Log method name structure consists of first the **Log type**, optionally followed by **"Format"**.<br/>
    /// `(Log type)(Format?)`
    /// 
    /// **Log method types:**<br/>
    /// |         |                                             |
    /// | :------ | :------------------------------------------ |
    /// | Log     | Logs a regular Log message in console.<br/> |
    /// | Warning | Logs a Warning Log message in console.<br/> |
    /// | Error   | Logs an Error Log message in console.       |
    ///
    /// **Log method arguments:**<br/>
    /// |                  |                                                                         |
    /// | :--------------- | :---------------------------------------------------------------------- |
    /// | f                | %Quantum frame for use in message formatting.<br/>                      |
    /// | source           | Source text for use in message formatting.<br/>                         |
    /// | message / format | The message or message format for the formatted message that is Logged. |
    ///
    /// **Static and Instance methods:**<br/>
    /// All Static Log methods take source as an argument.<br/>
    /// Instance methods use the source given when creating the instance.
    ///
    /// **Passing %Quantum frame:**<br/>
    /// All Log methods have two versions:<br/>
    /// One that takes a %Quantum frame as an argument.<br/>
    /// One that doesn't.
    ///
    /// **Formatted Log messages:**<br/>
    /// Methods that have the **"Format"** suffix in the method name take a format string in place of the message
    /// followed by an arbitrary amount of arguments for formatting.<br/>
    /// The format string and arguments are formatted same as when using [System.String.Format@u-exlink](https://learn.microsoft.com/en-us/dotnet/api/system.string.format).
    ///
    /// **Examples:**<br/>
    /// ```cs
    /// BattleDebugLogger.Log(f, nameof(ExampleClass), "Example message");
    /// BattleDebugLogger.WarningFormat(nameof(ExampleClass), "Example format {0}", value);
    ///
    /// BattleDebugLogger debugLogger = BattleDebugLogger.Create<ExampleClass>();
    /// debugLogger.ErrorFormat(f, "Example format {0} {1}", value1, value2);
    /// debugLogger.Log("Example message");
    /// ```
    public class BattleDebugLogger
    {
        /// @anchor BattleDebugLogger-CreateMethods
        /// @name Create Methods
        /// Methods for creating a BattleDebugLogger instance.
        /// @{
        #region Public Create Methods

        /// <summary>
        /// Creates a BattleDebugLogger instance using the given type <typeparamref name="T"/> as a source.
        /// </summary>
        /// @ref BattleDebugLogger-CreateMethods
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
        /// @ref BattleDebugLogger-CreateMethods
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
        /// @ref BattleDebugLogger-CreateMethods
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
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// @{
        #region Public Static Log Methods

        /// <summary>
        /// Logs the given <paramref name="message"/>
        /// using the given <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(string source, string message)
        {
            Debug.LogFormat(StaticFormatNoFrame, source, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/>
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="source">The name of the source used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Log(Frame f, string source, string message)
        {
            Debug.LogFormat(StaticFormat, f.Number, source, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/>
        /// using the given <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Logs the given <paramref name="message"/> as a Warning
        /// using the given <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(string source, string message)
        {
            Debug.LogWarningFormat(StaticFormatNoFrame, source, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Warning(Frame f, string source, string message)
        {
            Debug.LogWarningFormat(StaticFormat, f.Number, source, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning
        /// using the given <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Logs the given <paramref name="message"/> as an Error
        /// using the given <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="source">The name of the source used when formatting the Error Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(string source, string message)
        {
            Debug.LogErrorFormat(StaticFormatNoFrame, source, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="source">The name of the source used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Error(Frame f, string source, string message)
        {
            Debug.LogErrorFormat(StaticFormat, f.Number, source, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error
        /// using the given <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// using the given %Quantum frame <paramref name="f"/> and <paramref name="source"/> when formatting the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-StaticLogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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

        #endregion Public Static Log Methods
        /// @}

        /// @anchor BattleDebugLogger-LogMethods
        /// @name Log Methods
        /// Public Log methods that can only be used with a BattleDebugLogger instance.
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// @{
        #region Public Log Methods

        /// <summary>
        /// Logs the given <paramref name="message"/>.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(string message)
        {
            Debug.LogFormat(_formatNoFrame, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/>.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="f">The %Quantum frame used when formatting the Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Log(Frame f, string message)
        {
            Debug.LogFormat(_format, f.Number, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/><br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Log message.<br/>
        /// Output Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Logs the given <paramref name="message"/> as a Warning.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(string message)
        {
            Debug.LogWarningFormat(_formatNoFrame, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as a Warning.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="f">The %Quantum frame used when formatting the Warning Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Warning(Frame f, string message)
        {
            Debug.LogWarningFormat(_format, f.Number, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as a Warning.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Warning Log message.<br/>
        /// Output Warning Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Logs the given <paramref name="message"/> as an Error.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(string message)
        {
            Debug.LogErrorFormat(_formatNoFrame, message);
        }

        /// <summary>
        /// Logs the given <paramref name="message"/> as an Error.<br/>
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        /// 
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="message">The message that is Logged.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Error(Frame f, string message)
        {
            Debug.LogErrorFormat(_format, f.Number, message);
        }

        /// <summary>
        /// Logs a formatted message based on the given <paramref name="format"/> and <paramref name="args"/> as an Error.<br/>
        /// Uses the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
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
        /// Uses the given %Quantum frame <paramref name="f"/> and the source given when creating a BattleDebugLogger instance to format the Error Log message.<br/>
        /// Output Error Log message: [%Battle] [(frame number)] [(source)] (formatted message)
        /// </summary>
        /// @ref BattleDebugLogger-LogMethods
        ///
        /// See @ref BattleDebugLogger-LogMethodsDoc "Log methods" for more info.
        ///
        /// <param name="f">The %Quantum frame used when formatting the Error Log message.</param>
        /// <param name="format">The message string to be Logged after formatting.</param>
        /// <param name="args">The arguments for formatting the message string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ErrorFormat(Frame f, string format, params object[] args)
        {
            Error(f, string.Format(format, args));
        }

        #endregion Public Log Methods
        /// @}

        /// <summary>Constant format for log messages when called through static methods.</summary>
        private const string StaticFormat                  = "[Battle] [{0:D6}] [{1}] {2}";
        /// <summary>Constant format for log messages when called through static methods with no %Quantum frame given.</summary>
        private const string StaticFormatNoFrame           = "[Battle] [{0}] {1}";
        /// <summary>Constant format template for Log messages used to create the message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceFormatTemplate        = "[Battle] [{{0:D6}}] [{0}] {{1}}";
        /// <summary>Constant format template for Log messages used to create the no %Quantum frame message format when creating a BattleDebugLogger instance.</summary>
        private const string InstanceFormatNoFrameTemplate = "[Battle] [{0}] {{0}}";

        /// <summary>Saved Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _format;
        /// <summary>Saved no %Quantum frame Log message format, set when creating a BattleDebugLogger instance.</summary>
        private readonly string _formatNoFrame;

        /// <summary>
        /// Private constructor method called when any of the <b>Create methods</b> are called.<br/>
        /// Creates the Log message formats used when calling Log methods based on constant templates.
        /// </summary>
        ///
        /// See @ref BattleDebugLogger-CreateMethods "Create methods"
        /// 
        /// <param name="source">The name of the source used when creating the Log message formats.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BattleDebugLogger(string source)
        {
            _format        = string.Format(InstanceFormatTemplate, source);
            _formatNoFrame = string.Format(InstanceFormatNoFrameTemplate, source);
        }
    }
}
