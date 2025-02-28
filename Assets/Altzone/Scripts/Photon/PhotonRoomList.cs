using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Photon.Realtime;
using UnityEngine;
using Altzone.Scripts.Lobby.Wrappers;
//using ClientState = Battle1.PhotonRealtime.Code.ClientState;
//using ILobbyCallbacks = Battle1.PhotonRealtime.Code.ILobbyCallbacks;
//using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;
//using RoomInfo = Battle1.PhotonRealtime.Code.RoomInfo;
//using TypedLobbyInfo = Battle1.PhotonRealtime.Code.TypedLobbyInfo;

namespace Altzone.Scripts.Common.Photon
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

        private List<LobbyRoomInfo> _currentRoomList = new();

        public Action OnRoomsUpdated;

        public ReadOnlyCollection<LobbyRoomInfo> CurrentRooms => GetCurrentRooms();

        public ReadOnlyCollection<LobbyRoomInfo> GetCurrentRooms()
        {
            if (PhotonRealtimeClient.InLobby)
            {
                return _currentRoomList.AsReadOnly();
            }
            if (PhotonRealtimeClient.NetworkClientState == ClientState.Joining)
            {
                // It seems that OnRoomListUpdate can happen between transitioning from lobby to room:
                // -> JoinedLobby -> Joining -> Joined
                _currentRoomList.Clear();
                _debugRoomListCount = 0;
                return _currentRoomList.AsReadOnly();
            }
            throw new UnityException($"Invalid connection state: {PhotonRealtimeClient.NetworkClientState}");
        }

        private void OnEnable()
        {
            PhotonRealtimeClient.Client.AddCallbackTarget(this);
        }

        private void OnDisable()
        {
            PhotonRealtimeClient.Client.RemoveCallbackTarget(this);
        }

        private void UpdateRoomListing(List<LobbyRoomInfo> roomList)
        {
            // We always remove and add entries to keep cached data up-to-date.
            foreach (var newRoomInfo in roomList)
            {
                var curRoomInfoIndex = _currentRoomList.FindIndex(x => x.Equals(newRoomInfo)); // doesn't return the correct index even though the room is same that's why deleted rooms don't get deleted from list
                if (curRoomInfoIndex != -1)
                {
                    _currentRoomList.RemoveAt(curRoomInfoIndex);
                    //if (newRoomInfo.RemovedFromList)
                    //{
                    //    continue; // No need to add as this will be disappear soon!
                    //}
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
            //_currentRoomList.Clear();
            //_debugRoomListCount = 0;
            Debug.Log($"roomsUpdated: {_debugRoomListCount} CloudRegion={PhotonRealtimeClient.CloudRegion}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            //_currentRoomList.Clear();
            //_debugRoomListCount = 0;
            Debug.Log($"roomsUpdated: {_debugRoomListCount}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            //_currentRoomList.Clear();
            List<LobbyRoomInfo> lobbyRoomList = new();
            foreach (RoomInfo roomInfo in roomList)
            {
                lobbyRoomList.Add(new(roomInfo));
            }

            UpdateRoomListing(lobbyRoomList);
            _debugRoomListCount = lobbyRoomList.Count;
            Debug.Log($"Lobby Update: roomsUpdated: {_debugRoomListCount}");
            OnRoomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            // NOP
        }
    }
}
