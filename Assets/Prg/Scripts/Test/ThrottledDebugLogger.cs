using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Prg.Scripts.Test
{
    public class ThrottledDebugLogger
    {
        private readonly MonoBehaviour _host;
        private readonly float _samplingInterval;
        private readonly float _idleInterval;

        private bool _hasCoroutine;
        private string _logMessage;
        private string _memberName;

        [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
        public static void Log(ThrottledDebugLogger logger, string message, [CallerMemberName] string memberName = null)
        {
            logger._logMessage = message;
            logger._memberName = memberName;
            if (!logger._hasCoroutine)
            {
                logger._hasCoroutine = true;
                logger._host.StartCoroutine(logger.Logger());
            }
        }

        public ThrottledDebugLogger(MonoBehaviour host, float samplingInterval = 1.0f, float idleInterval = 5.0f)
        {
            _host = host;
            _samplingInterval = samplingInterval;
            _idleInterval = idleInterval;
        }

        private IEnumerator Logger()
        {
            var nextDebugLogTime = 0f;
            var time = Time.time;
            var exitCoroutineTime = time + _idleInterval;
            for (;;)
            {
                time = Time.time;
                if (_logMessage != null)
                {
                    if (time > nextDebugLogTime)
                    {
                        Debug.Log($"{_memberName}:{_logMessage}");
                        _logMessage = null;
                        nextDebugLogTime = time + _samplingInterval;
                        exitCoroutineTime = time + _idleInterval;
                    }
                }
                else if (time > exitCoroutineTime)
                {
                    _hasCoroutine = false;
                    yield break;
                }
                yield return null;
            }
        }
    }
}