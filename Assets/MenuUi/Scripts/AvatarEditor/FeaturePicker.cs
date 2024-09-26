using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUi.Scripts.AvatarEditor
{

    public class FeaturePicker : MonoBehaviour
    {
        [SerializeField]private Transform _characterImageParent;
        [SerializeField]private Transform _featureButtonsParent;

        #region placeholders
        [Header("Sprite placeholder lists")]
        [SerializeField]private List<Sprite> _eyeSpritesPlaceholder;
        [SerializeField]private List<Sprite> _hairSpritesPlaceholder;
        [SerializeField]private List<Sprite> _noseSpritesPlaceholder;
        [SerializeField]private List<Sprite> _mouthSpritesPlaceholder;
        [SerializeField]private List<Sprite> _facialHairSpritesPlaceholder;
        [SerializeField]private List<Sprite> _bodySpritesPlaceholder;
        [SerializeField]private List<Sprite> _handsSpritesPlaceholder;
        [SerializeField]private List<Sprite> _feetSpritesPlaceholder;
        #endregion
        [Header("Feature Buttons")]
        [SerializeField]private GameObject _featureButtonPrefab;
        [SerializeField]private GameObject _defaultFeatureButtonPrefab;
        [SerializeField]private FeatureSlot _defaultCategory;
        [SerializeField]private List<Button> _categoryButtons;
        [SerializeField]private List<Button> _pageButtons;
        [SerializeField]private List<Transform> _featureButtonPositions;
        [SerializeField]private Animator animator;
        private FeatureSlot _currentlySelectedCategory;
        private List<Sprite> _currentCategorySpritesPlaceholder = new();

        private int _currentPageNumber = 0;
        private int _pageCount = 0;
        private Transform _characterImage;

        private CharacterClassID _characterClassID;


        public void Start()
        {
            //Placeholder buttons, will be replaced by swiping at some point
            _categoryButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextCategory);
            _categoryButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousCategory);
            _pageButtons[0].GetComponent<Button>().onClick.AddListener(LoadNextPage);
            _pageButtons[1].GetComponent<Button>().onClick.AddListener(LoadPreviousPage);
        }
        public void OnEnable()
        {
            _currentlySelectedCategory = _defaultCategory;
            SwitchFeatureCategory();
        }
        public void OnDisable()
        {
            DestroyFeatureButtons();
        }
        private void LoadNextPage()
        {
            if(_currentPageNumber < _pageCount-1 && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
                _currentPageNumber++;
                StartCoroutine(PlayNextPageAnimation());      
            }
        }
        private IEnumerator PlayNextPageAnimation()
        {
            DestroyRightSideButtons();
            InstantiateRightSideButtons();
            animator.Play("PageFlip");
            yield return new WaitWhile(()=> !animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));
            animator.SetTrigger("ResetToIdle");
            
            DestroyLeftSideButtons();
            InstantiateLeftSideButtons();
        }
        private void LoadPreviousPage()
        {
            if (_currentPageNumber > 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")){
                _currentPageNumber--;
                StartCoroutine(PlayPreviousPageAnimation());
            }
        }
        private IEnumerator PlayPreviousPageAnimation()
        {
            DestroyLeftSideButtons();
            InstantiateLeftSideButtons();
            animator.Play("BackPageFlip");
            yield return new WaitWhile(()=> !animator.GetCurrentAnimatorStateInfo(0).IsName("AnimationEnded"));
            animator.SetTrigger("ResetToIdle");
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
                if ((i + (8*_currentPageNumber) < _currentCategorySpritesPlaceholder.Count) || 
                (i + (8*_currentPageNumber) == _currentCategorySpritesPlaceholder.Count && (_currentPageNumber != 0||i!=8)) )
                {
                    FeatureButton featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._sprite = _currentCategorySpritesPlaceholder[i+ (8*_currentPageNumber)-1];
                    featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Image>().sprite = featureButton._sprite;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureClicked(featureButton._sprite, featureButton._slot+2); });
                }
            }
        }
        private void InstantiateLeftSideButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                if(i == 0 && _currentPageNumber == 0){
                    FeatureButton featureButton = Instantiate(_defaultFeatureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { SetDefaultFeature(featureButton._slot+2); });
                }
                else if ((i + (8*_currentPageNumber) < _currentCategorySpritesPlaceholder.Count) || 
                (i + (8*_currentPageNumber) == _currentCategorySpritesPlaceholder.Count && (_currentPageNumber != 0||i!=8)) )
                {
                    FeatureButton featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._sprite = _currentCategorySpritesPlaceholder[i+ (8*_currentPageNumber)-1];
                    featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Image>().sprite = featureButton._sprite;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureClicked(featureButton._sprite, featureButton._slot+2); });
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
            for(int i = 0; i < 4; i++){
                if(_featureButtonPositions[i].childCount > 0){
                    foreach(Transform child in _featureButtonPositions[i]){
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        private void DestroyRightSideButtons()
        {
            for(int i = 4; i < 8; i++){
                if(_featureButtonPositions[i].childCount > 0){
                    foreach(Transform child in _featureButtonPositions[i]){
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        private void FeatureClicked(Sprite featureToChange, FeatureSlot slot)
        {
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = featureToChange;
            _characterImage.GetChild((int)slot).GetComponent<Image>().color = new Color(255, 255, 255,255);
            if(_characterClassID == CharacterClassID.Confluent){
                _characterImage.GetChild((int)slot).GetChild(0).GetComponent<Image>().sprite = featureToChange;
                _characterImage.GetChild((int)slot).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255,255);
            }
        }
        private void SetDefaultFeature(FeatureSlot slot)
        {
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().color = new Color(255, 255, 255,0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = null;
            if(_characterClassID == CharacterClassID.Confluent){
                _characterImage.GetChild((int)slot).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255,0);
                _characterImage.GetChild((int)slot).GetChild(0).GetComponent<Image>().sprite = null;
            }
        }
        public void LoadNextCategory()
        {
            _currentlySelectedCategory++ ;
            if( (int)_currentlySelectedCategory >= Enum.GetNames(typeof(FeatureSlot)).Length ){
                _currentlySelectedCategory = 0 ;
            }
            SwitchFeatureCategory();
        }
        public void LoadPreviousCategory()
        {
            _currentlySelectedCategory--;
            if( (int)_currentlySelectedCategory < 0){
                _currentlySelectedCategory = (FeatureSlot)(Enum.GetNames(typeof(FeatureSlot)).Length-1);
            }
            SwitchFeatureCategory();
        }

        private void SwitchFeatureCategory()
        {
            //placeholder until available features can be read from player inventory
            switch (_currentlySelectedCategory){
                case FeatureSlot.Hair:
                    _currentCategorySpritesPlaceholder = _hairSpritesPlaceholder;
                    break;
                case FeatureSlot.Eyes:
                    _currentCategorySpritesPlaceholder = _eyeSpritesPlaceholder;
                    break;
                case FeatureSlot.Nose:
                    _currentCategorySpritesPlaceholder = _noseSpritesPlaceholder;
                    break;
                case FeatureSlot.Mouth:
                    _currentCategorySpritesPlaceholder = _mouthSpritesPlaceholder;
                    break;
                case FeatureSlot.FacialHair:
                    _currentCategorySpritesPlaceholder = _facialHairSpritesPlaceholder;
                    break;
                case FeatureSlot.Body:
                    _currentCategorySpritesPlaceholder = _bodySpritesPlaceholder;
                    break;
                case FeatureSlot.Hands:
                    _currentCategorySpritesPlaceholder = _handsSpritesPlaceholder;
                    break;
                case FeatureSlot.Feet:
                    _currentCategorySpritesPlaceholder = _feetSpritesPlaceholder;
                    break;
                default:
                    break;
            }

            _currentPageNumber = 0;
            _pageCount = (_currentCategorySpritesPlaceholder.Count+1) / 8;
            if((_currentCategorySpritesPlaceholder.Count+1) % 8 != 0){
                _pageCount++;
            }

            DestroyFeatureButtons();
            InstantiateFeatureButtons();
        }
        public FeatureSlot GetCurrentlySelectedCategory()
        {
            return _currentlySelectedCategory;
        }

        public void SetCharacterClassID(CharacterClassID id)
        {
            _characterClassID = id;
        }
    }
}
