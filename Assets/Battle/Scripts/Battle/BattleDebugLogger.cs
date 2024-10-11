using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

namespace Battle.Scripts.Battle
{
    internal class BattleDebugLogger
    {
        #region Public Static Methods

        public static void Init(SyncedFixedUpdateClock syncedFixedUpdateClock)
        {
            s_syncedFixedUpdateClock = syncedFixedUpdateClock;
            Debug.Log("[[BATTLE LOG START]]");
            s_battleDebugLogger = new(nameof(BattleDebugLogger));

            s_fileWriter = null;
            s_filePath = null;

            if (OpenFile()) Application.logMessageReceivedThreaded += UnityLogCallback;
        }

        public static void End()
        {
            Application.logMessageReceivedThreaded -= UnityLogCallback;
            CloseFile();
            if (s_filePath != null) SendFile();
            s_filePath = null;
        }

        #endregion Public Static Methods

        #region Public Constructors

        public BattleDebugLogger(object source)
        {
            _loggerFormat = "[{0:000000}] [BATTLE] [" + source.GetType().Name + "] {1}";
        }

        public BattleDebugLogger(string source)
        {
            _loggerFormat = "[{0:000000}] [BATTLE] [" + source + "] {1}";
        }

        #endregion Public Constructors

        #region Public Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogInfo(string msg) { Debug.Log(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogInfo(string msgFormat, params object[] args) { LogInfo(string.Format(msgFormat, args)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogWarning(string msg) { Debug.LogWarning(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogWarning(string msgFormat, params object[] args) { LogWarning(string.Format(msgFormat, args)); }

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogError(string msg) { Debug.LogError(string.Format(_loggerFormat, s_syncedFixedUpdateClock.UpdateCount, msg)); }
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public void LogError(string msgFormat, params object[] args) { LogError(string.Format(msgFormat, args)); }

        #endregion Public Methods

        #region Private Static Fields

        // Files
        private static StreamWriter s_fileWriter;
        private static readonly string s_fileNameFormat = "BattleLog-{0:yyy-MM-dd-HH-mm-ss}-UTC-{{0:d2}}.log";
        private static readonly int s_fileSuffixMax = 99;
        private static readonly Encoding s_fileEncoding = new UTF8Encoding(false, false);
        private static string s_filePath;
        private static readonly int s_fileReadAttemptLimit = 3;

        // Game Time
        private static SyncedFixedUpdateClock s_syncedFixedUpdateClock;

        #endregion Private Static Fields

        #region Private Fields
        private readonly string _loggerFormat;
        #endregion Private Fields

        #region DEBUG
        private static BattleDebugLogger s_battleDebugLogger;
        #endregion DEBUG

        #region Private Methods

        private static bool OpenFile()
        {
            string fileName = string.Format(s_fileNameFormat, DateTime.UtcNow);
            string basePath = Path.Combine(Application.persistentDataPath, fileName);

            string path;
            int suffix = 0;

            for (;;)
            {
                path = string.Format(basePath, suffix);
                s_battleDebugLogger.LogInfo("Trying to open log file: {0} (write)", path);

                try
                {
                    // Open for overwrite!
                    s_fileWriter = new StreamWriter(path, false, s_fileEncoding) { AutoFlush = true };
                    break;
                }
                catch (IOException)
                {
                    suffix++;
                    if (suffix > s_fileSuffixMax)
                    {
                        s_fileWriter = null;
                        s_filePath = null;
                        s_battleDebugLogger.LogError("Unable to open log file (write)");
                        return false;
                    }
                }
            }

            s_filePath = path;

            s_battleDebugLogger.LogInfo("Log file opened (write)");
            return true;
        }

        private static void CloseFile()
        {
            if (s_fileWriter != null)
            {
                s_fileWriter.Close();
                s_fileWriter = null;
                s_battleDebugLogger.LogInfo("Log file closed (write)");
            }
        }

        private static void SendFile()
        {
            /*
            string path = s_filePath;
            StreamReader fileReader = null;

            int attempt = 1;

            for (;;)
            {
                s_battleDebugLogger.LogInfo("Trying to open log file: {0} (read)", path);

                try
                {
                    // Open for read!
                    fileReader = new(path, s_fileEncoding);
                    break;
                }
                catch (IOException)
                {
                    attempt++;
                    if (attempt > s_fileReadAttemptLimit)
                    {
                        s_battleDebugLogger.LogError("Unable to open log file (read)");
                        return;
                    }
                }
            }

            s_battleDebugLogger.LogInfo("Log file opened (read)");
            s_battleDebugLogger.LogInfo("Reading file...");
            string fileString = fileReader.ReadToEnd();
            fileReader.Close();
            s_battleDebugLogger.LogInfo("Log file closed (read)");
            /**/

            /*
            string fileString = "log msg1\nlog msg2\nlog msg3";
            /**/

            /*
            List<IMultipartFormSection> body = new()
            {
                new MultipartFormFileSection("logFile", fileString, s_fileEncoding, "BattleLog.log")
            };

            s_battleDebugLogger.LogInfo("Sending log file to server");
            ServerManager.Instance.SendDebugLogFile(body, "my_secret", null, response =>
            {
                if (response.result == UnityWebRequest.Result.Success) s_battleDebugLogger.LogInfo("Server response: SUCCESS");
                else s_battleDebugLogger.LogError("Server response: ERROR {0} {1}", response.error, response.downloadHandler.text);
            });
            /**/
        }

        private static void UnityLogCallback(string logString, string stackTrace, LogType type)
        {
            StringBuilder stringBuilder = new();

            bool repeat = true;
            int startIndex;
            string replacementString;

            for (;;)
            {
                startIndex = 0;

                for (int i = 0;;)
                {
                    if (i >= logString.Length)
                    {
                        if (startIndex < i) stringBuilder.Append(logString, startIndex, i - startIndex);
                        break;
                    }

                    switch (logString[i])
                    {
                        case '\\': replacementString = "\\\\"; goto Replace;
                        case '\n': replacementString = "\\n" ; goto Replace;
                    }

                    i++;
                    continue;

                Replace:
                    if (startIndex < i) stringBuilder.Append(logString, startIndex, i - startIndex);
                    stringBuilder.Append(replacementString);
                    i++;
                    startIndex = i;
                }

                if (!repeat) break;

                stringBuilder.Append("\\,");
                stringBuilder.Append(type.ToString());
                stringBuilder.Append("\\,");
                logString = stackTrace;
                repeat = false;
            }

            s_fileWriter.WriteLine(stringBuilder.ToString());
        }

        #endregion Private Methods
    }
}
