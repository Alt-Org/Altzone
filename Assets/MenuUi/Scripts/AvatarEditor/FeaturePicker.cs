using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using Assets.Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.AvatarPartsInfo;
namespace MenuUi.Scripts.AvatarEditor
{
    public class FeaturePicker : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
        [SerializeField] private AvatarEditorController _avatarEditorController;
        [SerializeField] private AvatarEditorFeatureButtonsHandler _featureButtonsHandler;
        [SerializeField] private Transform _characterImageParent;
        [SerializeField] private Transform _featureButtonsParent;
        [SerializeField] private TMP_Text _categoryText;
        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;

        [Header("Feature Buttons")]
        [SerializeField] private FeatureSlot _defaultCategory;
        [SerializeField] private List<Button> _categoryButtons;
        [SerializeField] private List<Button> _pageButtons;
        [SerializeField] private Animator _animator;
        [SerializeField] private Image _pageTurnImage;

        private readonly FeatureState _featureState = new();
        private readonly PageController _pageController = new();
        private readonly CategoryController _categoryController = new();
        //private readonly AnimationController _animationController = new();

        private RectTransform _swipeArea;
        private CharacterClassType _characterClassType;
        private Action _restoreDefaultColor;

        private const int FeaturesPerPage = 8;

        #region Unity Lifecycle

        private void Start()
        {
            //InitializeComponents();
            //SetupButtonListeners();
        }

        private void OnEnable()
        {
            //InitializeDefaultState();
            //SubscribeToEvents();
        }

        private void OnDisable()
        {
            //UnsubscribeFromEvents();
            //_animationController.StopAllAnimations();
            //_pageTurnImage.enabled = false;
        }

        //private void OnValidate()
        //{
        //    ValidateReferences();
        //}

        #endregion

        #region Initialization

        //private void InitializeComponents()
        //{
        //    _swipeArea = GetComponent<RectTransform>();
        //    _pageController.Initialize(FeaturesPerPage);
        //    _categoryController.Initialize();
        //    _animationController.Initialize(_animator, this);
        //}

        //private void SetupButtonListeners()
        //{
        //    if (_categoryButtons?.Count >= 2)
        //    {
        //        _categoryButtons[0].onClick.AddListener(() => _categoryController.LoadNextCategory());
        //        _categoryButtons[1].onClick.AddListener(() => _categoryController.LoadPreviousCategory());
        //    }

        //    if (_pageButtons?.Count >= 2)
        //    {
        //        _pageButtons[0].onClick.AddListener(() => _pageController.LoadPage(true));
        //        _pageButtons[1].onClick.AddListener(() => _pageController.LoadPage(false));
        //    }
        //}

        //private void InitializeDefaultState()
        //{
        //    _categoryController.SetCurrentCategory(_defaultCategory);
        //    SwitchFeatureCategory();
        //}

        //private void SubscribeToEvents()
        //{
        //    SwipeHandler.OnSwipe += OnFeaturePickerSwipe;
        //    _pageController.OnPageChanged += OnPageChanged;
        //    _categoryController.OnCategoryChanged += OnCategoryChanged;
        //}

        //private void UnsubscribeFromEvents()
        //{
        //    SwipeHandler.OnSwipe -= OnFeaturePickerSwipe;
        //    _pageController.OnPageChanged -= OnPageChanged;
        //    _categoryController.OnCategoryChanged -= OnCategoryChanged;
        //}

        #endregion

        #region Event Handlers

        //private void OnFeaturePickerSwipe(SwipeDirection direction, Vector2 start, Vector2 end)
        //{
        //    if (!RectTransformUtility.RectangleContainsScreenPoint(_swipeArea, start))
        //        return;

        //    HandleSwipeInput(direction);
        //}

        //private void OnPageChanged(int newPage, bool forward)
        //{
        //    SetFeatureButtons();
        //    StartCoroutine(_animationController.PlayPageFlipAnimation(forward, _featureButtonsHandler));
        //}

        //private void OnCategoryChanged(FeatureSlot newCategory)
        //{
        //    SwitchFeatureCategory();
        //}

        #endregion

        #region Input Handling

