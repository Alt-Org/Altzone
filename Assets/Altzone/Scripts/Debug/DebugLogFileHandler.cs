using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

using UnityEngine;

namespace Altzone.Scripts.AzDebug
{
    public static class DebugLogFileHandler
    {
        #region Config

        // { Context

        public enum ContextID
        {
            MenuUI,
            Battle
        }

        private static readonly ContextTemplate[] s_contextTemplateArray = new ContextTemplate[]
        {
            // ID, LogName, LogDetailsFormat, SingleFile
            new(ContextID.MenuUI, "MenuUiLog", "",           true ),
            new(ContextID.Battle, "BattleLog", "{0}-{1:d2}", false)
        };

        // } Context

        // File
        private static readonly string s_fileNameFormatWithoutDetails = "{0}-{1:yyy-MM-dd-HH-mm-ss}-UTC-{{0:d2}}.log";  // (LogName)-(DateAndTime)-UTC-(Suffix).log
        private static readonly string s_fileNameFormatWithDetails = "{0}-{1:yyy-MM-dd-HH-mm-ss}-UTC-{2}-{{0:d2}}.log"; // (LogName)-(DateAndTime)-UTC-(Details)-(Suffix).log
        private static readonly Encoding s_fileEncoding = new UTF8Encoding(false, false);
        private static readonly int s_fileSuffixMax = 99;
        //private static readonly int s_fileReadAttemptLimit = 3;

        #endregion Config

        #region Public

        #region Public - Types

        public class ContextTemplate
        {
            public readonly ContextID ID;
            public readonly string LogName;
            public readonly string LogDetailsFormat;
            public readonly bool SingleFile;

            public ContextTemplate(ContextID id, string logName, string logDetailsFormat, bool singleFile)
            {
                ID = id;
                LogName = logName;
                LogDetailsFormat = logDetailsFormat;
                SingleFile = singleFile;
            }
        }

        public readonly struct Context
        {
            public readonly ContextTemplate Info;
            public readonly bool FileOpen;

            public Context(ContextTemplate template, bool fileOpen)
            {
                Info = template;
                FileOpen = fileOpen;
            }
        }

        #endregion Public - Types

        #region Public - Static Properties
        public static bool Initialized => s_initialized;
        public static Context CurrentContext => s_contextCurrent.Public;
        #endregion Public - Static Properties

        #region Public - Static Methods

        public static void Init(ContextID contextID)
        {
            if (s_initialized) return;

            s_contextStack = new();
            s_contextSingleFileArray = new ContextInternal[s_contextTemplateArray.Length];

            s_contextCurrent = ContextOpen(contextID);
            Application.logMessageReceivedThreaded += UnityLogCallback;

            s_initialized = true;
        }

        public static void Exit()
        {
            if (!s_initialized) return;

            s_initialized = false;

            ContextClose(s_contextCurrent);

            while (s_contextStack.Count > 0) ContextClose(s_contextStack.Pop());

            Application.logMessageReceivedThreaded -= UnityLogCallback;

            foreach (ContextInternal context in s_contextSingleFileArray)
            {
                if (context != null) ContextClose(context, true);
            }

            s_contextCurrent = null;

            s_contextStack = null;
            s_contextSingleFileArray = null;
        }

        public static void ContextEnter(ContextID contextID)
        {
            if (!s_initialized) return;

            s_contextStack.Push(s_contextCurrent);
            s_contextCurrent = ContextOpen(contextID);
        }

        public static void ContextSwitch(ContextID contextID)
        {
            if (!s_initialized) return;

            ContextClose(s_contextCurrent);
            s_contextCurrent = ContextOpen(contextID);
        }

        public static void ContextExit()
        {
            if (!s_initialized) return;
            if (s_contextStack.Count <= 0) return;

            ContextClose(s_contextCurrent);
            s_contextCurrent = s_contextStack.Pop();
        }

        public static void FileOpen()
        {
            if (!s_initialized  ) return;
            if (s_contextCurrent.Public.FileOpen) return;

            FileOpen(s_contextCurrent);
        }

