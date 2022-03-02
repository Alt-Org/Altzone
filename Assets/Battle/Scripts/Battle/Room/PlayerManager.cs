using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Manages local player gameplay start.
    /// </summary>
    internal class PlayerManager : MonoBehaviour, IPlayerManager
    {
        private const int MsgCountdown = PhotonEventDispatcher.EventCodeBase + 3;

        private PhotonEventDispatcher _photonEventDispatcher;
        private Action _countdownFinished;
        private IPlayerLineConnector _playerLineConnector;
        private PlayerLineResult _nearest;

        private void Awake()
        {
            Debug.Log("Awake");
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.RegisterEventListener(MsgCountdown, data => { OnCountdown(data.CustomData); });
        }

        #region Photon Events

        private void OnCountdown(object data)
        {
            var payload = (int[])data;
            Assert.AreEqual(3, payload.Length, "Invalid message length");
            Assert.AreEqual(MsgCountdown, payload[0], "Invalid message id");
            var curValue = payload[1];
            var maxValue = payload[2];
            Debug.Log($"OnCountdown {curValue}/{maxValue}");
            this.Publish(new CountdownEvent(curValue, maxValue));
            if (curValue >= 0)
            {
                return;
            }
            _nearest = _playerLineConnector.GetNearest();
            _playerLineConnector.Hide();
            _playerLineConnector = null;
            _countdownFinished?.Invoke();
            _countdownFinished = null;
        }

        private void SendCountdown(int curValue, int maxValue)
        {
            var payload = new[] { MsgCountdown, curValue, maxValue };
            _photonEventDispatcher.RaiseEvent(MsgCountdown, payload);
        }

        #endregion

        #region IPlayerManager

        void IPlayerManager.StartCountdown(Action countdownFinished)
        {
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"StartCountdown {player.GetDebugLabel()} master {PhotonNetwork.IsMasterClient}");
            _countdownFinished = countdownFinished;
            if (PhotonNetwork.IsMasterClient)
            {
                var roomStartDelay = RuntimeGameConfig.Get().Variables._roomStartDelay;
                StartCoroutine(DoCountdown(roomStartDelay));
            }
            var playerActor = Context.GetPlayer(PhotonBattle.GetPlayerPos(player));
            _playerLineConnector = Context.GetTeamLineConnector(playerActor.TeamNumber);
            _playerLineConnector.Connect(playerActor);
        }

        private IEnumerator DoCountdown(int startValue)
        {
            var curValue = startValue;
            SendCountdown(curValue, startValue);
            var delay = new WaitForSeconds(1f);
            for (;;)
            {
                yield return delay;
                SendCountdown(--curValue, startValue);
                if (curValue < 0)
                {
                    yield break;
                }
            }
        }

        void IPlayerManager.StartGameplay()
        {
            Debug.Log(
                $"StartGameplay nearest {_nearest.PlayerActor.Transform.name} distY {_nearest.DistanceY:F1} master {PhotonNetwork.IsMasterClient}");
            foreach (var playerActor in Context.GetPlayers)
            {
                var actorTransform = playerActor.Transform;
                var actorPosition = actorTransform.position;
                var dist = Mathf.Abs((actorPosition - Vector3.zero).magnitude);
                Debug.Log($"{actorTransform.name} x={actorPosition.x:F1} y={actorPosition.y:F1} dist={dist:F1}");
            }
            if (PhotonNetwork.IsMasterClient)
            {
                _nearest.PlayerActor.SetGhostedMode();
                var ball = Context.GetBall;
                ball.SetColor(BallColor.NoTeam);
                var position = _nearest.PlayerActor.Transform.position;
                ball.StartMoving(position, _nearest.Force);
            }
        }

        void IPlayerManager.StopGameplay()
        {
            Debug.Log($"StopGameplay {PhotonNetwork.LocalPlayer.GetDebugLabel()}");
        }

        #endregion

        internal class CountdownEvent
        {
            public readonly int CurValue;
            public readonly int MaxValue;

            public CountdownEvent(int curValue, int maxValue)
            {
                CurValue = curValue;
                MaxValue = maxValue;
            }
        }
    }
}