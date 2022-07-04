using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.UI;

namespace Battle.Scripts.Ui
{
    public class ArenaStatusHandler : MonoBehaviour
    {
        [Header("TeamAreaStatus"), SerializeField] private Sprite _frozen;

        [Header("TeamAreas"), SerializeField] private GameObject _redArea;
        [SerializeField] private GameObject _blueArea;

        [Header("Duration"), SerializeField] private float _dur;
        // The Alpha channel for the image put i

        private Image _redImg;
        private Image _blueImg;


        private void Awake()
        {
            SetSprite();
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
        
        private void SetSprite()
        {
            _redImg = _redArea.GetComponent<Image>();
            _blueImg = _blueArea.GetComponent<Image>();
            _redImg.sprite = _frozen;
            _blueImg.sprite = _frozen;
            _redImg.CrossFadeAlpha(0f, 0f, false);
            _blueImg.CrossFadeAlpha(0f, 0f, false);
        }

        private void ChangeArenaState(bool red, bool blue)
        {
            var rVal = 0f;
            var bVal = 0f;
            if(red)
            {
                rVal = 1f;
            }
            if(blue)
            {
                bVal = 1f;
            }
            _redImg.CrossFadeAlpha(rVal, _dur, false);
            _blueImg.CrossFadeAlpha(bVal, _dur, false);
        }
    }
}
