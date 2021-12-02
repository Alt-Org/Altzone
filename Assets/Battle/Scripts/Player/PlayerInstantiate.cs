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
            Assert.IsTrue(PhotonBattle.IsRealPlayer(player), "PhotonBattle.IsRealPlayer(player)");
            var playerPos = PhotonBattle.GetPlayerPos(player);
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            var playerPrefab = GetPlayerPrefab(defence);

            Debug.Log($"Instantiate pos={playerPos} prefab={playerPrefab.name} {PhotonNetwork.LocalPlayer.GetDebugLabel()}");

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
                    return prefabs.playerForDes;
                case Defence.Deflection:
                    return prefabs.playerForDef;
                case Defence.Introjection:
                    return prefabs.playerForInt;
                case Defence.Projection:
                    return prefabs.playerForPro;
                case Defence.Retroflection:
                    return prefabs.playerForRet;
                case Defence.Egotism:
                    return prefabs.playerForEgo;
                case Defence.Confluence:
                    return prefabs.playerForCon;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defence), defence, null);
            }
        }
    }
}