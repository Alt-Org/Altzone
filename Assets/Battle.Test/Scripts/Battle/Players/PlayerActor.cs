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
        
        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;
        
        private IPlayerDriver _playerDriver;

        public static PlayerActor Instantiate(IPlayerDriver playerDriver, PlayerActor playerPrefab)
        {
            Debug.Log($"prefab {playerPrefab.name}");

            var playerPos = playerDriver.PlayerPos;
            var instantiationPosition = Context.GetPlayerPlayArea.GetPlayerStartPosition(playerPos);

            var playerActor = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            var playerTag = $"{playerPos}:{playerDriver.NickName}";
            playerActor.name = playerActor.name.Replace("Clone", playerTag);
            playerActor.SetPlayerDriver(playerDriver);
            return playerActor;
        }
        
        private void Awake()
        {
            Debug.Log($"{name} {enabled}");
            // Wait until PlayerDriver is assigned.
            if (enabled)
            {
                enabled = false;
            }
        }

        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            Debug.Log($"{name} {enabled}");
            // Now we are good to go.
            _playerDriver = playerDriver;
            enabled = true;
        }

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _debug._playerText.text = $"{_playerDriver.ActorNumber}";
        }
    }
}