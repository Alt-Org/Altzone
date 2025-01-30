using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanCreateView : MonoBehaviour
{
    [SerializeField] private TMP_InputField _clanNameInputField;
    [SerializeField] private TMP_InputField _clanTagInputField;
    [SerializeField] private TMP_InputField _gameCoinsInputField;
    [SerializeField] private Button _openClanButton;
    [SerializeField] private Button _returnToMainClanViewButton;

    private void Reset()
    {
        StopAllCoroutines();
        _clanNameInputField.text = "";
        _clanTagInputField.text = "";
        _gameCoinsInputField.text = "";
        _openClanButton.onClick.Invoke();
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameInputField.text;
        string clanTag = _clanTagInputField.text;
        int gameCoins = int.Parse(_gameCoinsInputField.text);
        bool isOpen = !_openClanButton.interactable;

        if (clanName == string.Empty || clanTag == string.Empty || _gameCoinsInputField.text == string.Empty)
            return;

        /*StartCoroutine(ServerManager.Instance.PostClanToServer(clanName, clanTag, isOpen, null,ClanAge.None,Goals.None,null,Language.None, clan =>
        {
            if (clan == null)
            {
                return;
            }

            _returnToMainClanViewButton.onClick.Invoke();
        }));*/
    }
}
