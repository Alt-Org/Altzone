using Battle0.Scripts.Ball;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle0.Scripts.Player
{
    /// <summary>
    /// Keep all players synchronized: play mode and color.
    /// </summary>
    public class PlayerModeController : MonoBehaviour
    {
        private const int msgSetActiveTeam = PhotonEventDispatcher.EventCodeBase + 1;

        private PhotonEventDispatcher photonEventDispatcher;

        private void Awake()
        {
            photonEventDispatcher = PhotonEventDispatcher.Get();
            photonEventDispatcher.RegisterEventListener(msgSetActiveTeam, data => { onSetActiveTeam(data.CustomData); });
        }

        private void OnEnable()
        {
            this.Subscribe<BallActor.ActiveTeamEvent>(onActiveTeamEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void onActiveTeamEvent(BallActor.ActiveTeamEvent data)
        {
            if (data.newTeamNumber == -1)
            {
                return; // Ignore indeterminate state
            }
            if (PhotonNetwork.IsMasterClient)
            {
                sendSetActiveTeam(data.newTeamNumber);
            }
        }

        private void sendSetActiveTeam(int activeTeamNumber)
        {
            photonEventDispatcher.RaiseEvent(msgSetActiveTeam, activeTeamNumber);
        }

        private void onSetActiveTeam(object data)
        {
            var activeTeamNumber = (int)data;
            foreach (var playerActor in PlayerActivator.AllPlayerActors)
            {
                if (playerActor.TeamNumber == activeTeamNumber)
                {
                    playerActor.setFrozenMode();
                }
                else
                {
                    playerActor.setNormalMode();
                }
            }
        }
    }
}