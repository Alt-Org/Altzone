using TMPro;
using UnityEngine;

public class OnlinePlayerTextHandler : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _text;

    public void SetInfo(string text)
    {
        _text.text = text;
    }

}
