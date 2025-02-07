using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using System.Linq;

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
        var sortedDict = from entry in _coroutinestore orderby entry.Key ascending select entry;
        _coroutinestore = sortedDict.ToDictionary(pair => pair.Key, pair => pair.Value);
        return i;
    }

    private IEnumerator CoroutineTimeout(Coroutine mainCoroutine, int coroutineKey, float timeoutTime, Action<bool> timeoutCallback)
    {
        bool? timeout = null;
        Coroutine timeoutCoroutine = StartCoroutine(WaitUntilTimeout(timeoutTime, data => timeout = data));

        yield return new WaitUntil(() => (_coroutinestore[coroutineKey] != true || timeout != null));

        if (_coroutinestore[coroutineKey] == false)
        {
            _coroutinestore.Remove(coroutineKey);
            StopCoroutine(mainCoroutine);
            Debug.LogError($"Player data operation: timeout or null.");
            timeoutCallback(true);
            yield break; //TODO: Add error handling.
        }
        else
            StopCoroutine(timeoutCoroutine);
        _coroutinestore.Remove(coroutineKey);
    }


    protected IEnumerator GetPlayerData(System.Action<PlayerData> callback)
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, callback);

        if (callback == null)
        {
            StartCoroutine(ServerManager.Instance.GetPlayerFromServer(content =>
            {
                if (content != null)
                    callback(new(content));
                else
                {
                    Debug.LogError("Could not connect to server and receive player");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => callback != null);
    }

    protected IEnumerator GetClanData(System.Action<ClanData> callback, string clanId = null)
    {
        if(clanId == null)
        {
            StartCoroutine(GetPlayerData(data => clanId = data.ClanId));
            yield return new WaitUntil(() => clanId != null);
        }

        Storefront.Get().GetClanData(clanId, callback);

        if (callback == null)
        {
            StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
            {
                if (content != null)
                    callback(new(content));
                else
                {
                    Debug.LogError("Could not connect to server and receive player");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => callback != null);
    }
}