        //private void HandleSwipeInput(SwipeDirection direction)
        //{
        //    switch (direction)
        //    {
        //        case SwipeDirection.Left:
        //            _pageController.LoadPage(true);
        //            break;
        //        case SwipeDirection.Right:
        //            _pageController.LoadPage(false);
        //            break;
        //        case SwipeDirection.Up:
        //            _categoryController.LoadNextCategory();
        //            break;
        //        case SwipeDirection.Down:
        //            _categoryController.LoadPreviousCategory();
        //            break;
        //    }
        //}

        #endregion

        #region Feature Management

        //private void SwitchFeatureCategory()
        //{
        //    var currentCategory = _categoryController.GetCurrentCategory();
        //    var categoryFeatures = GetSpritesByCategory(currentCategory);

        //    _featureState.SetCurrentCategoryFeatures(categoryFeatures);
        //    _pageController.SetPageCount(Mathf.CeilToInt((categoryFeatures.Count + 1f) / FeaturesPerPage));
        //    _pageController.ResetToFirstPage();

        //    SetCategoryNameText(currentCategory);
        //    SetFeatureButtons();
        //}

        //private void SetFeatureButtons()
        //{
        //    var currentPage = _pageController.GetCurrentPage();
        //    var categoryFeatures = _featureState.GetCurrentCategoryFeatures();
        //    var currentCategory = _categoryController.GetCurrentCategory();

        //    for (int i = 0; i < FeaturesPerPage; i++)
        //    {
        //        if (ShouldShowNoneButton(i, currentPage))
        //        {
        //            _featureButtonsHandler.SetOnClick(SetFeatureToNone, (int)currentCategory, i);
        //            continue;
        //        }

        //        int featureIndex = CalculateFeatureIndex(i, currentPage);
        //        if (IsValidFeatureIndex(featureIndex, categoryFeatures.Count))
        //        {
        //            var feature = categoryFeatures[featureIndex - 1];
        //            _featureButtonsHandler.SetOnClick(FeatureButtonClicked, feature, (int)currentCategory, i);
        //        }
        //        else
        //        {
        //            _featureButtonsHandler.SetOff(i);
        //        }
        //    }
        //}

        //private static bool ShouldShowNoneButton(int buttonIndex, int currentPage)
        //{
        //    return buttonIndex == 0 && currentPage == 0;
        //}

        //private static int CalculateFeatureIndex(int buttonIndex, int currentPage)
        //{
        //    return buttonIndex + FeaturesPerPage * currentPage;
        //}

        //private static bool IsValidFeatureIndex(int index, int featuresCount)
        //{
        //    return index <= featuresCount;
        //}

        //private void SetFeatureToNone(int slot)
        //{
        //    _featureState.SetSelectedFeature(slot, "0");
        //    _avatarEditorController.PlayerAvatar.SortAndAssignByID("0");
        //    UpdateCharacterImage((FeatureSlot)slot, null);
        //}

        //private void FeatureButtonClicked(AvatarPartInfo feature, int slot)
        //{
        //    SetFeature(feature, slot);
        //    _restoreDefaultColor?.Invoke();

        //    if (slot == 4)
        //        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        //}

        public void SetFeature(AvatarPartInfo feature, int slot)
        {
            _featureState.SetSelectedFeature(slot, feature.Id);
            _avatarEditorController.PlayerAvatar.SortAndAssignByID(feature.Id);
            UpdateCharacterImage((FeatureSlot)slot, feature.AvatarImage);
        }

        private void UpdateCharacterImage(FeatureSlot slot, Sprite sprite)
        {
            _avatarEditorCharacterHandle.SetMainCharacterImage(slot, sprite);

            if (_characterClassType == CharacterClassType.Confluent)
            {
                _avatarEditorCharacterHandle.SetSecondaryCharacterImage(slot, sprite);
            }
            else
            {
                _avatarEditorCharacterHandle.SetSecondaryCharacterHidden();
            }
        }

        #endregion

        #region UI Updates

        //private void SetCategoryNameText(FeatureSlot category)
        //{
        //    if (_categoryText == null) return;

        //    _categoryText.text = CategoryLocalizer.GetLocalizedName(category);
        //}

        #endregion

        #region Data Access

