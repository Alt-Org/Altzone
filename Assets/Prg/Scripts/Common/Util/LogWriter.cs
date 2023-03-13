using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    /// UNITY wrapper for <c>LogFileWriter</c> that catches all log messages from UNITY and writes them to a file.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class LogWriter : MonoBehaviour
    {
        public static void AddLogLineContentFilter(Func<string, string> filter)
        {
            // By design there is now way to remove filter once it has been installed.
            LogFileWriter.LogLineContentFilter += filter;
        }

        private static LogWriter _instance;

        [Header("Live Data"), SerializeField] private string _fileName;
        private LogFileWriter _logFileWriter;

        private void Awake()
        {
            if (_instance != null)
            {
                throw new UnityException("LogWriter already created");
            }
            // Register us as the singleton!
            _instance = this;
        }

        private void OnEnable()
        {
            _logFileWriter = LogFileWriter.CreateLogFileWriter();
            _fileName = _logFileWriter.Filename;
        }

        private void OnDestroy()
        {
            // OnApplicationQuit() comes before OnDestroy() so we are *not* interested to listen it.

            _logFileWriter?.Close();
            _instance = null;
        }
    }

    /// <summary>
    /// Simple file logger that catches all log messages from UNITY and writes them to a file.
    /// </summary>
    public class LogFileWriter
    {
        private const string LogFileSuffix = "game.log";

        private static readonly Encoding Encoding = new UTF8Encoding(false, false);
        private static readonly object Lock = new();
        private static readonly StringBuilder Builder = new(500);

        private static int _prevLogLineCount;
        private static string _prevLogString = string.Empty;

        private static LogFileWriter _instance;

        public static Func<string, string> LogLineContentFilter;

        public string Filename { get; }

        public static LogFileWriter CreateLogFileWriter() => new();

        private readonly StreamWriter _writer;

        private LogFileWriter()
        {
            try
            {
                var baseName = GetLogName();
                var baseFileName = Path.Combine(Application.persistentDataPath, baseName);
                Filename = baseFileName;
                var retry = 1;
                for (;;)
                {
                    try
                    {
                        // Open for overwrite!
                        _writer = new StreamWriter(Filename, false, Encoding) { AutoFlush = true };
                        break;
                    }
                    catch (IOException)
                    {
                        // Sharing violation if more than one instance at the same time
                        if (++retry > 10)
                        {
                            throw new UnityException("Unable to allocate log file");
                        }
                        var newSuffix = $"{retry:D2}_{LogFileSuffix}";
                        Filename = baseFileName.Replace(LogFileSuffix, newSuffix);
                    }
                }
                // Show effective log filename.
                if (AppPlatform.IsWindows)
                {
                    Filename = AppPlatform.ConvertToWindowsPath(Filename);
                }
            }
            catch (Exception x)
            {
                _writer = null;
                UnityEngine.Debug.LogWarning($"unable to create log file '{Filename}'");
                UnityEngine.Debug.LogException(x);
                throw;
            }
            UnityEngine.Debug.Log($"LogWriter Open file {Filename}");
            _instance = this;
            Application.logMessageReceivedThreaded += UnityLogCallback;
        }

        public void Close()
        {
            Application.logMessageReceivedThreaded -= UnityLogCallback;
            _instance = null;
            LogLineContentFilter = null;
            UnityEngine.Debug.Log($"LogWriter Close file {Filename}");
        }

        private void WriteLog(string message)
        {
            if (_writer == null)
            {
                return;
            }
            _writer.WriteLine(message);
            _writer.Flush();
        }

        /// <summary>
        /// Thread safe callback to listen UNITY Debug messages and write them to a file.
        /// </summary>
        /// <remarks>
        /// This is thread safe because Debug.Log can be called from background threads as well.
        /// </remarks>
        private static void UnityLogCallback(string logString, string stackTrace, LogType type)
        {
            lock (Lock)
            {
                if (logString.Equals(_prevLogString, StringComparison.Ordinal) && type != LogType.Error)
                {
                    // Filter away messages that comes in every frame like:
                    // There are no audio listeners in the scene. Please ensure there is always one audio listener in the scene
                    // Warning	Mesh has more materials (2) than subsets (1)
                    _prevLogLineCount += 1;
                    return;
                }

                if (_prevLogLineCount > 2)
                {
                    _instance.WriteLog($"duplicate_lines {_prevLogLineCount}");
                    _prevLogLineCount = 0;
                }
                _prevLogString = logString;
                if (LogLineContentFilter != null)
                {
                    // As we can modify the input parameter on the fly we must call each delegate separately with correct input.
                    // - avoid DynamicInvoke because it can be order of magnitude slower than "function pointer".
                    var invocationList = LogLineContentFilter.GetInvocationList();
                    if (invocationList.Length == 1)
                    {
                        logString = LogLineContentFilter(logString);
                    }
                    else
                    {
                        foreach (var callback in invocationList)
                        {
                            logString = callback.DynamicInvoke(logString) as string;
                        }
                    }
                }
                // Reset builder
                Builder.Length = 0;

                // File log has timestamp (and optionally category) before message.
                Builder.AppendFormat("{0:HH:mm:ss.fff} ", DateTime.Now);
                if (type != LogType.Log)
                {
                    Builder.Append(type).Append(' ');
                }

                Builder.Append(logString);
                _instance.WriteLog(Builder.ToString());
                if (type != LogType.Error && type != LogType.Exception)
                {
                    return;
                }
                // Show stack trace only for real errors with proper call stack.
                if (stackTrace.Length > 5)
                {
                    Builder.Length = 0;
                    Builder.AppendFormat("{0:HH:mm:ss.fff}\t{1}\t{2}", DateTime.Now, "STACK", stackTrace);
                    _instance.WriteLog(Builder.ToString());
                }
            }
        }

        /// <summary>
        /// Gets log file name for current platform.
        /// </summary>
        /// <returns></returns>
        public static string GetLogName()
        {
            var isEditor = AppPlatform.IsEditor;
            if (!isEditor)
            {
                // Delete old files.
                var oldFiles = Directory.GetFiles(Application.persistentDataPath, $"*_{LogFileSuffix}");
                var today = DateTime.Now.Day;
                foreach (var oldFile in oldFiles)
                {
                    if (File.GetCreationTime(oldFile).Day != today)
                    {
                        try
                        {
                            File.Delete(oldFile);
                        }
                        catch (IOException)
                        {
                            // NOP - we just swallow it
                        }
                    }
                }
            }
            var prefix = isEditor ? "editor" : Application.platform.ToString().ToLower().Replace("player", string.Empty);
            var baseName = Application.productName.ToLower();
            return $"{prefix}_{baseName}_{LogFileSuffix}";
        }
    }
}