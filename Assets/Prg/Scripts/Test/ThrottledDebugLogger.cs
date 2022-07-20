using System.Collections;
using System.Diagnostics;
using UnityEngine;

namespace Prg.Scripts.Test
{
    public class ThrottledDebugLogger
    {
        private readonly MonoBehaviour _host;
        private readonly float _samplingInterval;

        private bool _hasCoroutine;
        private string _logMessage;

        public bool IsStopped { set; get; }

        [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
        public static void Log(ThrottledDebugLogger logger, string message)
        {
            logger._logMessage = message;
            if (!logger._hasCoroutine)
            {
                logger._hasCoroutine = true;
                logger._host.StartCoroutine(logger.Logger());
            }
        }

        public ThrottledDebugLogger(MonoBehaviour host, float samplingInterval = 1.0f)
        {
            _host = host;
            _samplingInterval = samplingInterval;
        }

        private IEnumerator Logger()
        {
            var nextDebugLogTime = 0f;
            for (; !IsStopped;)
            {
                if (_logMessage != null && Time.time > nextDebugLogTime)
                {
                    Debug.Log(_logMessage);
                    _logMessage = null;
                    nextDebugLogTime = Time.time + _samplingInterval;
                }
                yield return null;
            }
        }
    }
}