using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    /// <summary>
    /// Attached to loadout change button in character stats window. Handles changing the button visuals.
    /// </summary>
    [RequireComponent(typeof(Button), typeof(Image))]
    public class LoadoutButton : MonoBehaviour
    {
        [SerializeField] private StatsWindowController _controller;
        [SerializeField] private PieChartPreview _piechartPreview;
        [SerializeField] private Image _loadoutImage;
        [SerializeField] private Sprite[] _loadoutSprites;
        private Button _button;
        private int _currentLoadoutIndex = 0;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ChangeLoadout);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            _piechartPreview.UpdateChart(_controller.CurrentCharacterID);
        }

        private void ChangeLoadout()
        {
            if (_currentLoadoutIndex + 1 < _loadoutSprites.Length)
            {
                _currentLoadoutIndex++;
            }
            else
            {
                _currentLoadoutIndex = 0;
            }

            _loadoutImage.sprite = _loadoutSprites[_currentLoadoutIndex];
        }
    }
}
