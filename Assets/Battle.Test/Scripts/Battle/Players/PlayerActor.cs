using System;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle.Factory;
using TMPro;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// This class manages local visual representation of player actor.
    /// </remarks>
    internal class PlayerActor : MonoBehaviour
    {
        [Serializable]
        internal class DebugSettings
        {
            public TextMeshPro _playerText;
        }
        
        [SerializeField] private PlayerDriver _playerDriver;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;
        
        public static PlayerActor Instantiate(PlayerDriver playerDriver, PlayerActor playerPrefab)
        {
            var player = playerDriver.Player;
            Debug.Log($"{player.GetDebugLabel()} prefab {playerPrefab.name}");
            
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var instantiationPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);

            var playerActor = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            var playerTag = $"{playerPos}:{player.NickName}";
            playerActor.name = playerActor.name.Replace("Clone", playerTag);
            playerActor.SetPlayerDriver(playerDriver);
            return playerActor;
        }
        
        private void Awake()
        {
            // Wait until PlayerDriver is assigned. 
            enabled = false;
        }

        private void SetPlayerDriver(PlayerDriver playerDriver)
        {
            // Now we are good to go.
            _playerDriver = playerDriver;
            enabled = true;
        }

        private void OnEnable()
        {
            var player = _playerDriver.Player;
            Debug.Log($"{player.GetDebugLabel()}");
            _debug._playerText.text = $"{player.ActorNumber}";
        }
    }
}