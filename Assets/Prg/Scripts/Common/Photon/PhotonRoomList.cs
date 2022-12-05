using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Convenience class and example how to keep updated room list in a cache.
    /// </summary>
    /// <remarks>
    /// Note that <c>PhotonNetwork.GetRoomList</c> does not exist anymore and
    /// you have to manually do your own book keeping of known rooms with <c>ILobbyCallbacks.OnRoomListUpdate</c>.
    /// </remarks>
    public class PhotonRoomList : MonoBehaviour, ILobbyCallbacks
    {
        [SerializeField] private int _debugRoomListCount;

        private List<RoomInfo> _currentRoomList = new();

        public Action OnRoomsUpdated;

        public ReadOnlyCollection<RoomInfo> CurrentRooms => GetCurrentRooms();

        public ReadOnlyCollection<RoomInfo> GetCurrentRooms()
        {
            if (PhotonNetwork.InLobby)
            {
                return _currentRoomList.AsReadOnly();
            }
            if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            {
                // It seems that OnRoomListUpdate can happen between transitioning from lobby to room:
                // -> JoinedLobby -> Joining -> Joined
                _currentRoomList.Clear();
                _debugRoomListCount = 0;
                return _currentRoomList.AsReadOnly();
            }
            throw new UnityException($"Invalid connection state: {PhotonNetwork.NetworkClientState}");
        }

        private void OnEnable()
        {
            PhotonNetwork.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonNetwork.RemoveCallbackTarget(this);
        }

        private void UpdateRoomListing(List<RoomInfo> roomList)
        {
            // We always remove and add entries to keep cached data up-to-date.
            foreach (var newRoomInfo in roomList)
            {
                var curRoomInfoIndex = _currentRoomList.FindIndex(x => x.Equals(newRoomInfo));
                if (curRoomInfoIndex != -1)
                {
                    _currentRoomList.RemoveAt(curRoomInfoIndex);
                    if (newRoomInfo.RemovedFromList)
                    {
                        continue; // No need to add as this will be disappear soon!
                    }
                }
                _currentRoomList.Add(newRoomInfo);
            }
            if (_currentRoomList.Any(x => x.RemovedFromList))
            {
                // Remove removed rooms from cache
                _currentRoomList = _currentRoomList.Where(x => !x.RemovedFromList).ToList();
            }
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
            _currentRoomList.Clear();
            _debugRoomListCount = 0;
            Debug.Log($"roomsUpdated: {_debugRoomListCount}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            _currentRoomList.Clear();
            _debugRoomListCount = 0;
            Debug.Log($"roomsUpdated: {_debugRoomListCount}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            UpdateRoomListing(roomList);
            _debugRoomListCount = roomList.Count;
            Debug.Log($"roomsUpdated: {_debugRoomListCount}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            // NOP
        }
    }
}