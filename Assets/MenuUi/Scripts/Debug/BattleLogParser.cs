using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal static class BattleLogParser
    {
        public static IReadOnlyMsgStorage ParseLogs(string[][] logs)
        {
            MsgStorage msgStorage = new(logs.Length);

            for (int i = 0; i < logs.Length; i++)
            {
                string[] lines = logs[i];
                ParseLog(msgStorage, i, lines);
            }

            return msgStorage;
        }

        private static void ParseLog(MsgStorage msgStorage, int client, string[] lines)
        {
            // msg variables
            int time = 0;
            string msg;
            string source;
            string trace;
            MessageType type;

            Match match;

            // line parsing output variables
            string[] msgParts = new string[3];
            int msgPart;

            // line parsing variables
            StringBuilder stringBuilder = new();
            string line;
            bool endOfLine;
            int startIndex;
            int endIndex;
            char replacementChar;

            for (int lineI = 0; lineI < lines.Length; lineI++)
            {
                // set parsing variables
                msgPart = 0;
                line = lines[lineI];
                endOfLine = false;
                startIndex = 0;

                // parse line
                for (int charI = 0; !endOfLine;)
                {
                    endOfLine = charI >= line.Length;

                    if (endOfLine) { endIndex = charI; goto AddPart; }

                    // check for escape character
                    if (line[charI] == '\\')
                    {
                        endIndex = charI;

                        // check next character
                        charI++;
                        switch (line[charI])
                        {
                            // replacement cases
                            case '\\' : replacementChar = '\\'; break;
                            case 'n'  : replacementChar = '\n'; break;

                            // end of part case
                            case ',':
                                if (msgPart >= msgParts.Length)
                                {
                                    Debug.LogError(string.Format("BattleLogParser Error: Too many msg part at line: {0}, col: {1}", lineI, charI));
                                    endOfLine = true;
                                }
                                charI++;
                                goto AddPart;

                            default:
                                Debug.LogError(string.Format("BattleLogParser Error: Invalid escape sequence \"\\{0}\" at line: {1}, col: {2}", line[charI], lineI, charI));
                                continue;
                        }

                        // replace
                        if (startIndex < endIndex) stringBuilder.Append(line, startIndex, endIndex - startIndex);
                        stringBuilder.Append(replacementChar);
                        charI++;
                        startIndex = charI;
                        continue;
                    }

                    charI++;
                    continue;

                AddPart:
                    if (startIndex < endIndex) stringBuilder.Append(line, startIndex, endIndex - startIndex);
                    startIndex = charI;
                    msgParts[msgPart] = stringBuilder.ToString();
                    stringBuilder.Length = 0;
                    msgPart++;
                }

                if (msgPart < msgParts.Length)
                {
                    Debug.LogError(string.Format("BattleLogParser Error: Too few msg part at line: {0}", lineI));
                    continue;
                }

                msg = msgParts[0];

                // extract time from msg if possible else use previous time
                // extract source from msg if possible else use SOURCE UNKNOW
                source = "SOURCE UNKNOW";
                match = s_battleMsgFormat.Match(msg);
                if (match.Success)
                {
                    time = int.Parse(match.Groups[1].Value);
                    source = match.Groups[2].Value;
                    msg = match.Groups[3].Value;
                }

                msg = string.Format("[{0}] {1}", source, msg);

                switch (msgParts[1])
                {
                    case "Log"     : type = MessageType.Info   ; break;
                    case "Warning" : type = MessageType.Warning; break;

                    case "Error":
                    case "Assert":
                    case "Exception":
                        type = MessageType.Error; break;

                    default:
                        Debug.LogError(string.Format("BattleLogParser Error: Invalid msg type \"{0}\" at line: {1}", msgParts[1], lineI));
                        type = MessageType.Error;
                        break;
                }

                trace = msgParts[2];

                msgStorage.Add(new MsgObject(client, time, msg, msgStorage.GetSourceFlagOrNew(source), trace, type));
            }
        }

        private static void CompareLogs()
        {

        }

        private static readonly Regex s_battleMsgFormat = new(@"^\[([0-9]+)\] \[BATTLE\] \[([^\]]+)\] (.*)$", RegexOptions.Singleline);
    }
}
