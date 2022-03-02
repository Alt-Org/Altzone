using Prg.Scripts.Common.Photon;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UiProto.Scripts.Window
{
    public class SceneLoader : MonoBehaviour
    {
        private static List<LevelIdDef> levels;

        static SceneLoader()
        {
            levels = LevelNames.loadLevelDefs();
            Debug.Log($"levels #{levels.Count} {string.Join(", ", levels)}");
        }

        public static LevelId GetLevelId(string levelName)
        {
            var level = findLevel(levelName);
            if (level != null)
            {
                return LevelId.CreateFrom(level);
            }
            return null;
        }

        public static void LoadScene(string levelName)
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"LoadScene {PhotonWrapper.NetworkClientState} : {currentSceneName} <- {levelName}");
            var level = findLevel(levelName);
            if (level == null)
            {
                Debug.Log($"levels {string.Join("| ", levels)}");
                WindowStack.dumpWindowStack();
                throw new UnityException($"levelName was not found in config: {levelName}");
            }
            loadLevel(level);
        }

        public static void LoadScene(LevelId levelId)
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"LoadScene {PhotonWrapper.NetworkClientState} : {currentSceneName} <- {levelId}");
            var level = findLevel(levelId);
            if (level == null)
            {
                Debug.Log($"levels {string.Join("| ", levels)}");
                WindowStack.dumpWindowStack();
                throw new UnityException($"levelId was not found in config: {levelId}");
            }
            loadLevel(level);
        }

        private static LevelIdDef findLevel(string levelName)
        {
            var level = levels.FirstOrDefault(x => !x.isExcluded && (x.levelName == levelName || x.unityName == levelName));
            return level;
        }

        private static LevelIdDef findLevel(LevelId levelId)
        {
            var level = levels.FirstOrDefault(x => !x.isExcluded && (x.levelId == levelId.levelId));
            return level;
        }

        private static void loadLevel(LevelIdDef level)
        {
            var currentSceneName = SceneManager.GetActiveScene().name;
            Debug.Log($"loadLevel {PhotonWrapper.NetworkClientState} : {currentSceneName} <- {level}");
            if (level.unityName == currentSceneName)
            {
                WindowStack.dumpWindowStack();
                throw new UnityException($"trying to load same scene twice: {level}");
            }
            if (level.isNetwork)
            {
                PhotonWrapper.LoadLevel(level.unityName);
            }
            else
            {
                SceneManager.LoadScene(level.unityName);
            }
        }
    }
}