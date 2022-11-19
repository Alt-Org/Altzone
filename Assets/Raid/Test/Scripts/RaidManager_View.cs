using TMPro;
using UnityEngine;

namespace Raid.Test.Scripts
{
    public class RaidManager_View : MonoBehaviour
    {
        [SerializeField] private TMP_Text _gameTimer;
        [SerializeField] private TMP_Text _gameBackpack;
        [SerializeField] private TMP_Text _debugSeed;

        public void ResetView()
        {
            _gameTimer.text = string.Empty;
            _gameBackpack.text = string.Empty;
            _debugSeed.text = string.Empty;
        }

        public void SetGameOVer() => _gameTimer.text = $"GAME OVER";
        public void SetTimer(float seconds) => _gameTimer.text = seconds > 0 ? $"Time {seconds:0} remaining" : "Time is up";
        public void SetBackpack(float percentage) => _gameBackpack.text = percentage > 0 ? $"Backpack {percentage:0}% full" : "Backpack is empty";
        public void SetSeed(int seed) => _debugSeed.text = $"Seed {seed}";
    }
}