using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Battle.Scripts.Ui
{
    public class ArenaStatusHandler : MonoBehaviour
    {
        [Header("TeamAreaStatus"), SerializeField] private Sprite _frozen;

        [Header("TeamAreas"), SerializeField] private GameObject _redArea;
        [SerializeField] private GameObject _blueArea;

        [Header("Duration"), SerializeField] private float _crossFadeAlphaDuration;

        private Image _redImage;
        private Image _blueImage;

        private void Awake()
        {
            _redImage = _redArea.GetComponent<Image>();
            _blueImage = _blueArea.GetComponent<Image>();
            Assert.IsNotNull(_redImage, "_redImage != null");
            Assert.IsNotNull(_blueImage, "_blueImage != null");
            if (_frozen != null)
            {
                _redImage.sprite = _frozen;
                _blueImage.sprite = _frozen;
            }
            HideArenaState();
        }

        private void OnEnable()
        {
            this.Subscribe<UiEvents.TeamActivation>(OnTeamActivation);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnTeamActivation(UiEvents.TeamActivation data)
        {
            ChangeArenaState(data.IsBallOnRedTeamArea, data.IsBallOnBlueTeamArea);
        }

        private void HideArenaState()
        {
            _redImage.CrossFadeAlpha(0f, 0f, false);
            _blueImage.CrossFadeAlpha(0f, 0f, false);
        }

        private void ChangeArenaState(bool red, bool blue)
        {
            _redImage.CrossFadeAlpha(red ? 1f : 0f, _crossFadeAlphaDuration, false);
            _blueImage.CrossFadeAlpha(blue ? 1f : 0f, _crossFadeAlphaDuration, false);
        }
    }
}