        private List<AvatarPartInfo> GetSpritesByCategory(FeatureSlot slot)
        {
            if (_avatarPartsReference == null)
                return new List<AvatarPartInfo>();

            var categoryId = CategoryController.GetCategoryId(slot);
            return string.IsNullOrEmpty(categoryId)
                ? new List<AvatarPartInfo>()
                : _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
        }

        #endregion

        #region Public API

        public FeatureSlot GetCurrentlySelectedCategory() => _categoryController.GetCurrentCategory();
        public List<string> GetCurrentlySelectedFeatures() => _featureState.GetSelectedFeatures();

        public List<Sprite> GetCurrentlySelectedFeaturesAsSprites()
        {
            return _featureState.GetSelectedFeatures()
                .Select((id, i) => GetCurrentlySelectedFeatureSprite((AvatarPiece)i))
                .ToList();
        }

        public Sprite GetCurrentlySelectedFeatureSprite(AvatarPiece pieceSlot)
        {
            var featureData = GetSpritesByCategory((FeatureSlot)pieceSlot);
            var featureId = _featureState.GetSelectedFeature((int)pieceSlot);

            if (string.IsNullOrEmpty(featureId)) return null;

            return featureData.Find(p => p.Id == featureId)?.AvatarImage;
        }

        public void SetCharacterClassID(CharacterClassType id) => _characterClassType = id;
        //public void RestoreDefaultColorToFeature(Action restore) => _restoreDefaultColor = restore;

        public void SetLoadedFeatures(PlayerAvatar avatar)
        {
            var featureTypes = Enum.GetValues(typeof(FeatureSlot));
            foreach(FeatureSlot feature in featureTypes)
            {
                string partId = avatar.GetPartId(feature);

                if (string.IsNullOrEmpty(partId))
                {
                    _featureState.SetSelectedFeature((int)feature, "0");
                    continue;
                }

                var featureData = GetSpritesByCategory(feature);

                var part = featureData.Find(p => p.Id == partId);
                if(part == null)
                {
                    _featureState.SetSelectedFeature((int)feature, "0");
                }
                else
                {
                    SetFeature(part, (int)feature);
                }
            }
            //for (int i = 0; i < avatarFeatures.Count; i++)
            //{
            //var featureData = GetSpritesByCategory((FeatureSlot)i);
            //
            // if (string.IsNullOrEmpty(avatarFeatures[i]))
            // {
            // _featureState.SetSelectedFeature(i, "0");
            // continue;
            //   }

            //   var part = featureData.Find(p => p.Id == avatarFeatures[i]);
            // if (part != null)
            //{
            //  SetFeature(part, i);
            //  }
            //  else
            //   {
            // _featureState.SetSelectedFeature(i, "0");
            // }
            //}
        }

        #endregion

        #region Validation

        private void ValidateReferences()
        {
            if (_avatarEditorCharacterHandle == null)
                Debug.LogWarning($"Avatar editor character handle not assigned in {gameObject.name}");

            if (_featureButtonsHandler == null)
                Debug.LogWarning($"Feature buttons handler not assigned in {gameObject.name}");

            if (_avatarPartsReference == null)
                Debug.LogWarning($"Avatar parts reference not assigned in {gameObject.name}");
        }

