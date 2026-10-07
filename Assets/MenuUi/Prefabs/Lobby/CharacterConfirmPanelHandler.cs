using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using MenuUi.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterConfirmPanelHandler : MonoBehaviour
{
    [SerializeField] private Image _battleLogoImage;
    [SerializeField] private TMP_Text _matchmakingLabel;
    [SerializeField] private Button _lockCharactersButton;

    private MatchmakingType _gameType = MatchmakingType.None;

    public delegate void LockCharacters(MatchmakingType gameType);
    public static event LockCharacters OnLockCharacters;

    private void Start()
    {
        _lockCharactersButton.onClick.AddListener(LockCharactersCall);
    }

    public void SetInfo(MatchmakingType gameType)
    {
        _gameType = gameType;
        List<GameTypeInfo> gameTypeList = GameTypeReference.Instance.GetGameTypeInfos();

        foreach (GameTypeInfo gameTypeInfo in gameTypeList)
        {
            if(gameTypeInfo.matchmakingType == gameType)
            {
                _battleLogoImage.sprite = gameTypeInfo.Banner;
                _matchmakingLabel.text = gameTypeInfo.Name;
            }
        }
    }

    private void LockCharactersCall()
    {
        OnLockCharacters?.Invoke(_gameType);
    }
}
