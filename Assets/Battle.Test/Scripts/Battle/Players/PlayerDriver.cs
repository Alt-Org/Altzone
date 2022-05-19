using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Model;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriver : MonoBehaviourPunCallbacks
    {
        [Serializable]
        internal class DebugSettings
        {
            public PlayerActor _playerPrefab;
        }

        [Header("Live Data"), SerializeField] private PlayerActor _playerActor;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        public Player Player => photonView.Owner;

        public CharacterModel CharacterModel { get; private set; }

        public static void InstantiateLocalPlayer(Player player, string networkPrefabName)
        {
            Assert.IsTrue(player.IsLocal, "player.IsLocal");
            Debug.Log($"{player.GetDebugLabel()} prefab {networkPrefabName}");
            var instance = PhotonNetwork.Instantiate(networkPrefabName, Vector3.zero, Quaternion.identity);
        }

        private void Awake()
        {
            print("+");
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var playerTag = $"{playerPos}:{player.NickName}";
            name = name.Replace("Clone", playerTag);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            var player = photonView.Owner;
            Debug.Log($"{player.GetDebugLabel()} {photonView}");
            if (!PhotonBattle.IsRealPlayer(player))
            {
                return;
            }
            if (_playerActor != null)
            {
                return;
            }
            _playerActor = PlayerActor.Instantiate(this, _debug._playerPrefab);
            CharacterModel = PhotonBattle.GetCharacterModelForRoom(player);
        }
    }
}