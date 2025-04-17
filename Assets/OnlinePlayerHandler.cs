using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;

public class OnlinePlayerHandler : AltMonoBehaviour
{
    [SerializeField]
    private GameObject _textPrefab;
    [SerializeField]
    private Transform _content;
    // Start is called before the first frame update
    void Start()
    {
        ServerManager.OnOnlinePlayersChanged += BuildOnlinePlayerList;
    }

    private void OnEnable()
    {
        BuildOnlinePlayerList(ServerManager.Instance.OnlinePlayers);
    }

    private void OnDestroy()
    {
        ServerManager.OnOnlinePlayersChanged -= BuildOnlinePlayerList;
    }

    private void BuildOnlinePlayerList(List<string> list)
    {
        foreach(Transform t in _content)
        {
            Destroy(t);
        }
        StartCoroutine(GetClanData(data =>
        {
            if (data == null)
            {
                Debug.LogError("Unable to find ClanData.");
                return;
            }
            foreach (string name in list)
            {
                string newName = data.Members.FirstOrDefault(x => x.Id == name)?.Name;
                if (newName == null) continue;
                GameObject textObject = Instantiate(_textPrefab, _content);
                textObject.GetComponent<OnlinePlayerTextHandler>().SetInfo(newName);
            }
        }));
    }
}
