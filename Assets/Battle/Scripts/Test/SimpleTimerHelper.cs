using System.Collections;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle.Scripts.Test
{
    public class SimpleTimerHelper : MonoBehaviour
    {
        [SerializeField] private TMP_Text _timerText;

        private double _startTime;
        private bool _isStopped;

        private void Awake()
        {
            ResetTimer();
        }

        public void ResetTimer()
        {
            _timerText.text = string.Empty;
        }

        public void StartTimer()
        {
            StopAllCoroutines();
            _startTime = PhotonNetwork.Time;
            _isStopped = false;
            FormatTime(0);
            StartCoroutine(TimerRoutine());
        }

        public void StopTimer()
        {
            _isStopped = true;
        }

        private void FormatTime(int seconds)
        {
            var minutes = seconds / 60;
            seconds -= minutes * 60;
            _timerText.text = $"{minutes}:{seconds:00}";
        }

        private IEnumerator TimerRoutine()
        {
            var delay = new WaitForSeconds(0.1f);
            while (!_isStopped)
            {
                yield return delay;
                if (_isStopped || !PhotonNetwork.InRoom)
                {
                    yield break;
                }
                FormatTime((int)(PhotonNetwork.Time - _startTime));
            }
        }
    }
}