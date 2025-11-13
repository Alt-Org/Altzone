using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Window;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleEndController : MonoBehaviour
{
    [SerializeField]
    private GameObject _battleStory;
    [SerializeField]
    private TextMeshProUGUI _battleWinnerText;
    [SerializeField]
    private GameObject _menuButtons;
    [SerializeField]
    private Button _battleStoryButton;
    [SerializeField]
    private Button _leaveButton;

    [Header("End Animator"), SerializeField]
    private Animator _victoryDefeatAnimation;
    [SerializeField]
    private AnimationClip _victoryAnimation;
    [SerializeField]
    private AnimationClip _defeatAnimation;

    // Start is called before the first frame update
    void Start()
    {
        _menuButtons.SetActive(false);

        _battleStoryButton.onClick.AddListener(SwitchToStory);
        _leaveButton.onClick.AddListener(LeaveToMain);

        bool? winner = DataCarrier.GetData<bool?>(DataCarrier.BattleWinner, false);

        if (winner.HasValue)
        {
            var dtListenerBattle = gameObject.GetComponent<DailyTaskProgressListenerBattle>();
            if (winner.Value) dtListenerBattle.WinBattle();
            dtListenerBattle.PlayBattle();
        }

        if (winner.HasValue) StartCoroutine(PlayAnimation(winner.Value));
        else SwitchToStory();
    }

    private IEnumerator PlayAnimation(bool winner)
    {

        float time = PlayBattleEnd(winner);
        yield return new WaitForSeconds(time);
        _menuButtons.SetActive(true);
    }

    private float PlayBattleEnd(bool winner)
    {
        if (winner)
        {
            _battleWinnerText.text = "Voitto";
            _battleWinnerText.color = Color.blue;
            _victoryDefeatAnimation.Play(_victoryAnimation.name);
            return _victoryAnimation.length;
        }
        else
        {
            _battleWinnerText.text = "Häviö";
            _battleWinnerText.color = Color.red;
            _victoryDefeatAnimation.Play(_defeatAnimation.name);
            return _defeatAnimation.length;
        }
    }

    private void SwitchToStory()
    {
        _battleStory.SetActive(true);
        gameObject.SetActive(false);
    }
    private void LeaveToMain()
    {
        DataCarrier.GetData<bool?>(DataCarrier.BattleWinner, suppressWarning: true);
        LobbyManager.ExitBattleStory();
    }
}
