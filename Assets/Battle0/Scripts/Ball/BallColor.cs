using Altzone.Scripts.Battle;
using Battle0.Scripts.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle0.Scripts.Ball
{
    public class BallColor : MonoBehaviour, IBallColor
    {
        private const int msgSetBallColor = PhotonEventDispatcher.eventCodeBase + 4;

        private const int neutralColorIndex = 0;
        private const int upperTeamColorIndex = 1;
        private const int lowerTeamColorIndex = 2;
        private const int ghostedColorIndex = 3;

        [Header("Settings"), SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private Color normalColor;
        [SerializeField] private Color upperTeamColor;
        [SerializeField] private Color lowerTeamColor;
        [SerializeField] private Color ghostColor;

        [Header("Live Data"), SerializeField] private bool isNormalMode;
        [SerializeField] private int currentColorIndex;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            // HACK: stupid way to initialize us if/because BallActor is awaken before us :-(
            Debug.Log("Awake");
            ((IBallColor)this).initialize();
        }

        private void sendSetBallColor(int colorIndex, bool normalMode)
        {
            var payload = new[] { (byte)colorIndex, (byte)(normalMode ? 1 : 0) };
            photonEventDispatcher.RaiseEvent(msgSetBallColor, payload);
        }

        private void onSetBallColor(object data)
        {
            byte[] payload = (byte[])data;
            currentColorIndex = payload[0];
            isNormalMode = payload[1] == 1;
            if (!isNormalMode)
            {
                _sprite.color = ghostColor;
                return;
            }
            switch (currentColorIndex)
            {
                case lowerTeamColorIndex:
                    _sprite.color = lowerTeamColor;
                    return;
                case upperTeamColorIndex:
                    _sprite.color = upperTeamColor;
                    return;
                case ghostedColorIndex:
                    _sprite.color = ghostColor;
                    return;
                default:
                    _sprite.color = normalColor;
                    return;
            }
        }

        private void OnEnable()
        {
            var player = PhotonNetwork.LocalPlayer;
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var teamNumber = PhotonBattle.GetTeamNumber(playerPos);
            if (teamNumber == PhotonBattle.TeamRedValue)
            {
                // c# swap via deconstruction
                (upperTeamColor, lowerTeamColor) = (lowerTeamColor, upperTeamColor);
            }
            this.Subscribe<BallActor.ActiveTeamEvent>(onActiveTeamEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        void IBallColor.initialize()
        {
            if (photonEventDispatcher == null)
            {
                photonEventDispatcher = PhotonEventDispatcher.Get();
                photonEventDispatcher.registerEventListener(msgSetBallColor, data => { onSetBallColor(data.CustomData); });
                isNormalMode = true;
                currentColorIndex = 0;
            }
        }

        void IBallColor.setNormalMode()
        {
            isNormalMode = true;
            sendSetBallColor(currentColorIndex, isNormalMode);
        }

        void IBallColor.setGhostedMode()
        {
            isNormalMode = false;
            sendSetBallColor(currentColorIndex, isNormalMode);
        }

        private void onActiveTeamEvent(BallActor.ActiveTeamEvent data)
        {
            switch (data.newTeamNumber)
            {
                case PhotonBattle.TeamBlueValue:
                    sendSetBallColor(lowerTeamColorIndex, isNormalMode);
                    return;
                case PhotonBattle.TeamRedValue:
                    sendSetBallColor(upperTeamColorIndex, isNormalMode);
                    return;
                case PhotonBattle.NoTeamValue:
                    sendSetBallColor(neutralColorIndex, isNormalMode);
                    return;
                default:
                    throw new UnityException($"unknown team index: {data.newTeamNumber}");
            }
        }
    }
}