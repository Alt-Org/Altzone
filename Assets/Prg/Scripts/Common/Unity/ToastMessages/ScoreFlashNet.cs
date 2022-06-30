using System;
using System.Text;
using Prg.Scripts.Common.Photon;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Unity.ToastMessages
{
    /// <summary>
    /// Networked version of <c>ScoreFlash</c> for current level. It will destroy itself when scene is unloaded!
    /// </summary>
    /// <remarks>
    /// Use <c>RegisterEventListener</c> if only listening for <c>ScoreFlash</c> messages.
    /// </remarks>
    public static class ScoreFlashNet
    {
        private static IScoreFlash _instance;
        private static bool _isCallbackRegistered;

        private static IScoreFlash Get()
        {
            return _instance ??= new ScoreFlasherNet();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _instance = null;
        }

        public static void RegisterEventListener()
        {
            if (!_isCallbackRegistered)
            {
                _isCallbackRegistered = true;
                SceneManager.sceneUnloaded += SceneUnloaded;
            }
            Get();
        }

        private static void SceneUnloaded(Scene scene)
        {
            _instance = null;
            _isCallbackRegistered = false;
            SceneManager.sceneUnloaded -= SceneUnloaded;
        }

        public static void Push(string message)
        {
            Get().Push(message, 0f, 0f);
        }

        public static void Push(string message, float worldX, float worldY)
        {
            Get().Push(message, worldX, worldY);
        }

        public static void Push(string message, Vector2 worldPosition)
        {
            Get().Push(message, worldPosition.x, worldPosition.y);
        }

        public static void Push(string message, Vector3 worldPosition)
        {
            Get().Push(message, worldPosition.x, worldPosition.y);
        }
    }

    internal class ScoreFlasherNet : IScoreFlash
    {
        private const int MsgScoreFlash = PhotonEventDispatcher.EventCodeBase + 6;
        private const int MsgBufferFixedLength = 4 + 4 + 1;
        private const int MaxStringMessageLength = 24;

        private readonly PhotonEventDispatcher _photonEventDispatcher;

        private byte[] _messageBuffer = Array.Empty<byte>();

        internal ScoreFlasherNet()
        {
            Debug.Log($"");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgScoreFlash, data => { OnScoreFlash((byte[])data.CustomData); });
        }

        void IScoreFlash.Push(string message, float worldX, float worldY)
        {
            Assert.IsTrue(message.Length <= MaxStringMessageLength, "message.Length <= MaxMessageLength");
            if (message.Length > MaxStringMessageLength)
            {
                message = message.Substring(0, MaxStringMessageLength);
            }
            SendScoreFlash(message, worldX, worldY);
        }

        private static void OnScoreFlash(string message, float x, float y)
        {
            Debug.Log($"{x:0.0},{y:0.0}: {message}");
            ScoreFlash.Push(message, x, y);
        }

        #region Photon Events

        private void OnScoreFlash(byte[] payload)
        {
            var index = 0;
            var x = BitConverter.ToSingle(payload, index);
            index += 4;
            var y = BitConverter.ToSingle(payload, index);
            index += 4;
            var messageLength = (int)payload[index];
            index += 1;
            var message = Encoding.UTF8.GetString(payload, index, messageLength);
            OnScoreFlash(message, x, y);
        }

        private void SendScoreFlash(string message, float worldX, float worldY)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            var requiredBufferLength = MsgBufferFixedLength + messageBytes.Length;
            if (_messageBuffer.Length < requiredBufferLength)
            {
                _messageBuffer = new byte[requiredBufferLength];
            }
            var index = 0;
            Array.Copy(BitConverter.GetBytes(worldX), 0, _messageBuffer, index, 4);
            index += 4;
            Array.Copy(BitConverter.GetBytes(worldY), 0, _messageBuffer, index, 4);
            index += 4;
            _messageBuffer[index] = (byte)messageBytes.Length;
            index += 1;
            Array.Copy(messageBytes, 0, _messageBuffer, index, messageBytes.Length);

            _photonEventDispatcher.RaiseEvent(MsgScoreFlash, _messageBuffer);
        }

        #endregion
    }
}