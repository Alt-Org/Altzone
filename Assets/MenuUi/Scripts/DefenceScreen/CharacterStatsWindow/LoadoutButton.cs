using MenuUi.Scripts.DefenceScreen.CharacterGallery;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

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
        private bool _firstTimeInitializing = true;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(ChangeLoadout);
            _controller.OnStatUpdated += UpdateChart;
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveAllListeners();
        }

        private void OnEnable()
        {
            // The first time CharacterStatsWindowView is opened initializing the chart doesn't work in OnEnable but has to be done in Start.
            if (_firstTimeInitializing == false)
            {
                InitializeChart();
            }
        }

        private void Start()
        {
            InitializeChart();
            _firstTimeInitializing = false;
        }

        private void InitializeChart()
        {
            if (_controller.IsCurrentCharacterLocked()) // If current character is locked setting the base stats to the preview and disabling button.
            {
                int impactForce = _controller.GetBaseStat(StatType.Attack);
                int hp = _controller.GetBaseStat(StatType.Hp);
                int defence = _controller.GetBaseStat(StatType.Defence);
                int characterSize = _controller.GetBaseStat(StatType.CharacterSize);
                int speed = _controller.GetBaseStat(StatType.Speed);

                _piechartPreview.UpdateChart(impactForce, hp, defence, characterSize, speed);
                _button.enabled = false;
            }
            else
            {
                _piechartPreview.UpdateChart(_controller.CurrentCharacterID);
                _button.enabled = true;
            }

            // always setting loadout button to the first one for now, but when loadouts can get implemented it needs to be fetched from saved data.
            _loadoutImage.sprite = _loadoutSprites[0];
        }

        private void UpdateChart(StatType statType)
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
