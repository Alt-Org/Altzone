using System;
using System.Collections.Generic;

using UnityEngine;

namespace Altzone.Scripts.Lobby
{
    internal sealed class JoinAttemptTracker
    {
        private sealed class JoinAttemptInfo
        {
            public string RoomName = string.Empty;
            public string[] ExpectedUsers = null;
            public float StartTime = 0f;
            public bool Completed = false;
            public bool Success = false;
            public float CompletionTime = 0f;
            public short? FailureCode = null;
            public string FailureMessage = null;
        }

        private readonly object _lock = new object();
        private int _joinAttemptCounter = 0;
        private int _currentJoinAttemptId = 0;
        private readonly Dictionary<int, JoinAttemptInfo> _joinAttempts = new Dictionary<int, JoinAttemptInfo>();

        public int CurrentJoinAttemptId
        {
            get
            {
                lock (_lock)
                {
                    return _currentJoinAttemptId;
                }
            }
        }

        public int LastIssuedAttemptId
        {
            get
            {
                lock (_lock)
                {
                    return _joinAttemptCounter;
                }
            }
        }

        public int BeginJoinAttempt(string roomName, string[] expectedUsers = null)
        {
            int id = ++_joinAttemptCounter;
            JoinAttemptInfo info = new JoinAttemptInfo()
            {
                RoomName = roomName ?? string.Empty,
                ExpectedUsers = expectedUsers,
                StartTime = Time.time,
                Completed = false,
                Success = false
            };

            lock (_lock)
            {
                _joinAttempts[id] = info;
                _currentJoinAttemptId = id;
            }

            Debug.Log($"JoinAttempt[{id}] BEGIN room='{info.RoomName}' teammates={expectedUsers?.Length ?? 0}");
            return id;
        }

        public void MarkJoinAttemptSuccess(int id)
        {
            lock (_lock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                if (id == 0) return;

                if (_joinAttempts.TryGetValue(id, out JoinAttemptInfo info))
                {
                    info.Completed = true;
                    info.Success = true;
                    info.CompletionTime = Time.time;
                    Debug.Log($"JoinAttempt[{id}] SUCCESS room='{info.RoomName}'");
                }

                if (_currentJoinAttemptId == id) _currentJoinAttemptId = 0;
                _joinAttempts.Remove(id);
            }
        }

        public void MarkJoinAttemptFailure(int id, short returnCode, string message)
        {
            lock (_lock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                if (id == 0)
                {
                    Debug.LogWarning($"JoinAttempt: failure callback with no current attempt (code={returnCode} msg={message})");
                    return;
                }

                if (_joinAttempts.TryGetValue(id, out JoinAttemptInfo info))
                {
                    info.Completed = true;
                    info.Success = false;
                    info.FailureCode = returnCode;
                    info.FailureMessage = message;
                    info.CompletionTime = Time.time;
                    Debug.Log($"JoinAttempt[{id}] FAILED room='{info.RoomName}' code={returnCode} msg={message}");
                }

                if (_currentJoinAttemptId == id) _currentJoinAttemptId = 0;
                _joinAttempts.Remove(id);
            }
        }

        public bool IsAttemptCompleted(int id)
        {
            lock (_lock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                return id != 0 && _joinAttempts.TryGetValue(id, out JoinAttemptInfo info) && info.Completed;
            }
        }

        public bool TryGetFailedJoinAttempt(int id, out string failureMessage)
        {
            lock (_lock)
            {
                if (id == 0) id = _currentJoinAttemptId;
                if (id != 0 && _joinAttempts.TryGetValue(id, out JoinAttemptInfo info) && info.Completed && !info.Success)
                {
                    failureMessage = info.FailureMessage;
                    return true;
                }
            }

            failureMessage = null;
            return false;
        }

        public int FindJoinAttemptIdForRoomName(string roomName)
        {
            roomName = roomName ?? string.Empty;
            lock (_lock)
            {
                if (_currentJoinAttemptId != 0
                    && _joinAttempts.TryGetValue(_currentJoinAttemptId, out JoinAttemptInfo current)
                    && string.Equals(current.RoomName, roomName, StringComparison.Ordinal))
                {
                    return _currentJoinAttemptId;
                }

                foreach (KeyValuePair<int, JoinAttemptInfo> kvp in _joinAttempts)
                {
                    if (string.Equals(kvp.Value.RoomName, roomName, StringComparison.Ordinal))
                    {
                        return kvp.Key;
                    }
                }
            }

            return 0;
        }
    }
}