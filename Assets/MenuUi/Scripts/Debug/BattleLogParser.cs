using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    interface IReadOnlyBattleLogParserStatus
    {
        public string Msg { get; }
        public int Progress { get; }

        public IReadOnlyMsgStorage GetResult();
    }

    class BattleLogParserStatus : IReadOnlyBattleLogParserStatus
    {
        public string Msg { get; private set; }
        public int Progress { get; private set; }
        public int StepCount { get; private set;  }

        public void InitInstance(int stepCount, Thread thread, IReadOnlyMsgStorage msgStorage)
        {
            StepCount = stepCount;
            _thread = thread;
            _msgStorage = msgStorage;
        }

        public void SetMsg(string msg) { Msg = msg; }
        public void UpdateProgress() { Progress++; }

        public IReadOnlyMsgStorage GetResult()
        {
            if (_thread.IsAlive) return null;
            return _msgStorage;
        }

        private Thread _thread;
        private IReadOnlyMsgStorage _msgStorage;
    }

    internal static class BattleLogParser
    {
        public static IReadOnlyBattleLogParserStatus ParseLogs(string[][] logs)
        {
            BattleLogParserStatus battleLogParserStatus = new();
            MsgStorage msgStorage = new(logs.Length);

            Thread thread = new(new ThreadStart(() =>
            {
                battleLogParserStatus.SetMsg("Parsing logs");
                for (int i = 0; i < logs.Length; i++)
                {
                    string[] lines = logs[i];
                    ParseLog(msgStorage, i, lines);
                    battleLogParserStatus.UpdateProgress();
                }

                battleLogParserStatus.SetMsg("Comparing logs");
                if (msgStorage.ClientCount > 1) CompareLogs(msgStorage);
                battleLogParserStatus.UpdateProgress();
            }));

            battleLogParserStatus.InitInstance(logs.Length + 1, thread, msgStorage);
            thread.Start();

            return battleLogParserStatus;
        }

        private class CompareData
        {
            public MatchMatrix MatchMatrix { get; private set; }
            public List<MatchGroup> AllMatchGroups { get; }
            public int[] ColorGroupArray { get; }
            public int ColorGroupFullMatch { get; }

            public CompareData(MsgStorage msgStorage)
            {
                _msgStorage = msgStorage;
                _timeArray = _msgStorage.GetTimes();
                _timeIndex = -1;

                _msgObjectLists = new IReadOnlyList<MsgObject>[msgStorage.ClientCount];

                AllMatchGroups = new();
                List<int> colorSuperGroupList;
                (ColorGroupArray, colorSuperGroupList) = GenerateColorGroupArray(msgStorage.ClientCount);
                ColorGroupFullMatch = ColorGroupArray[ColorGroupArray.Length - 1];

                _msgStorage.SetColorSuperGroupList(colorSuperGroupList);
            }

            public bool CompareNextTime()
            {
                _timeIndex++;
                if (_timeIndex >= _timeArray.Length) return false;
                int time = _timeArray[_timeIndex];
                Debug.Log(string.Format("BattleLogParser Info: Comparing timestamp {0}", time));

                int length = 0;

                for (int i = 0; i < _msgStorage.ClientCount; i++)
                {
                    _msgObjectLists[i] = _msgStorage.GetMutableTime(i, time);
                    if (_msgObjectLists[i] == null) _msgObjectLists[i] = new List<MsgObject>();
                    length = Math.Max(length, _msgObjectLists[i].Count);
                }

                MatchMatrix = new MatchMatrix(_msgStorage.ClientCount, length, _msgObjectLists);
                AllMatchGroups.Clear();

                return true;
            }

            private readonly MsgStorage _msgStorage;
            private readonly int[] _timeArray;
            private int _timeIndex;

            private readonly IReadOnlyList<MsgObject>[] _msgObjectLists;
        }

        private class MatchMatrix
        {
            public int ClientCount { get; }
            public int Length { get; }

            public MatchMatrix(int clientCount, int length, IReadOnlyList<MsgObject>[] msgObjectLists)
            {
                ClientCount = clientCount;
                Length = length;

                _array = new MatchObject[clientCount * length];

                IReadOnlyList<MsgObject> msgObjectList;
                MsgObject msgObject;
                int client;
                int index;
                int i;

                for (client = 0; client < clientCount; client++)
                {
                    msgObjectList = msgObjectLists[client];

                    for (index = 0; index < length; index++)
                    {
                        i = client * length + index;

                        if (index < msgObjectList.Count) msgObject = msgObjectList[index];
                        else msgObject = null;

                        _array[i] = new MatchObject(msgObject, client, index, length);
                    }
                }
            }

            public MatchObject GetMatchObject(int client, int index) { return _array[client * Length + index]; }

            public readonly MatchObject[] _array;
        }

        private class MatchObject
        {
            public int Client { get; }
            public int Index { get; }
            public MsgObject MsgObject { get; }
            public MatchGroup MatchGroup { get; set; }

            public static bool Compare(MatchObject matchObjectA, MatchObject matchObjectB, MatchGroup matchGroup)
            {
                Debug.Log(string.Format("BattleLogParser Info: Comparing ({0}, {1}), ({2}, {3})", matchObjectA.Client, matchObjectA.Index, matchObjectB.Client, matchObjectB.Index));
                matchObjectA._checkArray[matchObjectB.Index] |= 1 << matchObjectB.Client;
                matchObjectB._checkArray[matchObjectA.Index] |= 1 << matchObjectA.Client;

                MsgObject msgA = matchObjectA.MsgObject;
                MsgObject msgB = matchObjectB.MsgObject;

                (float matchScore, int[] matchStringArrayA, int[] matchStringArrayB) = CompareMsgs(msgA, msgB);
                if (matchScore > s_matchScoreThreshold)
                {
                    matchGroup.AddMatch(matchObjectA, matchObjectB, matchStringArrayA, matchStringArrayB);
                    return true;
                }

                return false;
            }

            public MatchObject(MsgObject msgObject, int client, int index, int matchMatrixLength)
            {
                MsgObject = msgObject;
                Client = client;
                Index = index;
                MatchGroup = null;
                _checkArray = new int[matchMatrixLength];
            }

            public bool IsComparedTo(MatchObject matchObject)
            {
                return (_checkArray[matchObject.Index] & (1 << matchObject.Client)) > 0;
            }

            public void DebugSheck()
            {
                /*
                if (Client == 4 && Index == 4)
                {
                    var t = 1;
                }
                /**/
            }

            private readonly int[] _checkArray;
        }

        private class MatchGroup
        {
            public int[] IndexArray { get; }
            public int MatchFlags { get; private set; }
            public int[][] MatchStringArrays { get; }

            public MatchGroup(int clientCount)
            {
                IndexArray = new int[clientCount];
                MatchFlags = 0;
                MatchStringArrays = new int[clientCount][];

                Array.Fill(IndexArray, -1);
                Array.Fill(MatchStringArrays, null);
            }

            public bool HasMatch(int client) { return IndexArray[client] >= 0; }

            public void AddMatch(MatchObject matchObjectA, MatchObject matchObjectB, int[] matchStringArrayA, int[] matchStringArrayB)
            {
                Debug.Log(string.Format("BattleLogParser Info: Match ({0}, {1}), ({2}, {3})", matchObjectA.Client, matchObjectA.Index, matchObjectB.Client, matchObjectB.Index));

                MatchFlags |= 1 << matchObjectA.Client;
                MatchFlags |= 1 << matchObjectB.Client;

                if (IndexArray[matchObjectA.Client] < 0)
                {
                    MatchStringArrays[matchObjectA.Client] = new int[matchStringArrayA.Length];
                    for (int i = 0; i < matchStringArrayA.Length; i++) MatchStringArrays[matchObjectA.Client][i] = (1 << matchObjectA.Client);
                }
                for (int i = 0; i < matchStringArrayA.Length; i++) MatchStringArrays[matchObjectA.Client][i] |= (matchStringArrayA[i] << matchObjectB.Client);

                if (IndexArray[matchObjectB.Client] < 0)
                {
                    MatchStringArrays[matchObjectB.Client] = new int[matchStringArrayB.Length];
                    for (int i = 0; i < matchStringArrayB.Length; i++) MatchStringArrays[matchObjectB.Client][i] = (1 << matchObjectB.Client);
                }
                for (int i = 0; i < matchStringArrayB.Length; i++) MatchStringArrays[matchObjectB.Client][i] |= (matchStringArrayB[i] << matchObjectA.Client);

                IndexArray[matchObjectA.Client] = matchObjectA.Index;
                IndexArray[matchObjectB.Client] = matchObjectB.Index;
            }
        }

        private static readonly Regex s_battleMsgFormat = new(@"^\[([0-9]+)\] \[BATTLE\] \[([^\]]+)\] (.*)$", RegexOptions.Singleline);
        private static readonly float s_matchScoreThreshold = 0.5f;

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
                                    Debug.LogError(string.Format("BattleLogParser Error: Too many msg parts at line: {0}, col: {1}", lineI, charI));
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
                    Debug.LogError(string.Format("BattleLogParser Error: Too few msg parts at line: {0}", lineI));
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

        private static (int[], List<int>) GenerateColorGroupArray(int clientCount)
        {
            int[] colorGroupArray = new int[1 << clientCount];
            List<int> colorSuperGroupList = new();
            int group = 0;
            int superGroupSize = 1;

            int flags;
            int flagsEnd;

            bool increment;

            int[] indexArray = new int[clientCount];
            int i1;
            int i2a;
            int i2b;

            for (i1 = 0; i1 < clientCount; i1++)
            {
                flagsEnd = (1 << (i1 + 1)) - 1;
                for (i2a = 0; i2a < i1 + 1; i2a++) indexArray[i2a] = i2a;

                for (;;)
                {
                    flags = 0;
                    increment = true;

                    for (i2a = 0; ;)
                    {
                        flags |= 1 << (clientCount - indexArray[i2a] - 1);

                        if (increment) indexArray[i2a]++;
                        increment = false;

                        i2b = i2a + 1;
                        if (i2b >= i1 + 1) break;

                        if (indexArray[i2a] == indexArray[i2b])
                        {
                            increment = true;
                            indexArray[i2a] = i2a;
                        }

                        i2a = i2b;
                    }

                    if (i1 > 0) { group++; superGroupSize++; }
                    colorGroupArray[flags] = group;
                    if (flags == flagsEnd) break;
                }

                colorSuperGroupList.Add(superGroupSize);
                superGroupSize = 0;
            }

            colorGroupArray[0] = -1;

            return (colorGroupArray, colorSuperGroupList);
        }

        private static void CompareLogs(MsgStorage msgStorage)
        {
            CompareData compareData = new(msgStorage);

            while (compareData.CompareNextTime())
            {
                CompareTime(compareData);
                CompareTimeSaveResults(compareData);
            }
        }

        private static void CompareTime(CompareData compareData)
        {
            MatchMatrix matchMatrix = compareData.MatchMatrix;

            int indexA;
            int indexB;

            int clientA;
            int clientB;

            MatchObject matchObjectA;
            MatchObject matchObjectB;

            bool match;

            int clientTemp;
            MatchObject matchObjectTemp;

            MatchGroup newMatchGroup = new(matchMatrix.ClientCount);

            for (indexA = 0; indexA < matchMatrix.Length; indexA++)
            {
                for (clientA = 0; clientA < matchMatrix.ClientCount; clientA++)
                {
                    match = false;

                    matchObjectA = matchMatrix.GetMatchObject(clientA, indexA);
                    matchObjectA.DebugSheck();

                    if (matchObjectA.MsgObject == null) continue;
                    if (matchObjectA.MatchGroup != null) continue;

                    for (clientB = 0; clientB < matchMatrix.ClientCount; clientB++)
                    {
                        if (clientA == clientB) continue;

                        for (indexB = indexA; indexB >= 0; indexB--)
                        {
                            matchObjectB = matchMatrix.GetMatchObject(clientB, indexB);
                            matchObjectB.DebugSheck();

                            if (matchObjectB.MsgObject == null) continue;

                            if (matchObjectB.IsComparedTo(matchObjectA)) continue;

                            if (matchObjectB.MatchGroup != null)
                            {
                                if (matchObjectB.MatchGroup.HasMatch(clientA)) break;

                                for (clientTemp = 0; clientTemp < matchMatrix.ClientCount; clientTemp++)
                                {
                                    if (clientA == clientTemp) continue;
                                    if (!matchObjectB.MatchGroup.HasMatch(clientTemp)) continue;
                                    matchObjectTemp = matchMatrix.GetMatchObject(clientTemp, matchObjectB.MatchGroup.IndexArray[clientTemp]);
                                    matchObjectTemp.DebugSheck();

                                    if (MatchObject.Compare(matchObjectA, matchObjectTemp, matchObjectB.MatchGroup)) match = true;
                                }

                                matchObjectA.DebugSheck();
                                matchObjectB.DebugSheck();
                                if (match) matchObjectA.MatchGroup = matchObjectB.MatchGroup;

                                break;
                            }

                            if (MatchObject.Compare(matchObjectA, matchObjectB, newMatchGroup))
                            {
                                matchObjectA.DebugSheck();
                                matchObjectB.DebugSheck();
                                compareData.AllMatchGroups.Add(newMatchGroup);
                                matchObjectA.MatchGroup = newMatchGroup;
                                matchObjectB.MatchGroup = newMatchGroup;
                                newMatchGroup = new(matchMatrix.ClientCount);
                                match = true;
                                break;
                            }
                        }

                        if (match) break;
                    }

                    if (match) break;
                }
            }
        }

        private static void CompareTimeSaveResults(CompareData compareData)
        {
            MatchMatrix matchMatrix = compareData.MatchMatrix;

            MatchObject matchObject;
            MsgObject msgObject;
            MatchGroup matchGroup;

            int index;
            int client;

            int i;

            for (i = 0; i < compareData.AllMatchGroups.Count; i++)
            {
                matchGroup = compareData.AllMatchGroups[i];

                List<MsgObject> matchList = new();

                for (client = 0; client < matchMatrix.ClientCount; client++)
                {
                    if (!matchGroup.HasMatch(client)) continue;

                    index = matchGroup.IndexArray[client];
                    msgObject = matchMatrix.GetMatchObject(client, index).MsgObject;
                    msgObject.SetMatchList(matchList);
                    matchList.Add(msgObject);
                }
            }

            int colorGroup;
            int[] matchStringArray;
            List<Tuple<int, int, int>> stringColorGroupList;
            int stringColorGroup;
            int stringColorGroupPrev;
            int stringColorGroupStart;
            int stringColorGroupLength;

            bool endOfString;

            for (index = 0; index < matchMatrix.Length; index++)
            {
                for (client = 0; client < matchMatrix.ClientCount; client++)
                {
                    matchObject = matchMatrix.GetMatchObject(client, index);
                    msgObject = matchObject.MsgObject;
                    if (msgObject == null) continue;
                    matchGroup = matchObject.MatchGroup;

                    colorGroup = 0;
                    stringColorGroupList = null;
                    stringColorGroupPrev = -1;
                    stringColorGroupStart = -1;
                    stringColorGroupLength = -1;

                    if (matchGroup != null)
                    {
                        colorGroup = 4; /*compareData.ColorGroupArray[matchGroup.MatchFlags];*/

                        stringColorGroupList = new();
                        matchStringArray = matchGroup.MatchStringArrays[matchObject.Client];
                        endOfString = false;

                        for (i = 0;; i++)
                        {
                            if (i < matchStringArray.Length) stringColorGroup = compareData.ColorGroupArray[matchStringArray[i]];
                            else { stringColorGroup = -1; endOfString = true; }

                            if (stringColorGroup != stringColorGroupPrev)
                            {
                                if (stringColorGroupStart >= 0)
                                    stringColorGroupList.Add(new (stringColorGroupStart, stringColorGroupLength, stringColorGroupPrev));

                                if (endOfString) break;

                                stringColorGroupPrev = stringColorGroup;
                                stringColorGroupStart = i;
                                stringColorGroupLength = 1;
                            }
                            else stringColorGroupLength++;
                        }
                    }

                    msgObject.SetColorGroup(colorGroup, compareData.ColorGroupFullMatch);
                    msgObject.SetStringColorGroupList(stringColorGroupList);
                }
            }
        }

        private static (float matchScore, int[] matchStringArrayA, int[] matchStringArrayB) CompareMsgs(MsgObject msgA, MsgObject msgB)
        {
            if (msgA.SourceFlag != msgB.SourceFlag) return (0.0f, null, null);

            string stringA = msgA.Msg;
            string stringB = msgB.Msg;

            int matchCount = 0;
            int[] matchStringArrayA = new int[stringA.Length];
            int[] matchStringArrayB = new int[stringB.Length];

            int iA = 0;
            int iB = 0;

            int diffStartA = -1;
            int diffStartB = -1;
            int diffEndA = -1;
            int diffEndB = -1;

            for (;;)
            {
                if (diffStartA >= 0)
                {
                    if (diffEndA >= stringA.Length || diffEndB >= stringB.Length) break;

                    if (stringA[iA] == stringB[diffEndB])
                    {
                        iB = diffEndB;
                        diffStartA = -1;
                        continue;
                    }

                    if (stringA[diffEndA] == stringB[iB])
                    {
                        iA = diffEndA;
                        diffStartA = -1;
                        continue;
                    }

                    if (iA == diffEndA)
                    {
                        iA = diffStartA;
                        iB = diffStartB;
                        diffEndA++;
                        diffEndB++;
                        continue;
                    }

                    iA++;
                    iB++;
                    continue;
                }

                if (stringA[iA] == stringB[iB])
                {
                    matchStringArrayA[iA] = 1;
                    matchStringArrayB[iB] = 1;
                    matchCount++;
                }
                else
                {
                    diffStartA = iA;
                    diffStartB = iB;
                    diffEndA = iA + 1;
                    diffEndB = iB + 1;
                    continue;
                }

                iA++;
                iB++;

                if (iA >= stringA.Length || iB >= stringB.Length) break;
            }

            float matchScore = (float)(matchCount * 2) / (stringA.Length + stringB.Length);

            return (matchScore, matchStringArrayA, matchStringArrayB);
        }
    }
}
