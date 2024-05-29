using System;
using System.Diagnostics;
using System.Globalization;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Util
{
    /// <summary>
    ///  Simple timer based on <c>Stopwatch</c> for simple execution time measurement.<br />
    /// Default implementation is "one shot" and it must be stopped before querying elapsed time.
    /// </summary>
    public class Timer : IDisposable
    {
        private readonly Stopwatch _stopwatch;
        private readonly bool _isOneShot;

#if UNITY_EDITOR
        public double TotalMinutes
        {
            get
            {
                Assert.IsFalse(_isOneShot && _stopwatch.IsRunning);
                return _stopwatch.Elapsed.TotalMinutes;
            }
        }

        public double TotalSeconds
        {
            get
            {
                Assert.IsFalse(_isOneShot && _stopwatch.IsRunning);
                return _stopwatch.Elapsed.TotalSeconds;
            }
        }

        public double TotalMilliseconds
        {
            get
            {
                Assert.IsFalse(_isOneShot && _stopwatch.IsRunning);
                return _stopwatch.Elapsed.TotalMilliseconds;
            }
        }
#else
        public double TotalMinutes => _stopwatch.Elapsed.TotalMinutes;
        public double TotalSeconds => _stopwatch.Elapsed.TotalSeconds;
        public double TotalMilliseconds => _stopwatch.Elapsed.TotalMilliseconds;
#endif

        public string ElapsedTime
        {
            get
            {
                var totalMilliseconds = TotalMilliseconds;
                if (totalMilliseconds < 10.0)
                {
                    return totalMilliseconds.ToString("0.### ms", CultureInfo.InvariantCulture);
                }
                if (totalMilliseconds < 1000.0)
                {
                    return totalMilliseconds.ToString("0 ms", CultureInfo.InvariantCulture);
                }
                var totalSeconds = TotalSeconds;
                if (totalSeconds < 60)
                {
                    return Math.Round(totalSeconds, 2).ToString("0.## s", CultureInfo.InvariantCulture);
                }
                return Math.Round(TotalMinutes, 2).ToString("0.## m", CultureInfo.InvariantCulture);
            }
        }

        public Timer(bool isOneShot = true)
        {
            _stopwatch = Stopwatch.StartNew();
            _isOneShot = isOneShot;
        }

        public Timer Stop()
        {
            Assert.IsTrue(_stopwatch.IsRunning);
            _stopwatch.Stop();
            return this;
        }

        public void Dispose()
        {
            if (_stopwatch.IsRunning)
            {
                _stopwatch.Stop();
            }
        }
    }
}
