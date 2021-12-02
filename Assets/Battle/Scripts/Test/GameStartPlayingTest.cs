using Altzone.Scripts.Config;
using Battle.Scripts.SlingShot;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle.Scripts.Test
{
    /// <summary>
    /// Test script to start room using <c>BallSlingShotTest</c> when countdown reaches zero.
    /// </summary>
    public class GameStartPlayingTest : MonoBehaviour
    {
        private const int photonEventCode = PhotonEventDispatcher.eventCodeBase + 3;

        [Header("Live Data"), SerializeField] private int secondsRemaining;
        [SerializeField] private float roomCountdownTime;

        private PhotonEventDispatcher photonEventDispatcher;

        // Configurable settings
        private GameVariables variables;

        private void Awake()
        {
            variables = RuntimeGameConfig.Get().Variables;
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.registerEventListener(photonEventCode, data => { handleRoomTimerProgress(data.CustomData); });
        }

        private void Start()
        {
            Debug.Log($"Start: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
        }

        private void OnEnable()
        {
            Debug.Log($"OnEnable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
            // Timer start running from here!
            secondsRemaining = variables.roomStartDelay;
            sendRoomTimerProgress();
            roomCountdownTime = Time.time + 1.0f;
        }

        private void OnDisable()
        {
            Debug.Log($"OnDisable: {PhotonNetwork.NetworkClientState} time={Time.time:0.00}");
            this.Unsubscribe();
        }

        private void sendRoomTimerProgress()
        {
            // Synchronize to all game managers
            var payload = secondsRemaining;
            photonEventDispatcher.RaiseEvent(photonEventCode, payload);
        }

        private void handleRoomTimerProgress(object payload)
        {
            secondsRemaining = (int)payload;
            this.Publish(new CountdownEvent(variables.roomStartDelay, secondsRemaining));
            if (secondsRemaining <= 0)
            {
                startRoom();
                enabled = false;
            }
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            if (Time.time > roomCountdownTime)
            {
                secondsRemaining -= 1;
                sendRoomTimerProgress();
                roomCountdownTime = Time.time + 1.0f;
            }
        }

        private void startRoom()
        {
            Debug.Log("*");
            Debug.Log($"* startRoom={Time.time:0.00}");
            Debug.Log("*");
            BallSlingShot.startTheBall();
        }

        internal class CountdownEvent
        {
            public readonly int maxCountdownValue;
            public readonly int curCountdownValue;

            public CountdownEvent(int maxCountdownValue, int curCountdownValue)
            {
                this.maxCountdownValue = maxCountdownValue;
                this.curCountdownValue = curCountdownValue;
            }

            public override string ToString()
            {
                return $"max: {maxCountdownValue}, cur: {curCountdownValue}";
            }
        }
    }
}