using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatWindow : MonoBehaviour
{
    private List<TMP_Dropdown.OptionData> _dropdownOptions;
    [SerializeField] private Button _changeToWindowButton;
    [SerializeField] private RectTransform _rectTransform;

    private ChatChannel _channel = new ChatChannel();

    public List<TMP_Dropdown.OptionData> DropdownOptions { get => _dropdownOptions; set => _dropdownOptions = value; }
    public Button ChangeToWindowButton { get => _changeToWindowButton; set => _changeToWindowButton = value; }
    public RectTransform RectTransform { get => _rectTransform; set => _rectTransform = value; }
    public ChatChannel Channel { get => _channel; set => _channel = value; }
}
