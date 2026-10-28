using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Lobby;
using Altzone.Scripts.Window;
using MenuUi.Scripts.Window;
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
    private Image _background;
    [SerializeField]
    private GameObject _victoryDefeatAnimationScreen;
    [SerializeField]
    private GameObject _resultPanel;
    [SerializeField]
    private GameObject _menuButtons;
    [SerializeField]
    private Button _battleStoryButton;
    [SerializeField]
    private Button _leaveButton;

    [SerializeField]
    private BattleResultHandler _resultHandler;

    [Header("End Animator"), SerializeField]
    private Animator _victoryDefeatAnimation;
    [SerializeField]
    private AnimationClip _victoryAnimationEN;
    [SerializeField]
    private AnimationClip _victoryAnimationFI;
    [SerializeField]
    private AnimationClip _defeatAnimationEN;
    [SerializeField]
    private AnimationClip _defeatAnimationFI;

    // Start is called before the first frame update
    void Start()
    {
        _background.enabled = true;
        _victoryDefeatAnimationScreen.SetActive(true);
        _resultPanel.SetActive(false);
        _menuButtons.SetActive(false);

        _battleStoryButton.onClick.AddListener(SwitchToStory);
        _leaveButton.onClick.AddListener(LeaveToMain);

        OverlayPanelCheck.Instance.ToggleOverlay(false);

        Application.quitting += EmptyDataStorage;

        bool? winner = DataCarrier.GetData<bool?>(DataCarrier.OwnBattleResult, false);

        if (winner.HasValue)
        {
            if (winner.Value)
                if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
                    && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationCategory is Altzone.Scripts.Model.Poco.Game.EducationCategoryType.Action
                && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.WinBattle)
                {
                    DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.WinBattle, "1");
                }

            if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
                && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationCategory is Altzone.Scripts.Model.Poco.Game.EducationCategoryType.Action
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationActionType == Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.PlayBattle)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationActionType.PlayBattle, "1");
            }
        }

        if (winner.HasValue) StartCoroutine(PlayAnimation(winner.Value));
        else SwitchToStory();
    }

    private void OnDestroy()
    {
        Application.quitting -= EmptyDataStorage;
    }

    private IEnumerator PlayAnimation(bool winner)
    {

        float time = PlayBattleEnd(winner);
        _resultHandler.SetBattleResult(winner);
        yield return new WaitForSeconds(time+1);
        _background.enabled = false;
        _victoryDefeatAnimationScreen.SetActive(false);
        _resultPanel.SetActive(true);
        _menuButtons.SetActive(true);
    }

    private float PlayBattleEnd(bool winner)
    {
        AnimationClip animationClip = null;
        if (winner)
        {
            _battleWinnerText.text = "Voitto";
            _battleWinnerText.color = Color.blue;

            if (Localization.Language is LanguageType.Finnish)
            {
                animationClip = _victoryAnimationFI;
            }
            else if(Localization.Language is LanguageType.English)
            {
                animationClip = _victoryAnimationEN;
            }
        }
        else
        {
            _battleWinnerText.text = "Häviö";
            _battleWinnerText.color = Color.red;

            if (Localization.Language is LanguageType.Finnish)
            {
                animationClip = _defeatAnimationFI;
            }
            else if (Localization.Language is LanguageType.English)
            {
                animationClip = _defeatAnimationEN;
            }
        }
        if (animationClip == null) return 0;

        _victoryDefeatAnimation.Play(animationClip.name);
        return animationClip.length;
    }

    private void SwitchToStory()
    {
        _battleStory.SetActive(true);
        gameObject.SetActive(false);
    }
    private void LeaveToMain()
    {
        EmptyDataStorage();
        LobbyManager.ExitBattleStory();
    }

    private void EmptyDataStorage()
    {
        DataCarrier.GetData<int>(DataCarrier.BattleWinner, suppressWarning: true);
        DataCarrier.GetData<bool?>(DataCarrier.OwnBattleResult, suppressWarning: true);
        DataCarrier.GetData<string>(DataCarrier.BattleOwnTeamName, suppressWarning: true);
        DataCarrier.GetData<string>(DataCarrier.BattleEnemyTeamName, suppressWarning: true);
    }
}
