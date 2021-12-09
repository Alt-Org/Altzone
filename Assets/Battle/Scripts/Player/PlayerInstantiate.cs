using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Battle.Scripts.Scene;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Player
{
    /// <summary>
    /// Instantiates local networked player.
    /// </summary>
    public class PlayerInstantiate : MonoBehaviour
    {
        private void OnEnable()
        {
            var sceneConfig = SceneConfig.Get();
            var player = PhotonNetwork.LocalPlayer;
            Debug.Log($"OnEnable {player.GetDebugLabel()}");
            Assert.IsTrue(PhotonBattle.IsRealPlayer(player), "PhotonBattle.IsRealPlayer(player)");

            var playerPos = PhotonBattle.GetPlayerPos(player);
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            var playerPrefab = GetPlayerPrefab(defence);

            Debug.Log($"Instantiate pos={playerPos} prefab={playerPrefab.name}");
            var playerStartPos = sceneConfig.playerStartPos;
            var playerIndex = PhotonBattle.GetPlayerIndex(playerPos);
            var instantiationPosition = playerStartPos[playerIndex].position;
            PhotonNetwork.Instantiate(playerPrefab.name, instantiationPosition, Quaternion.identity);
            // ... rest of instantiation is done in PlayerActor (or elsewhere) because local and remote requirements can be different.
        }

        private static GameObject GetPlayerPrefab(Defence defence)
        {
            var prefabs = RuntimeGameConfig.Get().Prefabs;
            switch (defence)
            {
                case Defence.Desensitisation:
                    return prefabs._playerForDes;
                case Defence.Deflection:
                    return prefabs._playerForDef;
                case Defence.Introjection:
                    return prefabs._playerForInt;
                case Defence.Projection:
                    return prefabs._playerForPro;
                case Defence.Retroflection:
                    return prefabs._playerForRet;
                case Defence.Egotism:
                    return prefabs._playerForEgo;
                case Defence.Confluence:
                    return prefabs._playerForCon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defence), defence, null);
            }
        }
    }
}