using System;
using System.Collections.Generic;
using System.Linq;
/*using Battle1.PhotonRealtime.Code;*/
#if PHOTON_UNITY_NETWORKING
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
#endif
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Prg.Scripts.DevUtil
{
    /// <summary>
    /// Helper OnGUI window to show some Photon related info as "overlay" window.
    /// </summary>
    internal class PhotonStatsWindow : MonoBehaviour
    {
        public bool _visible;
        public Key _controlKey = Key.F2;

#if PHOTON_UNITY_NETWORKING && (DEVELOPMENT_BUILD || UNITY_EDITOR)
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

  /*      private void OnGUI()
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
        }*/

        /*private void DebugWindow(int windowId)
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
            // We use LoadBalancingPeer to access timing related info to explicitly to show how things work 'under the hood'.
            // - typically we would use PhotonNetwork for this to hide actual internal implementation details for us.
            var peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
            var space = "  ";
            var label = $"game ver={PhotonLobby.GameVersion}\r\n" +
                        $"reg={PhotonNetwork.CloudRegion} ping={peer.RoundTripTime} var={peer.RoundTripTimeVariance}";
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
            label += $"\r\nnick={PhotonNetwork.NickName}\r\n{FormatServerTimestamp(peer)}";
            GUILayout.Label(label, _guiLabelStyle);
        }*/

    /*    private static string FormatServerTimestamp(LoadBalancingPeer peer)
        {
            // Synchronized Timestamp (PhotonNetwork.ServerTimestamp)
            // https://doc.photonengine.com/pun/v2/getting-started/feature-overview#synchronized_timestamp
            // More details how it works
            // https://forum.photonengine.com/discussion/1112/servertimeinmilliseconds-and-fetchservertimestamp
            if (PhotonNetwork.OfflineMode)
            {
                return "offline";
            }
            return $"timestamp {(uint)(peer.ServerTimeInMilliSeconds):# ### ### ##0}";
        }*/

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
