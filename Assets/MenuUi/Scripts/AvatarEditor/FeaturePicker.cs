using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using Altzone.Scripts.ReferenceSheets;

namespace MenuUi.Scripts.AvatarEditor
{
    public class FeaturePicker : MonoBehaviour
    {
        [SerializeField] private Transform _characterImageParent;
        [SerializeField] private Transform _featureButtonsParent;
        [SerializeField] private TMP_Text _categoryText;

        [SerializeField] private AvatarPartsReference _avatarPartsReference;
        [SerializeField] private AvatarDefaultReference _avatarDefaultReference;

        [Header("Feature Buttons")]
        [SerializeField] private GameObject _featureButtonPrefab;
        [SerializeField] private GameObject _defaultFeatureButtonPrefab;
        [SerializeField] private FeatureSlot _defaultCategory;
        [SerializeField] private List<Button> _categoryButtons;
        [SerializeField] private List<Button> _pageButtons;
        [SerializeField] private List<Transform> _featureButtonPositions;
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

        private int _partsPageCount = 0;

        public void Start()
        {
            _swipeArea = GetComponent<RectTransform>();
            _categoryButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextCategory);
            _categoryButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousCategory);
            _pageButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextPage);
            _pageButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousPage);

            _partsPageCount = _avatarPartsReference.GetCategoryCount();
        }

        public void OnEnable()
        {
            _partsPageCount = _avatarPartsReference.GetCategoryCount();
            _currentlySelectedCategory = _defaultCategory;
            SwitchFeatureCategory();
            SwipeHandler.OnSwipe += OnFeaturePickerSwipe;
        }

        public void OnDisable()
        {
            DestroyFeatureButtons();
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
            DestroyRightSideButtons();
            InstantiateRightSideButtons();
            _animator.Play("PageFlip");

            yield return new WaitWhile(()=> !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

            _animator.SetTrigger("ResetToIdle");
            DestroyLeftSideButtons();
            InstantiateLeftSideButtons();
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
            DestroyLeftSideButtons();
            InstantiateLeftSideButtons();
            _animator.Play("BackPageFlip");

            yield return new WaitWhile(()=> !_animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));

            _animator.SetTrigger("ResetToIdle");
            DestroyRightSideButtons();
            InstantiateRightSideButtons();
        }

        private void InstantiateFeatureButtons()
        {
            InstantiateRightSideButtons();
            InstantiateLeftSideButtons();
        }

        private void InstantiateRightSideButtons()
        {
            for (int i = 4; i < 8; i++)
            {
                int categoryFeatureIndex = i + (8 * _currentPageNumber);

                if ((categoryFeatureIndex < _currentCategoryFeatureDataPlaceholder.Count) || 
                (categoryFeatureIndex == _currentCategoryFeatureDataPlaceholder.Count && (_currentPageNumber != 0 || i != 8)))
                {
                    Button featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();

                    if(_currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].IconImage != null)
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].IconImage;
                    else
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].AvatarImage;

                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureButtonClicked(_currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1], (int)_currentlySelectedCategory); });
                }
            }
        }

        private void InstantiateLeftSideButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                int categoryFeatureIndex = i + (8 * _currentPageNumber);

                if (i == 0 && _currentPageNumber == 0)
                {
                    Button featureButton = Instantiate(_defaultFeatureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { SetFeatureToNone((int)_currentlySelectedCategory); });
                }
                else if ((categoryFeatureIndex < _currentCategoryFeatureDataPlaceholder.Count) || 
                (categoryFeatureIndex == _currentCategoryFeatureDataPlaceholder.Count && ((_currentPageNumber != 0) || (i != 8))))
                {
                    Button featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();

                    if (_currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].IconImage != null)
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].IconImage;
                    else
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1].AvatarImage;

                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureButtonClicked(_currentCategoryFeatureDataPlaceholder[categoryFeatureIndex - 1], (int)_currentlySelectedCategory); });
                }
            }
        }

        private void DestroyFeatureButtons()
        {
            DestroyLeftSideButtons();
            DestroyRightSideButtons();
        }

        private void DestroyLeftSideButtons()
        {
            for(int i = 0; i < 4; i++)
            {
                if(_featureButtonPositions[i].childCount > 0)
                {
                    foreach(Transform child in _featureButtonPositions[i])
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void DestroyRightSideButtons()
        {
            for(int i = 4; i < 8; i++)
            {
                if(_featureButtonPositions[i].childCount > 0)
                {
                    foreach(Transform child in _featureButtonPositions[i])
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        private void FeatureButtonClicked(AvatarPartsReference.AvatarPartInfo featureToChange, int slot)
        {
            SetFeature(featureToChange, slot);
            _restoreDefaultColor?.Invoke();
        }

        private void SetFeature(AvatarPartsReference.AvatarPartInfo featureToChange, int slot)
        {
            _selectedFeatures[slot] = featureToChange.Id;
            _characterImage = _characterImageParent.GetChild(0);

            if (featureToChange.AvatarImage == null)
                _characterImage.GetChild(slot + 1).gameObject.SetActive(false);
            else
            {
                _characterImage.GetChild(slot + 1).gameObject.SetActive(true);
                _characterImage.GetChild(slot+ 1).GetComponent<Image>().sprite = featureToChange.AvatarImage;
                _characterImage.GetChild(slot + 1).GetComponent<Image>().color = new Color(255, 255, 255,255);
            }

            if(_characterClassID == CharacterClassID.Confluent){
                if (featureToChange.AvatarImage == null)
                    _characterImage.GetChild(slot + 1).GetChild(0).gameObject.SetActive(false);
                else
                {
                    _characterImage.GetChild(slot + 1).GetChild(0).gameObject.SetActive(true);
                    _characterImage.GetChild(slot + 1).GetChild(0).GetComponent<Image>().sprite = featureToChange.AvatarImage;
                    _characterImage.GetChild(slot + 1).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255, 255);
                }
            }
        }

        private void SetFeatureToNone(int slot)
        {
            //_selectedFeatures[slot] = FeatureID.None;
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild(slot + 1).GetComponent<Image>().color = new Color(255, 255, 255,0);
            _characterImage.GetChild(slot + 1).GetComponent<Image>().sprite = null;
            if(_characterClassID == CharacterClassID.Confluent){
                _characterImage.GetChild(slot + 1).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255,0);
                _characterImage.GetChild(slot + 1).GetChild(0).GetComponent<Image>().sprite = null;
            }
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
            if( (int)_currentlySelectedCategory < 0)
            {
                _currentlySelectedCategory = (FeatureSlot)(System.Enum.GetNames(typeof(FeatureSlot)).Length - 1);
            }
            SwitchFeatureCategory();
        }

        private void SwitchFeatureCategory()
        {
            //placeholder until available features can be read from player inventory
            _currentCategoryFeatureDataPlaceholder = GetSpritesByCategory(_currentlySelectedCategory);
            _currentPageNumber = 0;
            _pageCount = (_currentCategoryFeatureDataPlaceholder.Count + 1) / _partsPageCount;

            if ((_currentCategoryFeatureDataPlaceholder.Count + 1) % _partsPageCount != 0)
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
            for (int i = 0; i < features.Count; i++){
                List<AvatarPartsReference.AvatarPartInfo> currentCategoryFeatureDataPlaceholder = GetSpritesByCategory((FeatureSlot)i);

                //if(features[i] == FeatureID.Default)
                //{
                //    features[i] = ResolveCharacterDefaultFeature(i);
                //}

                //if (features[i] == FeatureID.None)
                //{
                //    SetFeatureToNone(i);
                //}
                //else
                //{
                if (currentCategoryFeatureDataPlaceholder == null)
                    return;

                if (features[i] != null && features[i] != "")
                {
                    AvatarPartsReference.AvatarPartInfo partData =
                        currentCategoryFeatureDataPlaceholder.Find(part => part.Id == features[i]);

                    if (partData != null)
                        SetFeature(partData, i);
                }
                //}
            }
        }
    }
}
