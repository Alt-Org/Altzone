using System;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Test
{
    public class PhotonPingWindow : MonoBehaviour
    {
        public bool _visible;
        public Key _controlKey = Key.F4;

#if DEVELOPMENT_BUILD || UNITY_EDITOR

        private int _windowId;
        private Rect _windowRect;
        private string _windowTitle;
        private bool _hasStyles;
        private GUIStyle _guiButtonStyle;
        private GUIStyle _guiLabelStyle;

        private int _regionCount;
        private List<Region> _enabledRegions;

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
            var curRegion = PhotonLobby.GetRegion();
            if (!string.IsNullOrEmpty(curRegion))
            {
                curRegion = $"'{curRegion}' {PhotonNetwork.GetPing()} ms";
            }
            var title = $"Photon: {PhotonNetwork.NetworkClientState} {curRegion}";
            if (GUILayout.Button(title, _guiButtonStyle))
            {
                ToggleWindowState();
                return;
            }
            if (_enabledRegions == null)
            {
                if (PhotonNetwork.NetworkingClient.RegionHandler?.EnabledRegions != null)
                {
                    _enabledRegions = new List<Region>();
                    foreach (var enabledRegion in PhotonNetwork.NetworkingClient.RegionHandler.EnabledRegions)
                    {
                        var pingRegion = new Region(enabledRegion.Code, enabledRegion.HostAndPort);
                        Debug.Log(pingRegion.ToString());
                        _enabledRegions.Add(pingRegion);
                        new RegionPinger(pingRegion, region =>
                                pingRegion.Ping = region.Ping)
                            .Start();
                    }
                    _regionCount = _enabledRegions.Count;
                }
                return;
            }
            try
            {
                foreach (var enabledRegion in _enabledRegions)
                {
                    GUILayout.Button($"{enabledRegion.Code} {enabledRegion.Ping} ms", _guiLabelStyle);
                }
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }
#endif
    }
}