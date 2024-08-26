using System.Text.RegularExpressions;

namespace DebugUi.Scripts.BattleAnalyzer
{
    internal static class BattleLogParser
    {
        public static IReadOnlyMsgStorage ParseLog(string[] lines)
        {
            MsgStorage msgStorage = new(1);

            int i = 0;

            // find start and set i to first battle msg line or return if not found
            while (true)
            {
                if (i >= lines.Length) return msgStorage;
                if (lines[i] == "[[BATTLE LOG START]]") break;
                i++;
            }

            // msg variables
            int time = 0;
            string msg;
            string trace;
            MessageType type;

            Match match;

            // flow control variables
            bool loop = true;
            bool skipMsg;

            // loop through rest of the lines starting at i
            do
            {
                // set to default
                // use previous time as default
                msg = "";
                trace = "";
                type = MessageType.Info;

                // get msg lines and set loop to exit if end of lines
                for (;;)
                {
                    if (i >= lines.Length) { loop = false; break; }
                    if (lines[i].Length <= 0) { i++; break; }
                    msg += lines[i] + "\n";
                    i++;
                }

                // skip msg if empty or matches a filter patter
                if (msg.Length <= 0) continue;
                skipMsg = false;
                foreach (Regex filter in s_filterPatters)
                {
                    if (filter.Match(msg).Success) { skipMsg = true; break; }
                }
                if (skipMsg) continue;

                // extract time from msg if possible else use previous time
                match = s_battleMsgFormat.Match(msg);
                if (match.Success)
                {
                    time = int.Parse(match.Groups[1].Value);
                    msg = match.Groups[2].Value;
                }

                // split msg to msg and trace if possible else leave msg as is and trace empty
                match = s_unityMsgFormat.Match(msg);
                if (match.Success)
                {
                    msg = match.Groups[1].Value;
                    trace = match.Groups[2].Value;

                    // get msg type from trace if possible or leave type as info
                    match = s_msgTypePattern.Match(trace);
                    if (match.Success)
                    {
                        switch (match.Groups[1].Value)
                        {
                            // skip checking for info since it's default
                            case "Warning" : type = MessageType.Warning; break;
                            case "Error"   : type = MessageType.Error; break;
                        }
                    }
                }

                msgStorage.Add(new MsgObject(0, time, msg, trace, type));
            } while (loop);

            // add test msg containing number of total messages
            msgStorage.Add(new MsgObject(0, 999999, msgStorage.TotalMessages().ToString(), "", MessageType.Info));
            return msgStorage;
        }

        private static readonly Regex[] s_filterPatters =
        {
            new(@"^\(Filename: .* Line: .*\)$")
        };

        private static readonly Regex s_battleMsgFormat = new(@"^\[([0-9]+)\] (\[BATTLE\] .*)$", RegexOptions.Singleline);
        private static readonly Regex s_unityMsgFormat  = new(@"^(.*?)\s*\n(UnityEngine\.StackTraceUtility:ExtractStackTrace \(\).*?)\s*$", RegexOptions.Singleline);
        private static readonly Regex s_msgTypePattern  = new(@"^UnityEngine\.Debug:Log(.*) \(.*\)$", RegexOptions.Multiline);
    }
}
