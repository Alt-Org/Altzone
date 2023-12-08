using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Text;

public class ChatServerDebug : MonoBehaviour
{
    [SerializeField] private TMP_InputField _chatIdInputField;
    [SerializeField] private TMP_InputField _chatMessageInputField;

    private string _address;
    [SerializeField] private string _chatName;

    string[] _DebugChats = { "Global", "Country 1", "Country 2", "Country 3", "Clan 1", "Clan 2", "Clan 3" };

    private void Start()
    {
        _address = ChatListener.Instance._serverAddress;
    }

    public void PostChat()
    {
        StartCoroutine(PostChatCoroutine(_chatName));
    }

    public void GetAllChats()
    {
        StartCoroutine(GetAllChatsCoroutine());
    }

    public void DeleteChat()
    {
        StartCoroutine(DeleteChatCoroutine());
    }

    public void ClearActiveChat()
    {
        StartCoroutine(ClearActiveChatCoroutine());
    }

    #region Coroutines
    private IEnumerator PostChatCoroutine(string chatName)
    {
        if(_chatName == null || _chatName.Length == 0)
            yield break;

        WWWForm data = new WWWForm();
        data.AddField("name", _chatName);

        UnityWebRequest www = UnityWebRequest.Post(_address, data);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }

    private IEnumerator GetAllChatsCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Get(_address);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Show results as text
            Debug.Log(request.downloadHandler.text);

            // Or retrieve results as binary data
            byte[] results = request.downloadHandler.data;
        }
    }

    private IEnumerator DeleteChatCoroutine()
    {
        UnityWebRequest request = UnityWebRequest.Delete(_address + _chatIdInputField.text);

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Show results as text
            Debug.Log("Success!");
        }
    }
    private IEnumerator ClearActiveChatCoroutine()
    {
        //\",\"_messages\": []
        string json = "{\"name\":\"" + ChatListener.Instance._activeChatChannel._channelName + "\",\"_id\":\"" + "656881073af653a15f1846ab" + "\"}";
        Debug.Log(json);

        var bytes = Encoding.UTF8.GetBytes(json);
        var request = UnityWebRequest.Put(_address, bytes);
        request.method = "PUT"; // Hack to send POST to server instead of PUT
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log("Form upload complete!");
        }
    }

    #endregion
}
