using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Test
{
    /// <summary>
    /// Helper OnGUI window to show some Photon related info as "overlay" window.
    /// </summary>
    public class PhotonStatsWindow : MonoBehaviour
    {
        public bool _visible;
        public Key _controlKey = Key.F2;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private int _windowId;
        private Rect _windowRect;
        private string _windowTitle;
        private bool _hasStyles;
        private GUIStyle _guiButtonStyle;
        private GUIStyle _guiLabelStyle;

        private void OnEnable()
        {
            Assert.IsTrue(FindObjectsOfType<PhotonStatsWindow>().Length == 1, "FindObjectsOfType<PhotonStatsWindow>().Length == 1");
            _windowId = (int)DateTime.Now.Ticks;
            _windowRect = new Rect(0, 0, Screen.width, Screen.height);
            _windowTitle = $"({_controlKey}) Photon";
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame)
            {
                ToggleWindowState();
            }
        }

        private void ToggleWindowState()
        {
            _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible)
            {
                return;
            }
            if (!_hasStyles)
            {
                _hasStyles = true;
                _guiButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
                _guiLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 24 };
            }
            _windowRect = GUILayout.Window(_windowId, _windowRect, DebugWindow, _windowTitle);
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
            if (GUILayout.Button(label, _guiButtonStyle))
            {
                ToggleWindowState();
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
            if (PhotonNetwork.OfflineMode || PhotonNetwork.AutomaticallySyncScene)
            {
                label += $"\r\n";
                if (PhotonNetwork.OfflineMode)
                {
                    label += $"OfflineMode ";
                }
                if (PhotonNetwork.AutomaticallySyncScene)
                {
                    label += $"AutomaticallySyncScene ";
                }
            }
            GUILayout.Label(label, _guiLabelStyle);
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
#if false
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
#endif
    }
}