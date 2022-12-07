using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Scripts.DevUtil
{
    /// <summary>
    /// Helper OnGUI window to show some Photon related info as "overlay" window.
    /// </summary>
    internal class PhotonStatsWindow : MonoBehaviour
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
            var type = GetType();
            Assert.IsTrue(FindObjectsOfType(type).Length == 1, $"FindObjectsOfType({type}).Length == 1");
            _windowId = type.GetHashCode();
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
            string title;
            var inRoom = PhotonNetwork.InRoom;
            if (inRoom)
            {
                var room = PhotonNetwork.CurrentRoom;
                title = $"{PhotonNetwork.LocalPlayer.NickName} | {room.Name}" +
                        $"{(room.IsVisible ? string.Empty : ",hidden")}" +
                        $"{(room.IsOpen ? string.Empty : ",closed")} " +
                        $"{(room.PlayerCount == 1 ? "1 player" : $"{room.PlayerCount} players")}" +
                        $"{(room.MaxPlayers == 0 ? string.Empty : $" (max {room.MaxPlayers})")}";
            }
            else if (PhotonNetwork.InLobby)
            {
                title = $"Lobby: rooms {PhotonNetwork.CountOfRooms}, players {PhotonNetwork.CountOfPlayers}";
            }
            else
            {
                title = $"Photon: {PhotonNetwork.NetworkClientState}";
            }
            if (GUILayout.Button(title, _guiButtonStyle))
            {
                ToggleWindowState();
                return;
            }
            var space = "  ";
            var label = $"v={PhotonLobby.GameVersion} r={PhotonNetwork.CloudRegion} ping={PhotonNetwork.GetPing()}";
            if (inRoom)
            {
                label += $"\r\n--Room--";
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
                    label += $"\r\n{space}{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                }
                label += "\r\n--Players--";
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
                            label += $"\r\n{space}{key}={propValue} [{ShortTypeName(propValue.GetType())}]";
                        }
                    }
                }
            }
            label += $"\r\nsend rate={PhotonNetwork.SendRate} ser rate={PhotonNetwork.SerializationRate}";
            if (PhotonNetwork.OfflineMode || PhotonNetwork.AutomaticallySyncScene)
            {
                label += $"\r\n";
                if (PhotonNetwork.OfflineMode)
                {
                    label += $"OfflineMode ";
                }
                if (PhotonNetwork.AutomaticallySyncScene)
                {
                    label += $"AutoSyncScene ";
                }
            }
            label += $"\r\nnick={PhotonNetwork.NickName}";
            GUILayout.Label(label, _guiLabelStyle);
        }

        #region Type names

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

        #endregion

#endif
    }
}