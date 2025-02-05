using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltMonoBehaviour : MonoBehaviour
{
    protected IEnumerator WaitUntilTimeout(float timeoutSeconds, System.Action<bool> callback)
    {
        yield return new WaitForSeconds(timeoutSeconds);
        callback(true);
    }

    private Dictionary<int, bool> _coroutinestore = new();

    protected IEnumerator CoroutineWithTimeout<T1>(Func<Action<T1>, IEnumerator> method, T1 para1, float timeoutTime, Action<bool?> timeoutCallback, Action<T1> methodCallback)
    {
        int coroutineKey = GetOpenCoroutineKey();

        Coroutine playerCoroutine = StartCoroutine(method(data => { para1 = data; if (para1 != null) _coroutinestore[coroutineKey] = true; }));

        bool? timeout = null;
        StartCoroutine(CoroutineTimeout(playerCoroutine, coroutineKey, timeoutTime, data => timeout = data));

        yield return new WaitUntil(() => (para1 != null || timeout != null));

        timeoutCallback(timeout);
        methodCallback(para1);
    }

    protected IEnumerator CoroutineWithTimeout<T1, T2>(Func<T1, Action<T2>, IEnumerator> method, T1 para1, T2 para2, float timeoutTime, Action<bool?> timeoutCallback, Action<T2> methodCallback)
    {
        int coroutineKey = GetOpenCoroutineKey();

        Coroutine playerCoroutine = StartCoroutine(method(para1, data => { para2 = data; if (para2 != null) _coroutinestore[coroutineKey] = true; }));

        bool? timeout = null;
        StartCoroutine(CoroutineTimeout(playerCoroutine, coroutineKey, timeoutTime, data => timeout = data));

        yield return new WaitUntil(() => (para2 != null || timeout != null));

        timeoutCallback(timeout);
        methodCallback(para2);
    }

    private int GetOpenCoroutineKey()
    {
        int i = 0;
        foreach (var item in _coroutinestore)
        {
            if (item.Key == i) i++;
            else break;
        }
        _coroutinestore.Add(i, false);
        return i;
    }

    private IEnumerator CoroutineTimeout(Coroutine mainCoroutine, int coroutineKey, float timeoutTime, Action<bool> timeoutCallback)
    {
        bool? timeout = null;
        Coroutine timeoutCoroutine = StartCoroutine(WaitUntilTimeout(timeoutTime, data => timeout = data));

        yield return new WaitUntil(() => (_coroutinestore[coroutineKey] != true || timeout != null));

        if (_coroutinestore[coroutineKey] == false)
        {
            timeoutCallback(true);
            StopCoroutine(mainCoroutine);
            _coroutinestore.Remove(coroutineKey);
            Debug.LogError($"Player data operation: timeout or null.");
            yield break; //TODO: Add error handling.
        }
        else
            StopCoroutine(timeoutCoroutine);
        _coroutinestore.Remove(coroutineKey);
    }
}