        #endregion
    }

    #region Helper Classes

    public class FeatureState
    {
        private List<string> _selectedFeatures = new List<string>(new string[7]);
        private List<AvatarPartInfo> _currentCategoryFeatures = new();

        public List<string> GetSelectedFeatures() => new List<string>(_selectedFeatures);
        public string GetSelectedFeature(int index) =>
            index >= 0 && index < _selectedFeatures.Count ? _selectedFeatures[index] : null;

        public void SetSelectedFeature(int index, string value)
        {
            if (index >= 0 && index < _selectedFeatures.Count)
                _selectedFeatures[index] = value;
        }

        public List<AvatarPartInfo> GetCurrentCategoryFeatures() => _currentCategoryFeatures;
        public void SetCurrentCategoryFeatures(List<AvatarPartInfo> features) =>
            _currentCategoryFeatures = features ?? new List<AvatarPartInfo>();
    }

    public class PageController
    {
        private int _currentPageNumber;
        private int _pageCount;
        private int _featuresPerPage;

        public event Action<int, bool> OnPageChanged;

        public void Initialize(int featuresPerPage)
        {
            _featuresPerPage = featuresPerPage;
        }

        public void LoadPage(bool forward)
        {
            if (!CanChangePage(forward)) return;

            _currentPageNumber += forward ? 1 : -1;
            OnPageChanged?.Invoke(_currentPageNumber, forward);
        }

        private bool CanChangePage(bool forward)
        {
            return forward ? _currentPageNumber < _pageCount - 1 : _currentPageNumber > 0;
        }

        public void SetPageCount(int count) => _pageCount = count;
        public void ResetToFirstPage() => _currentPageNumber = 0;
        public int GetCurrentPage() => _currentPageNumber;
    }

    public class CategoryController
    {
        private FeatureSlot _currentCategory;

        private static readonly Dictionary<FeatureSlot, string> CategoryIds = new()
        {
            { FeatureSlot.Hair, "10" }, { FeatureSlot.Eyes, "21" }, { FeatureSlot.Nose, "22" },
            { FeatureSlot.Mouth, "23" }, { FeatureSlot.Body, "31" }, { FeatureSlot.Hands, "32" },
            { FeatureSlot.Feet, "33" }
        };

        public event Action<FeatureSlot> OnCategoryChanged;

        public void Initialize() { }

        public void LoadNextCategory() => ChangeCategory(1);
        public void LoadPreviousCategory() => ChangeCategory(-1);

        private void ChangeCategory(int delta)
        {
            var enumValues = Enum.GetValues(typeof(FeatureSlot));
            int newIndex = ((int)_currentCategory + delta + enumValues.Length) % enumValues.Length;

            SetCurrentCategory((FeatureSlot)newIndex);
        }

        public void SetCurrentCategory(FeatureSlot category)
        {
            _currentCategory = category;
            OnCategoryChanged?.Invoke(_currentCategory);
        }

        public FeatureSlot GetCurrentCategory() => _currentCategory;
        public static string GetCategoryId(FeatureSlot slot) =>
            CategoryIds.TryGetValue(slot, out var id) ? id : null;
    }

    //public class AnimationController
    //{
    //    private Animator _animator;
    //    private MonoBehaviour _coroutineRunner;
    //    private Coroutine _currentCoroutine;

    //    public void Initialize(Animator animator, MonoBehaviour coroutineRunner)
    //    {
    //        _animator = animator;
    //        _coroutineRunner = coroutineRunner;
    //    }

    //    public IEnumerator PlayPageFlipAnimation(bool forward, AvatarEditorFeatureButtonsHandler featureHandler)
    //    {
    //        if (_animator == null || !_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
    //            yield break;

    //        SetupAnimationSides(forward, featureHandler);
    //        PlayAnimation(forward);

    //        yield return new WaitWhile(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

    //        _animator.SetTrigger("ResetToIdle");
    //        CompleteAnimationSides(forward, featureHandler);
    //    }

    //    private void SetupAnimationSides(bool forward, AvatarEditorFeatureButtonsHandler featureHandler)
    //    {
    //        if (forward)
    //        {
    //            featureHandler.ShowRightSide();
    //            featureHandler.HideLeftSide();
    //        }
    //        else
    //        {
    //            featureHandler.ShowLeftSide();
    //            featureHandler.HideRightSide();
    //        }
    //    }

    //    private void PlayAnimation(bool forward)
    //    {
    //        _animator.Play(forward ? "PageFlip" : "BackPageFlip");
    //    }

    //    private void CompleteAnimationSides(bool forward, AvatarEditorFeatureButtonsHandler featureHandler)
    //    {
    //        if (forward)
    //            featureHandler.ShowLeftSide();
    //        else
    //            featureHandler.ShowRightSide();
    //    }

    //    public void StopAllAnimations()
    //    {
    //        if (_currentCoroutine != null)
    //        {
    //            _coroutineRunner.StopCoroutine(_currentCoroutine);
    //            _currentCoroutine = null;
    //        }
    //    }
    //}

    public static class CategoryLocalizer
    {
        public static string GetLocalizedName(FeatureSlot category)
        {
            return category switch
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
    }

    #endregion
}

