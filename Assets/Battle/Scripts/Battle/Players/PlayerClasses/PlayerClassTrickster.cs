using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;
using Altzone.Scripts.Config;
using ExitGames.Client.Photon;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine.UIElements;
using Altzone.Scripts.Model.Poco.Game;


namespace Battle.Scripts.Battle.Players
{
    public class PlayerClassTrickster : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private ShieldManager _shieldManager;
        [SerializeField] private GameObject[] _shieldBounceRandomizers;

        [Obsolete("SpecialAbilityOverridesBallBounce is deprecated, please use return value of OnBallShieldCollision instead.")]
        public bool SpecialAbilityOverridesBallBounce => false;

        public void OnBallShieldCollision()
        {
            // Ammus vaihtaa sattumanvaraisesti suuntaa kun osuu suojakilpeen, jolloin on vaikea ennustaa mihin ammus menee.
            // Käytännössä kilpi vaihtuu joka osuman jälkeen

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldCollision called", _syncedFixedUpdateClock.UpdateCount));

            ShieldRandomizer();
        }

        public void OnBallShieldBounce()
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "OnBallShieldBounce called", _syncedFixedUpdateClock.UpdateCount));
        }

        public bool BounceOnBallShieldCollision => true;

        // Variables
        private PhotonView _photonView;
        private PhotonEventDispatcher _photonEventDispatcher;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS TRICKSTER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _photonView = GetComponent<PhotonView>();

            _photonEventDispatcher = Context.GetPhotonEventDispatcher;

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        private void ShieldRandomizer()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is master client", _syncedFixedUpdateClock.UpdateCount));

                int ShieldChangeUpdateNumber = _syncedFixedUpdateClock.UpdateCount + Math.Max(5, _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay));

                int choiceIndex = Random.Range(0, _shieldBounceRandomizers.Length);

                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Trickster shield set to " + choiceIndex, _syncedFixedUpdateClock.UpdateCount));
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Sending network message", _syncedFixedUpdateClock.UpdateCount));

                _photonView.RPC(nameof(ShieldRandomizerRpc), RpcTarget.All, ShieldChangeUpdateNumber, choiceIndex);
            }
            else
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is pleb client", _syncedFixedUpdateClock.UpdateCount));
            }
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            byte playerPos = 0;

            PlayerActor playerActor = transform.parent.GetComponentInParent<PlayerActor>();

            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.PlayerActor == playerActor)
                {
                    playerPos = (byte)(driver.PlayerPos - PhotonBattle.PlayerPosition1);
                    break;
                }
            }

            byte eventCode = (byte)(PhotonBattle.EventCodes.PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE + playerPos);

            _photonEventDispatcher.RegisterEventListener(eventCode, OnPhotonViewIdReceived);

            if (PhotonNetwork.IsMasterClient)
            {
                this.ExecuteOnNextFrame(() => {
                    PhotonNetwork.AllocateViewID(_photonView);

                    _photonEventDispatcher.RaiseEvent(PhotonBattle.EventCodes.PLAYER_CLASS_TRICKSTER_SET_PHOTON_VIEW_ID_EVENTCODE, _photonView.ViewID);
                });
            }
        }

        private void OnPhotonViewIdReceived(EventData data)
        {
            _photonView.ViewID = (int) data.CustomData;
        }

        [PunRPC]
        private void ShieldRandomizerRpc(int ShieldChangeUpdateNumber, int choiceIndex)
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Receiving network message", _syncedFixedUpdateClock.UpdateCount));

            _syncedFixedUpdateClock.ExecuteOnUpdate(ShieldChangeUpdateNumber, -10, () =>
            {
                GameObject _shieldChoice = _shieldBounceRandomizers[choiceIndex];

                _shieldManager.SetShield(_shieldChoice);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME + "Shield is set to choice {1}", _syncedFixedUpdateClock.UpdateCount, choiceIndex));
            });
        }
    }
}
