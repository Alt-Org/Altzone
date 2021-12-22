using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Helper OnGUI window to show some Photon related info as "overlay" window.
    /// </summary>
    public class PhotonStatsWindow : MonoBehaviour
    {
        public bool Visible;
        public KeyCode controlKey = KeyCode.F2;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private int WindowId;
        private Rect WindowRect;
        private string WindowTitle;
        private bool hasStyles;
        private GUIStyle guiButtonStyle;
        private GUIStyle guiLabelStyle;

        private void OnEnable()
        {
            WindowId = (int)DateTime.Now.Ticks;
            WindowRect = new Rect(0, 0, Screen.width, Screen.height);
            WindowTitle = $"({controlKey}) Photon";
        }

        private void Update()
        {
            if (Input.GetKeyDown(controlKey))
            {
                toggleWindowState();
            }
        }

        private void toggleWindowState()
        {
            Visible = !Visible;
        }

        private void OnGUI()
        {
            if (!Visible)
            {
                return;
            }
            if (!hasStyles)
            {
                hasStyles = true;
                guiButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
                guiLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            }
            WindowRect = GUILayout.Window(WindowId, WindowRect, DebugWindow, WindowTitle);
        }

        private void DebugWindow(int windowId)
        {
            string label;
            var inRoom = PhotonNetwork.InRoom;
            if (inRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                label = $"{PhotonNetwork.LocalPlayer.NickName} | {room.Name}" +
                        $"{(room.IsVisible ? string.Empty : ",hidden")}" +
                        $"{(room.IsOpen ? string.Empty : ",closed")} " +
                        $"{(room.PlayerCount == 1 ? "1 player" : $"{room.PlayerCount} players")}" +
                        $"{(room.MaxPlayers == 0 ? string.Empty : $" (max {room.MaxPlayers})")}";
            }
            else if (PhotonNetwork.InLobby)
            {
                label = $"Lobby: rooms {PhotonNetwork.CountOfRooms}, players {PhotonNetwork.CountOfPlayers}";
            }
            else
            {
                label = $"Photon: {PhotonNetwork.NetworkClientState}";
            }
            if (GUILayout.Button(label, guiButtonStyle))
            {
                toggleWindowState();
            }
            if (inRoom)
            {
                label = "Props:";
                var room = PhotonNetwork.CurrentRoom;
                var props = room.CustomProperties;
                var keys = props.Keys.ToList();
                keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                foreach (var key in keys)
                {
                    if ("curScn".Equals(key))
                    {
                        // Skip current scene name.
                        continue;
                    }
                    var propValue = props[key];
                    label += $"\r\n{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                }
                label += "\r\nPlayers:";
                foreach (var player in room.GetPlayersByActorNumber())
                {
                    var text = player.GetDebugLabel(verbose: false);
                    label += $"\r\n{text}";
                    props = player.CustomProperties;
                    if (props.Count > 0)
                    {
                        keys = props.Keys.ToList();
                        keys.Sort((a, b) => string.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
                        foreach (var key in keys)
                        {
                            var propValue = props[key];
                            label += $"\r\n{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                        }
                    }
                }
            }
            label += $"\r\nPhoton v='{PhotonLobby.GameVersion}'";
            label += $"\r\nSend rate={PhotonNetwork.SendRate} ser rate={PhotonNetwork.SerializationRate}";
            GUILayout.Label(label, guiLabelStyle);
        }

        private static readonly Dictionary<Type, string> TypeMap = new Dictionary<Type, string>()
        {
            // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types
            { typeof(bool), "bool" },
            { typeof(byte), "byte" },
            { typeof(sbyte), "sbyte" },
            { typeof(char), "char" },
            { typeof(decimal), "decimal" },
            { typeof(double), "double" },
            { typeof(float), "float" },
            { typeof(int), "int" },
            { typeof(uint), "uint" },
            { typeof(long), "long" },
            { typeof(ulong), "ulong" },
            { typeof(short), "short" },
            { typeof(ushort), "ushort" },
            { typeof(string), "str" },
        };

        private static string ShortTypeName(Type type)
        {
            return TypeMap.TryGetValue(type, out var name) ? name : type.Name;
        }

        /// <summary>
        /// Ring buffer for average lag compensation calculation, not exactly exact!
        /// </summary>
        /// <remarks>
        /// Values are updated once per buffer fill and can be seen in Editor.
        /// </remarks>
        [Serializable]
        public class LagCompensation
        {
            public string status;
            public double samplingStart;
            public double samplingDuration;
            public int sampleCount;
            public int sampleIndexCur;
            public int sampleIndexMax;
            public float[] samples;

            public LagCompensation(int sampleCount)
            {
                this.sampleCount = sampleCount;
                sampleIndexMax = sampleCount - 1;
                samples = new float[sampleCount];
                reset();
            }

            public void reset()
            {
                status = string.Empty;
                samplingStart = 0;
                samplingDuration = 0;
                sampleIndexCur = 0;
            }

            public void addSample(float lagValue)
            {
                samples[sampleIndexCur] = lagValue;
                if (sampleIndexCur == 0)
                {
                    samplingStart = Time.time;
                    sampleIndexCur += 1;
                }
                else if (sampleIndexCur == sampleIndexMax)
                {
                    samplingDuration = Time.time - samplingStart;
                    sampleIndexCur = 0;
                    status = ToString();
                }
                else
                {
                    sampleIndexCur += 1;
                }
            }

            public override string ToString()
            {
                var sum = 0f;
                for (var i = 0; i < sampleCount; ++i)
                {
                    sum += samples[i];
                }
                return $"avg lag {sum / sampleCount:0.000} s ({sampleCount}) : {sampleCount / samplingDuration: 0.0} msg/s";
            }
        }
#endif
    }
}