using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeaturePicker : MonoBehaviour
    {
        [SerializeField] private AvatarEditorCharacterHandle _avatarEditorCharacterHandle;
        [SerializeField] private AvatarEditorFeatureButtonsHandler _featureButtonsHandler;
        [Space]
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
        private FeatureSlot _currentlySelectedCategory;
        private List<string> _selectedFeatures = new()
        {
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };
        private List<AvatarPartsReference.AvatarPartInfo> _currentCategoryFeatureDataPlaceholder = new();

        private int _currentPageNumber = 0;
        private int _pageCount = 0;
        private Transform _characterImage;

        private CharacterClassID _characterClassID;
        private System.Action _restoreDefaultColor;
        private RectTransform _swipeArea;

        private readonly string _hairCategoryId = "10";
        private readonly string _eyesCategoryId = "21";
        private readonly string _noseCategoryId = "22";
        private readonly string _mouthCategoryId = "23";
        private readonly string _bodyCategoryId = "31";
        private readonly string _handsCategoryId = "32";
        private readonly string _feetCategoryId = "33";

        public void Start()
        {
            _swipeArea = GetComponent<RectTransform>();
            _categoryButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextCategory);
            _categoryButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousCategory);
            _pageButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextPage);
            _pageButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousPage);
        }

        public void OnEnable()
        {
            _currentlySelectedCategory = _defaultCategory;
            SwitchFeatureCategory();
            SwipeHandler.OnSwipe += OnFeaturePickerSwipe;
        }

        public void OnDisable()
        {
            SwipeHandler.OnSwipe -= OnFeaturePickerSwipe;
        }

        private void OnFeaturePickerSwipe(SwipeDirection direction, Vector2 swipeStartPoint, Vector2 swipeEndPoint)
        {
            Debug.Log("swipe detected!");
            if(RectTransformUtility.RectangleContainsScreenPoint(_swipeArea, swipeStartPoint))
            {
                switch(direction){
                    case SwipeDirection.Left:
                        LoadNextPage();
                        break;
                    case SwipeDirection.Right:
                        LoadPreviousPage();
                        break;
                    case SwipeDirection.Up:
                        LoadNextCategory();
                        break;
                    case SwipeDirection.Down:
                        LoadPreviousCategory();
                        break;
                    default:
                        break;
                }
            }
        }

        private void LoadNextPage()
        {
            if (_currentPageNumber < _pageCount - 1 && _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                _currentPageNumber++;
                StartCoroutine(PlayNextPageAnimation());      
            }
        }

        private IEnumerator PlayNextPageAnimation()
        {
            SetFeatureButtons();
            _featureButtonsHandler.ShowRightSide();
            _featureButtonsHandler.HideLeftSide();
            _animator.Play("PageFlip");

            yield return new WaitWhile(()=> !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

            _animator.SetTrigger("ResetToIdle");
            _featureButtonsHandler.ShowLeftSide();
        }

        private void LoadPreviousPage()
        {
            if (_currentPageNumber > 0 && _animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
            {
                _currentPageNumber--;
                StartCoroutine(PlayPreviousPageAnimation());
            }
        }

        private IEnumerator PlayPreviousPageAnimation()
        {
            SetFeatureButtons();
            _featureButtonsHandler.ShowLeftSide();
            _featureButtonsHandler.HideRightSide();
            _animator.Play("BackPageFlip");

            yield return new WaitWhile(()=> !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

            _animator.SetTrigger("ResetToIdle");
            _featureButtonsHandler.ShowRightSide();
        }

        private void SetFeatureButtons()
        {
            for (int i = 0; i < 8;  i++)
            {
                if (i == 0 && _currentPageNumber == 0)
                {
                    _featureButtonsHandler.SetOnClick(SetFeatureToNone, (int)_currentlySelectedCategory, i);
                    continue;
                }

                int categoryFeatureIndex = i + (8 * _currentPageNumber);

                if (categoryFeatureIndex <= _currentCategoryFeatureDataPlaceholder.Count)
                {
                    _featureButtonsHandler.SetOnClick(FeatureButtonClicked, _currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1], (int)_currentlySelectedCategory, i);
                    continue;
                }

                _featureButtonsHandler.SetOff(i);
            }
        }

        private void SetFeatureToNone(int slot)
        {
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, null);

            if (_characterClassID == CharacterClassID.Confluent)
                _avatarEditorCharacterHandle.SetSecondaryCharacterImage((FeatureSlot)slot, null);
        }

        private void FeatureButtonClicked(AvatarPartsReference.AvatarPartInfo featureToChange, int slot)
        {
            SetFeature(featureToChange, slot);
            _restoreDefaultColor?.Invoke();
        }

        private void SetFeature(AvatarPartsReference.AvatarPartInfo featureToChange, int slot)
        {
            _selectedFeatures[slot] = featureToChange.Id;
            _avatarEditorCharacterHandle.SetMainCharacterImage((FeatureSlot)slot, featureToChange.AvatarImage);

            if (_characterClassID == CharacterClassID.Confluent)
                _avatarEditorCharacterHandle.SetSecondaryCharacterImage((FeatureSlot)slot, featureToChange.AvatarImage);
            else
                _avatarEditorCharacterHandle.SetSecondaryCharacterHidden();
        }

        private void LoadNextCategory()
        {
            _currentlySelectedCategory++ ;

            if((int)_currentlySelectedCategory >= System.Enum.GetNames(typeof(FeatureSlot)).Length)
                _currentlySelectedCategory = 0;

            SwitchFeatureCategory();
        }

        private void LoadPreviousCategory()
        {
            _currentlySelectedCategory--;

            if((int)_currentlySelectedCategory < 0)
                _currentlySelectedCategory = (FeatureSlot)(System.Enum.GetNames(typeof(FeatureSlot)).Length - 1);

            SwitchFeatureCategory();
        }

        private void SwitchFeatureCategory()
        {
            //placeholder until available features can be read from player inventory
            _currentCategoryFeatureDataPlaceholder = GetSpritesByCategory(_currentlySelectedCategory);
            _currentPageNumber = 0;
            _pageCount = (_currentCategoryFeatureDataPlaceholder.Count + 1) / 8;

            if (((_currentCategoryFeatureDataPlaceholder.Count + 1) % 8) != 0)
                _pageCount++;

            StartCoroutine(PlayNextPageAnimation());
            SetCategoryNameText(_currentlySelectedCategory);
        }

        private void SetCategoryNameText(FeatureSlot category){
            string name = category switch
            {
                FeatureSlot.Hair => "Hiukset",
                FeatureSlot.Eyes => "Silmät",
                FeatureSlot.Nose => "Nenä",
                FeatureSlot.Mouth => "Suu",
                FeatureSlot.Body => "Keho",
                FeatureSlot.Hands => "Kädet",
                FeatureSlot.Feet => "Jalat",
                _ => "Virhe",
            };
            _categoryText.text = name;
        }

        private List<AvatarPartsReference.AvatarPartInfo> GetSpritesByCategory(FeatureSlot slot)
        {
            return slot switch
            {
                FeatureSlot.Hair => _avatarPartsReference.GetAvatarPartsByCategory(_hairCategoryId),
                FeatureSlot.Eyes => _avatarPartsReference.GetAvatarPartsByCategory(_eyesCategoryId),
                FeatureSlot.Nose => _avatarPartsReference.GetAvatarPartsByCategory(_noseCategoryId),
                FeatureSlot.Mouth => _avatarPartsReference.GetAvatarPartsByCategory(_mouthCategoryId),
                FeatureSlot.Body => _avatarPartsReference.GetAvatarPartsByCategory(_bodyCategoryId),
                FeatureSlot.Hands => _avatarPartsReference.GetAvatarPartsByCategory(_handsCategoryId),
                FeatureSlot.Feet => _avatarPartsReference.GetAvatarPartsByCategory(_feetCategoryId),
                _ => null,
            };
        }

        public FeatureSlot GetCurrentlySelectedCategory()
        {
            return _currentlySelectedCategory;
        }

        public List<string> GetCurrentlySelectedFeatures()
        {
            return _selectedFeatures;
        }

        public void SetCharacterClassID(CharacterClassID id)
        {
            _characterClassID = id;
        }

        public void RestoreDefaultColorToFeature(System.Action restore)
        {
            _restoreDefaultColor = restore;
        }
    
        public void SetLoadedFeatures(List<string> features)
        {
            for (int i = 0; i < features.Count; i++)
            {
                List<AvatarPartsReference.AvatarPartInfo> currentCategoryFeatureDataPlaceholder = GetSpritesByCategory((FeatureSlot)i);

                if (currentCategoryFeatureDataPlaceholder == null)
                    return;

                if (features[i] != null && features[i] != "")
                {
                    AvatarPartsReference.AvatarPartInfo partData =
                        currentCategoryFeatureDataPlaceholder.Find(part => part.Id == features[i]);

                    if (partData != null)
                        SetFeature(partData, i);
                }
            }
        }
    }
}
