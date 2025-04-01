using System.Collections;
using TMPro;
using UnityEngine;

namespace QuantumUser.Scripts.UI.Views
{
    /// <summary>
    /// Handles setting timer text since match started.
    /// </summary>
    public class GameUiTimerHandler : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;
        bool _recordTime = false;
        float _matchTimeSeconds = 0;

        private void Update()
        {
            if (_recordTime)
            {
                _matchTimeSeconds += Time.deltaTime;
            }
        }

        private IEnumerator UpdateText()
        {
            while (_recordTime)
            {
                int minutes = Mathf.FloorToInt(_matchTimeSeconds / 60);
                _timerText.text = $"{minutes}:{Mathf.Floor(_matchTimeSeconds):00}";
                yield return new WaitForSeconds(1);
            }
        }

        public void StartTimer()
        {
            _recordTime = true;
            StartCoroutine(UpdateText());
            _timerText.gameObject.SetActive(true);
        }

        public void StopTimer()
        {
            _recordTime = false;
        }
    }
}

