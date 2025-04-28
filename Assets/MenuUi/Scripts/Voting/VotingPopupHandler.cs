using System.Collections;
using Altzone.Scripts.Voting;
using TMPro;
using UnityEngine;

/// <summary>
/// Handles displaying the voting popup in UIOverlayPanel.
/// Works very similarly to the DailyTaskProgressPopup script.
/// </summary>
public class VotingPopupHandler : MonoBehaviour
{
    [SerializeField] private GameObject _popupGameObject;
    [SerializeField] private RectTransform _popupVisibleLocation;
    [SerializeField] private RectTransform _popupHiddenLocation;
    [SerializeField] private TMP_Text _popupText;
    [Space]
    [Tooltip("Minimum time for the popup container to stay in hidden position.")]
    [SerializeField] private float _popupShowCooldown = 5f;
    [Tooltip("The time it takes for the popup window to move between positions.")]
    [SerializeField] private float _popupMoveTime = 0.75f;
    [Tooltip("How long the popup conatiner stays in visible position.")]
    [SerializeField] private float _popupStopTime = 2.5f;
    [Tooltip("Used to animate how the popup window moves between positions.")]
    [SerializeField] private AnimationCurve _popupAnimationCurve;

    private bool _popupActive = false;
    private Coroutine _coroutineMovePopup = null;
    private Coroutine _coroutineCooldownPopup = null;
    private int _phase = 1;

    private void Start()
    {
        Reset();
    }

    private void OnEnable()
    {
        PollManager.ShowVotingPopup += ShowVotingPopup;
    }

    private void OnDisable()
    {
        PollManager.ShowVotingPopup -= ShowVotingPopup;
        Reset();
    }

    private void OnDestroy()
    {
        PollManager.ShowVotingPopup -= ShowVotingPopup;
    }

    private void Reset()
    {
        _popupActive = false;
        _popupText.text = "";
        _coroutineMovePopup = null;
        _coroutineCooldownPopup = null;
        _phase = 1;
        _popupGameObject.transform.position = _popupHiddenLocation.position;
        _popupGameObject.SetActive(false);

        if (_coroutineMovePopup != null)
            StopCoroutine(_coroutineMovePopup);
    }

    private void ShowVotingPopup(FurniturePollType pollType)
    {
        if (_popupActive) return;

        switch (pollType)
        {
            case FurniturePollType.Buying:
                _popupText.text = "Uusi osto-‰‰nestys k‰ynniss‰";
                break;
            case FurniturePollType.Selling:
                _popupText.text = "Uusi myynti-‰‰nestys k‰ynniss‰";
                break;
        }

        _popupActive = true;
        _coroutineMovePopup = StartCoroutine(MoveVotingPopup());
    }

    /// <summary>
    /// Moves the voting popup from hidden position to visible position and back to hidden once a specified time has elapsed.
    /// </summary>
    private IEnumerator MoveVotingPopup()
    {
        _phase = 1;

        _popupGameObject.SetActive(true);

        while (_phase < 4)
        {
            float timer = 0.0f;
            float curve = 0.0f;

            if (_phase == 1 || _phase == 3)
            {
                while (timer < _popupMoveTime)
                {
                    if (_phase == 1)
                        curve = _popupAnimationCurve.Evaluate(timer / _popupMoveTime);
                    else
                        curve = _popupAnimationCurve.Evaluate(1.0f - (timer / _popupMoveTime));

                    _popupGameObject.transform.position = Vector3.Lerp(_popupHiddenLocation.position, _popupVisibleLocation.position, curve);

                    timer += Time.deltaTime;
                    yield return null;
                }
            }
            else if (_phase == 2)
            {
                while (timer < _popupStopTime)
                {
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            _phase++;
            yield return null;
        }

        _popupGameObject.SetActive(false);

        if (_popupActive) _popupActive = false;
    }
}
