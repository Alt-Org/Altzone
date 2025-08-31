using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using Assets.Altzone.Scripts.Model.Poco.Player;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeaturePicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
        [SerializeField] private AvatarEditorFeatureButtonsHandler _featureButtonsHandler;
        [SerializeField] private Transform _characterImageParent;
        [SerializeField] private Transform _featureButtonsParent;
        [SerializeField] private TMP_Text _categoryText;
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;

        [Header("Feature Buttons")]
        [SerializeField] private FeatureSlot _defaultCategory;
        [SerializeField] private List<Button> _categoryButtons; // 0 = next, 1 = previous
        [SerializeField] private List<Button> _pageButtons;     // 0 = next, 1 = previous
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _pageTurnImage;

        private FeatureSlot _currentlySelectedCategory;
        private List<string> _selectedFeatures = new List<string>(new string[7]);
        private List<AvatarPartsReference.AvatarPartInfo> _currentCategoryFeatures = new();

        private int _currentPageNumber;
        private int _pageCount;
        private CharacterClassID _characterClassID;
        private Action _restoreDefaultColor;
        private RectTransform _swipeArea;
        private Coroutine _pageCoroutine;

        private readonly Dictionary<FeatureSlot, string> _categoryIds = new()
        {
            { FeatureSlot.Hair, "10" }, { FeatureSlot.Eyes, "21" }, { FeatureSlot.Nose, "22" },
            { FeatureSlot.Mouth, "23" }, { FeatureSlot.Body, "31" }, { FeatureSlot.Hands, "32" },
            { FeatureSlot.Feet, "33" }
        };

        private const int FeaturesPerPage = 8;

        private void Start()
        {
            _swipeArea = GetComponent<RectTransform>();

            _categoryButtons[0].onClick.AddListener(LoadNextCategory);
            _categoryButtons[1].onClick.AddListener(LoadPreviousCategory);
            _pageButtons[0].onClick.AddListener(() => LoadPage(true));
            _pageButtons[1].onClick.AddListener(() => LoadPage(false));
        }

        private void OnEnable()
        {
            _currentlySelectedCategory = _defaultCategory;
            SwitchFeatureCategory();
            SwipeHandler.OnSwipe += OnFeaturePickerSwipe;
        }

        private void OnDisable()
        {
            SwipeHandler.OnSwipe -= OnFeaturePickerSwipe;
            if (_pageCoroutine != null) StopCoroutine(_pageCoroutine);
            _pageTurnImage.enabled = false;
        }

        private void OnFeaturePickerSwipe(SwipeDirection direction, Vector2 start, Vector2 end)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(_swipeArea, start)) return;

            switch (direction)
            {
                case SwipeDirection.Left: LoadPage(true); break;
                case SwipeDirection.Right: LoadPage(false); break;
                case SwipeDirection.Up: LoadNextCategory(); break;
                case SwipeDirection.Down: LoadPreviousCategory(); break;
            }
        }

        private void LoadPage(bool forward)
        {
            if (forward && _currentPageNumber >= _pageCount - 1) return;
            if (!forward && _currentPageNumber <= 0) return;
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) return;

            _currentPageNumber += forward ? 1 : -1;
            _pageCoroutine = StartCoroutine(PlayPageFlipAnimation(forward));
        }

        private IEnumerator PlayPageFlipAnimation(bool forward)
        {
            SetFeatureButtons();
            if (forward) { _featureButtonsHandler.ShowRightSide(); _featureButtonsHandler.HideLeftSide(); }
            else { _featureButtonsHandler.ShowLeftSide(); _featureButtonsHandler.HideRightSide(); }

            _animator.Play(forward ? "PageFlip" : "BackPageFlip");
            yield return new WaitWhile(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

            _animator.SetTrigger("ResetToIdle");
            if (forward) _featureButtonsHandler.ShowLeftSide();
            else _featureButtonsHandler.ShowRightSide();
        }

        private void SetFeatureButtons()
        {
            for (int i = 0; i < FeaturesPerPage; i++)
            {
                if (i == 0 && _currentPageNumber == 0)
                {
                    _featureButtonsHandler.SetOnClick(SetFeatureToNone, (int)_currentlySelectedCategory, i);
                    continue;
                }

                int index = i + FeaturesPerPage * _currentPageNumber;
                if (index <= _currentCategoryFeatures.Count)
                    _featureButtonsHandler.SetOnClick(FeatureButtonClicked, _currentCategoryFeatures[index - 1], (int)_currentlySelectedCategory, i);
                else
                    _featureButtonsHandler.SetOff(i);
            }
        }

        private void SetFeatureToNone(int slot)
        {
            _selectedFeatures[slot] = "0";
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, null);
            if (_characterClassID == CharacterClassID.Confluent)
                _avatarEditorCharacterHandle.SetSecondaryCharacterImage((FeatureSlot)slot, null);
        }

        private void FeatureButtonClicked(AvatarPartsReference.AvatarPartInfo feature, int slot)
        {
            SetFeature(feature, slot);
            _restoreDefaultColor?.Invoke();
        }

        private void SetFeature(AvatarPartsReference.AvatarPartInfo feature, int slot)
        {
            _selectedFeatures[slot] = feature.Id;
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, feature.AvatarImage);

            if (_characterClassID == CharacterClassID.Confluent)
                _avatarEditorCharacterHandle.SetSecondaryCharacterImage((FeatureSlot)slot, feature.AvatarImage);
            else
                _avatarEditorCharacterHandle.SetSecondaryCharacterHidden();
        }

        private void LoadNextCategory() => ChangeCategory(1);
        private void LoadPreviousCategory() => ChangeCategory(-1);

        private void ChangeCategory(int delta)
        {
            int newIndex = ((int)_currentlySelectedCategory + delta + Enum.GetValues(typeof(FeatureSlot)).Length) % Enum.GetValues(typeof(FeatureSlot)).Length;
            _currentlySelectedCategory = (FeatureSlot)newIndex;
            SwitchFeatureCategory();
        }

        private void SwitchFeatureCategory()
        {
            _currentCategoryFeatures = GetSpritesByCategory(_currentlySelectedCategory);
            _currentPageNumber = 0;
            _pageCount = Mathf.CeilToInt((_currentCategoryFeatures.Count + 1f) / FeaturesPerPage);
            _pageCoroutine = StartCoroutine(PlayPageFlipAnimation(true));
            SetCategoryNameText(_currentlySelectedCategory);
        }

        private void SetCategoryNameText(FeatureSlot category)
        {
            _categoryText.text = category switch
            {
                FeatureSlot.Hair => "Hiukset",
                FeatureSlot.Eyes => "Silmät",
                FeatureSlot.Nose => "Nenä",
                FeatureSlot.Mouth => "Suu",
                FeatureSlot.Body => "Keho",
                FeatureSlot.Hands => "Kädet",
                FeatureSlot.Feet => "Jalat",
                _ => "Virhe"
            };
        }

        private List<AvatarPartsReference.AvatarPartInfo> GetSpritesByCategory(FeatureSlot slot)
        {
            return _categoryIds.TryGetValue(slot, out var id)
                ? _avatarPartsReference.GetAvatarPartsByCategory(id)
                : new List<AvatarPartsReference.AvatarPartInfo>();
        }

        public FeatureSlot GetCurrentlySelectedCategory() => _currentlySelectedCategory;
        public List<string> GetCurrentlySelectedFeatures() => _selectedFeatures;

        public List<Sprite> GetCurrentlySelectedFeaturesAsSprites()
        {
            return _selectedFeatures.Select((id, i) => GetCurrentlySelectedFeatureSprite((AvatarPiece)i)).ToList();
        }

        public Sprite GetCurrentlySelectedFeatureSprite(AvatarPiece pieceSlot)
        {
            var featureData = GetSpritesByCategory((FeatureSlot)pieceSlot);
            if (string.IsNullOrEmpty(_selectedFeatures[(int)pieceSlot])) return null;
            return featureData.Find(p => p.Id == _selectedFeatures[(int)pieceSlot])?.AvatarImage;
        }

        public void SetCharacterClassID(CharacterClassID id) => _characterClassID = id;
        public void RestoreDefaultColorToFeature(Action restore) => _restoreDefaultColor = restore;

        public void SetLoadedFeatures(List<string> features)
        {
            for (int i = 0; i < features.Count; i++)
            {
                var featureData = GetSpritesByCategory((FeatureSlot)i);
                if (string.IsNullOrEmpty(features[i])) { _selectedFeatures[i] = "0"; continue; }

                var part = featureData.Find(p => p.Id == features[i]);
                if (part != null) SetFeature(part, i);
                else _selectedFeatures[i] = "0";
            }
        }
    }
}

