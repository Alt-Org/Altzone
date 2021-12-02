using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace Prg.Scripts.Common.Photon
{
    /// <summary>
    /// Convenience class and example how to keep updated room list in a cache.
    /// </summary>
    /// <remarks>
    /// Note that <c>PhotonNetwork.GetRoomList</c> does not exist and
    /// you have to manually do your own book keeping of known rooms with <c>ILobbyCallbacks.OnRoomListUpdate</c>.
    /// </remarks>
    public class PhotonRoomList : MonoBehaviour, ILobbyCallbacks
    {
        [SerializeField] private int roomListCount; // Just for debugging in Editor

        private List<RoomInfo> currentRoomList = new List<RoomInfo>(); // Cached list of current rooms

        public Action roomsUpdated;

        public ReadOnlyCollection<RoomInfo> currentRooms => getRoomListing();

        public ReadOnlyCollection<RoomInfo> getRoomListing()
        {
            if (PhotonNetwork.InLobby)
            {
                return currentRoomList.AsReadOnly();
            }
            if (PhotonNetwork.NetworkClientState == ClientState.Joining)
            {
                // It seems that OnRoomListUpdate can happen between transitioning from lobby to room:
                // -> JoinedLobby -> Joining -> Joined
                currentRoomList.Clear();
                roomListCount = 0;
                return currentRoomList.AsReadOnly();
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

        private void updateRoomListing(List<RoomInfo> roomList)
        {
            // We always remove and add entries to keep cached data up-to-date.
            foreach (var newRoomInfo in roomList)
            {
                var curRoomInfoIndex = currentRoomList.FindIndex(x => x.Equals(newRoomInfo));
                if (curRoomInfoIndex != -1)
                {
                    currentRoomList.RemoveAt(curRoomInfoIndex);
                    if (newRoomInfo.RemovedFromList)
                    {
                        continue; // No need to add as this will be disappear soon!
                    }
                }
                currentRoomList.Add(newRoomInfo);
            }
            if (currentRoomList.Any(x => x.RemovedFromList))
            {
                // Remove removed rooms from cache
                currentRoomList = currentRoomList.Where(x => !x.RemovedFromList).ToList();
            }
        }

        void ILobbyCallbacks.OnJoinedLobby()
        {
            currentRoomList.Clear();
            roomListCount = 0;
            Debug.Log($"OnJoinedLobby roomsUpdated: {roomListCount}");
            roomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLeftLobby()
        {
            currentRoomList.Clear();
            roomListCount = 0;
            Debug.Log($"OnLeftLobby roomsUpdated: {roomListCount}");
            roomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnRoomListUpdate(List<RoomInfo> roomList)
        {
            updateRoomListing(roomList);
            roomListCount = roomList.Count;
            Debug.Log($"OnRoomListUpdate roomsUpdated: {roomListCount}");
            roomsUpdated?.Invoke();
        }

        void ILobbyCallbacks.OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            // NOP
        }
    }
}