        public static void FileOpen(params object[] details)
        {
            if (!s_initialized) return;
            if (s_contextCurrent.Public.FileOpen) return;

            FileOpen(s_contextCurrent, string.Format(s_contextCurrent.Public.Info.LogDetailsFormat, details));
        }

        #endregion Public - Static Methods

        #endregion Public

        #region Private

        #region Private - Types

        private class ContextInternal
        {
            public Context Public {  get; private set; }
            public List<string> TempLogMsgList { get; }
            public File File { get; private set; }

            public ContextInternal(ContextTemplate template)
            {
                Public = new Context(template, false);
                TempLogMsgList = new List<string>();
                File = null;
            }

            public void SetFile(File file)
            {
                File = file;
                Public = new Context(Public.Info, true);
            }
        }

        private class File
        {
            public readonly StreamWriter Writer;
            public readonly string Path;

            public static File Open(string path)
            {
                StreamWriter writer;

                // Open for overwrite!
                try { writer = new StreamWriter(path, false, s_fileEncoding) { AutoFlush = true }; }
                catch (IOException) { return null; }

                return new File(writer, path);
            }

            private File(StreamWriter writer, string path)
            {
                Writer = writer;
                Path = path;
            }
        }

        #endregion Private - Types

        #region Private - Static Flields

        private static bool s_initialized;

        // Context
        private static ContextInternal s_contextCurrent = null;
        private static Stack<ContextInternal> s_contextStack = null;
        private static ContextInternal[] s_contextSingleFileArray = null;

        #endregion Private - Static Flields

        #region Private - Static Methods

        private static ContextInternal ContextOpen(ContextID contextID)
        {
            ContextTemplate template = s_contextTemplateArray[(int)contextID];
            ContextInternal context;

            if (template.SingleFile)
            {
                context = s_contextSingleFileArray[(int)contextID];
                if (context != null) return context;
            }

            context = new ContextInternal(template);

            if (template.SingleFile) s_contextSingleFileArray[(int)contextID] = context;

            return context;
        }

        private static void ContextClose(ContextInternal context, bool singleFileOverride = false)
        {
            if (context.Public.Info.SingleFile && !singleFileOverride) return;
            if (context.Public.FileOpen) FileClose(context.File);
        }

        private static void FileOpen(ContextInternal context, string details = null)
        {
            File file;

            string fileName = details != null ?
                string.Format(s_fileNameFormatWithDetails,    context.Public.Info.LogName, DateTime.UtcNow, details) :
                string.Format(s_fileNameFormatWithoutDetails, context.Public.Info.LogName, DateTime.UtcNow);
            string basePath = Path.Combine(Application.persistentDataPath, fileName);

            string path;
            int suffix = 0;

            for (;;)
            {
                path = string.Format(basePath, suffix);

                file = File.Open(path);
                if (file != null) break;

                suffix++;
                if (suffix > s_fileSuffixMax) return;
            }

            context.SetFile(file);

            foreach (string logMsg in context.TempLogMsgList) file.Writer.WriteLine(logMsg);
            context.TempLogMsgList.Clear();
        }

        private static void FileClose(File file)
        {
            file.Writer.Close();
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

                for (int i = 0; ;)
                {
                    if (i >= logString.Length)
                    {
                        if (startIndex < i) stringBuilder.Append(logString, startIndex, i - startIndex);
                        break;
                    }

                    switch (logString[i])
                    {
                        case '\\': replacementString = "\\\\"; goto Replace;
                        case '\n': replacementString = "\\n";  goto Replace;
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
#if !DEVELOPMENT_BUILD || !UNITY_EDITOR
                if (type is LogType.Log) break;
#endif
                logString = stackTrace;
                repeat = false;
            }

            if (s_contextCurrent.Public.FileOpen) s_contextCurrent.File.Writer.WriteLine(stringBuilder.ToString());
            else s_contextCurrent.TempLogMsgList.Add(stringBuilder.ToString());
        }

        #endregion Private - Static Methods

        #endregion Private
    }
}
