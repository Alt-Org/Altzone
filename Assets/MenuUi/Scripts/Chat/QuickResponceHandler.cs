using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickResponceHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;
    [SerializeField]
    private Button _button;

    private ChatResponseObject _chatResponse;

    public ChatResponseObject ChatResponse { get => _chatResponse;}

    public Button SetData(ChatResponseObject response)
    {
        _text.text = response.Response;
        _chatResponse = response;
        return _button;
    }
}
