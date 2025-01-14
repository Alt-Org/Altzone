using System;
using Altzone.Scripts.Config;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Battle1.Scripts.Battle.Game;
using ExitGames.Client.Photon;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/
using Random = UnityEngine.Random;

namespace Battle1.Scripts.Battle.Players.PlayerClasses
{
    internal class PlayerClassTrickster : MonoBehaviour, IPlayerClass
    {
        // Serialized fields
        [SerializeField] private ShieldManager _shieldManager;
        [SerializeField] private GameObject[] _shieldBounceRandomizers;

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        public bool BounceOnBallShieldCollision => true;

        public void InitInstance(IReadOnlyBattlePlayer battlePlayer)
        {
            _battlePlayer = battlePlayer;
            _battleDebugLogger = new BattleDebugLogger(this);
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
           /* _photonView = GetComponent<PhotonView>();*/
            /*_photonEventDispatcher = Context.GetPhotonEventDispatcher;*/
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        public void OnBallShieldCollision()
        {
            // Projectile changes direction at random, making it difficult to predict.
            // Shield changes after every contact

            _battleDebugLogger.LogInfo("OnBallShieldCollision called");

            ShieldRandomizer();
        }

        public void OnBallShieldBounce()
        {
            _battleDebugLogger.LogInfo("OnBallShieldBounce called");
        }

        // Variables
        private IReadOnlyBattlePlayer _battlePlayer;
      /*  private PhotonView _photonView;
        private PhotonEventDispatcher _photonEventDispatcher;*/

        // Debug
        private BattleDebugLogger _battleDebugLogger;
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER CLASS TRICKSTER] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock; // only needed for logging time

        // debug
        private void Start()
        {}

        private void ShieldRandomizer()
        {
            /*if (PhotonNetwork.IsMasterClient)
            {
                _battleDebugLogger.LogInfo("Shield is master client");

                int ShieldChangeUpdateNumber = _syncedFixedUpdateClock.UpdateCount + Math.Max(5, _syncedFixedUpdateClock.ToUpdates(GameConfig.Get().Variables._networkDelay));

                int choiceIndex = Random.Range(0, _shieldBounceRandomizers.Length);

                _battleDebugLogger.LogInfo("Trickster shield set to " + choiceIndex);
                _battleDebugLogger.LogInfo("Sending network message");

                _photonView.RPC(nameof(ShieldRandomizerRpc), RpcTarget.All, ShieldChangeUpdateNumber, choiceIndex);
            }
            else
            {
                _battleDebugLogger.LogInfo("Shield is pleb client");
            }*/
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PlayerActor playerActor = _battlePlayer.PlayerActor;
            int playerPos = _battlePlayer.PlayerPosition;
            _battleDebugLogger.LogInfo("Player position is " + playerPos);

            byte eventCode = (byte)(PhotonBattle.EventCodes.PlayerClassTricksterSetPhotonViewIdEventCode + playerPos);

            /*_photonEventDispatcher.RegisterEventListener(eventCode, OnPhotonViewIdReceived);

            if (PhotonNetwork.IsMasterClient)
            {
                this.ExecuteOnNextFrame(() =>
                {
                    PhotonNetwork.AllocateViewID(_photonView);

                    _photonEventDispatcher.RaiseEvent(PhotonBattle.EventCodes.PlayerClassTricksterSetPhotonViewIdEventCode, _photonView.ViewID);
                });
            }*/
        }

        /*private void OnPhotonViewIdReceived(EventData data)
        {
            _photonView.ViewID = (int) data.CustomData;
        }

        [PunRPC]
        private void ShieldRandomizerRpc(int ShieldChangeUpdateNumber, int choiceIndex)
        {
            _battleDebugLogger.LogInfo("Receiving network message");

            _syncedFixedUpdateClock.ExecuteOnUpdate(ShieldChangeUpdateNumber, -10, () =>
            {
                GameObject _shieldChoice = _shieldBounceRandomizers[choiceIndex];

                _shieldManager.SetShield(_shieldChoice);
                _battleDebugLogger.LogInfo("Shield is set to choice " + choiceIndex);
            });
        }*/
    }
}
