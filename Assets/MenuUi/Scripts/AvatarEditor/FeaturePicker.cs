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
        [Header("feature data placeholder lists")]
        [SerializeField]private List<FeatureData> _blankHeadDataPlaceholder;
        [SerializeField]private List<FeatureData> _hairDataPlaceholder;
        [SerializeField]private List<FeatureData> _eyebrowsDataPlaceholder;
        [SerializeField]private List<FeatureData> _eyesDataPlaceholder;
        [SerializeField]private List<FeatureData> _noseDataPlaceholder;
        [SerializeField]private List<FeatureData> _mouthDataPlaceholder;
        [SerializeField]private List<FeatureData> _facialHairDataPlaceholder;
        [SerializeField]private List<FeatureData> _bodyDataPlaceholder;
        [SerializeField]private List<FeatureData> _handsDataPlaceholder;
        [SerializeField]private List<FeatureData> _feetDataPlaceholder;
        // [SerializeField]private List<Sprite> _blankHeadSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _hairSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _eyebrowsSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _eyeSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _noseSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _mouthSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _facialHairSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _bodySpritesPlaceholder;
        // [SerializeField]private List<Sprite> _handsSpritesPlaceholder;
        // [SerializeField]private List<Sprite> _feetSpritesPlaceholder;
        #endregion
        #region character defaults
        [Header("Default features of each avatar archetype")]
        List<FeatureID> __ConfluenceGirlsOneDefaults = new()
            {
            FeatureID.BlankHeadTwo,
            FeatureID.ConfluenceGirlsOneHair,
            FeatureID.ConfluenceGirlsOneEyebrows,
            FeatureID.ConfluenceGirlsOneEyes,
            FeatureID.ConfluenceGirlsOneNose,
            FeatureID.ConfluenceGirlsOneMouth,
            FeatureID.None,
            FeatureID.GrafitiArtistBody,
            FeatureID.None,
            FeatureID.None
            };
            List<FeatureID> __ConfluenceGirlsTwoDefaults = new()
            {
            FeatureID.BlankHeadTwo,
            FeatureID.ConfluenceGirlsTwoHair,
            FeatureID.ConfluenceGirlsTwoEyebrows,
            FeatureID.ConfluenceGirlsTwoEyes,
            FeatureID.ConfluenceGirlsTwoNose,
            FeatureID.ConfluenceGirlsTwoMouth,
            FeatureID.None,
            FeatureID.GrafitiArtistBody,
            FeatureID.None,
            FeatureID.None
            };
        List<FeatureID> _researcherDefaults = new()
            {
            FeatureID.BlankHeadOne,
            FeatureID.ResearcherHair,
            FeatureID.ResearcherEyebrows,
            FeatureID.ResearcherEyes,
            FeatureID.ResearcherNose,
            FeatureID.ResearcherMouth,
            FeatureID.ResearcherFacialHair,
            FeatureID.PreacherBody,
            FeatureID.None,
            FeatureID.None
            };
            List<FeatureID> _bodybuilderDefaults = new()
            {
            FeatureID.BlankHeadOne,
            FeatureID.ResearcherHair,
            FeatureID.ResearcherEyebrows,
            FeatureID.ResearcherEyes,
            FeatureID.ResearcherNose,
            FeatureID.ResearcherMouth,
            FeatureID.ResearcherFacialHair,
            FeatureID.PreacherBody,
            FeatureID.None,
            FeatureID.None
            };
            List<FeatureID> _comedianDefaults = new()
            {
            FeatureID.BlankHeadOne,
            FeatureID.ResearcherHair,
            FeatureID.ResearcherEyebrows,
            FeatureID.ResearcherEyes,
            FeatureID.ResearcherNose,
            FeatureID.ResearcherMouth,
            FeatureID.ResearcherFacialHair,
            FeatureID.PreacherBody,
            FeatureID.None,
            FeatureID.None
            };
        List<FeatureID> __grafitiArtistDefaults = new()
            {
            FeatureID.BlankHeadThree,
            FeatureID.GrafitiArtistHair,
            FeatureID.GrafitiArtistEyebrows,
            FeatureID.GrafitiArtistEyes,
            FeatureID.GrafitiArtistNose,
            FeatureID.GrafitiArtistMouth,
            FeatureID.None,
            FeatureID.GrafitiArtistBody,
            FeatureID.None,
            FeatureID.None
            };
            List<FeatureID> _overeaterDefaults = new()
            {
            FeatureID.BlankHeadTwo,
            FeatureID.OvereaterHair,
            FeatureID.OvereaterEyebrows,
            FeatureID.OvereaterEyes,
            FeatureID.OvereaterNose,
            FeatureID.OvereaterMouth,
            FeatureID.None,
            FeatureID.PreacherBody,
            FeatureID.None,
            FeatureID.None
            };
            List<FeatureID> _preacherDefaults = new()
            {
            FeatureID.BlankHeadTwo,
            FeatureID.PreacherHair,
            FeatureID.PreacherEyebrows,
            FeatureID.PreacherEyes,
            FeatureID.PreacherNose,
            FeatureID.PreacherMouth,
            FeatureID.None,
            FeatureID.PreacherBody,
            FeatureID.None,
            FeatureID.None
            };
        

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
        private List<FeatureID> _selectedFeatures = new(){
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
            FeatureID.Default,
        };
        private List<FeatureData> _currentCategoryFeatureDataPlaceholder = new();

        private int _currentPageNumber = 0;
        private int _pageCount = 0;
        private Transform _characterImage;

        private CharacterClassID _characterClassID;
        private Action _restoreDefaultColor;
        private AvatarEditorSwipe _swiper;


        public void Start()
        {
            GetComponent<AvatarEditorSwipe>().SetSwipeActions(swipeRight: LoadNextPage, swipeLeft: LoadPreviousPage, swipeUp: LoadPreviousCategory, swipeDown: LoadNextCategory);


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
                int j = i;
                if ((i + (8*_currentPageNumber) < _currentCategoryFeatureDataPlaceholder.Count) || 
                (i + (8*_currentPageNumber) == _currentCategoryFeatureDataPlaceholder.Count && (_currentPageNumber != 0||i!=8)) )
                {
                    Button featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();
                    // featureButton._sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].sprite;
                    // featureButton._slot = _currentlySelectedCategory;
                    if(_currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].iconSprite != null){
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].iconSprite;
                    }
                    else{
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].sprite;
                    }
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureButtonClicked(_currentCategoryFeatureDataPlaceholder[j+ (8*_currentPageNumber)-1], (int)_currentlySelectedCategory); });
                }
            }
        }
        private void InstantiateLeftSideButtons()
        {
            for (int i = 0; i < 4; i++)
            {
                int j = i;
                if(i == 0 && _currentPageNumber == 0){
                    Button featureButton = Instantiate(_defaultFeatureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();
                    // featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { SetFeatureToNone((int)_currentlySelectedCategory); });
                }
                else if ((i + (8*_currentPageNumber) < _currentCategoryFeatureDataPlaceholder.Count) || 
                (i + (8*_currentPageNumber) == _currentCategoryFeatureDataPlaceholder.Count && (_currentPageNumber != 0||i!=8)) )
                {
                    Button featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<Button>();
                    // featureButton._sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].sprite;
                    // featureButton._slot = _currentlySelectedCategory;
                    if(_currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].iconSprite != null){
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].iconSprite;
                    }
                    else{
                        featureButton.gameObject.GetComponent<Image>().sprite = _currentCategoryFeatureDataPlaceholder[i+ (8*_currentPageNumber)-1].sprite;
                    }
                    
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureButtonClicked(_currentCategoryFeatureDataPlaceholder[j+ (8*_currentPageNumber)-1], (int)_currentlySelectedCategory); });
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
        private void FeatureButtonClicked(FeatureData featureToChange, int slot){
            SetFeature(featureToChange, slot);
            _restoreDefaultColor?.Invoke();
        }
        private void SetFeature(FeatureData featureToChange, int slot)
        {
            
            _selectedFeatures[slot] = featureToChange.id;
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild(slot).GetComponent<Image>().sprite = featureToChange.sprite;
            _characterImage.GetChild(slot).GetComponent<Image>().color = new Color(255, 255, 255,255);
            if(_characterClassID == CharacterClassID.Confluent){
                _characterImage.GetChild(slot).GetChild(0).GetComponent<Image>().sprite = featureToChange.sprite;
                _characterImage.GetChild(slot).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255,255);
            }
            
        }
        private void SetFeatureToNone(int slot)
        {
            _selectedFeatures[slot] = FeatureID.None;
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild(slot).GetComponent<Image>().color = new Color(255, 255, 255,0);
            _characterImage.GetChild(slot).GetComponent<Image>().sprite = null;
            if(_characterClassID == CharacterClassID.Confluent){
                _characterImage.GetChild(slot).GetChild(0).GetComponent<Image>().color = new Color(255, 255, 255,0);
                _characterImage.GetChild(slot).GetChild(0).GetComponent<Image>().sprite = null;
            }
        }
        private void LoadNextCategory()
        {
            _currentlySelectedCategory++ ;
            if( (int)_currentlySelectedCategory >= Enum.GetNames(typeof(FeatureSlot)).Length ){
                _currentlySelectedCategory = 0 ;
            }
            SwitchFeatureCategory();
        }
        private void LoadPreviousCategory()
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
            _currentCategoryFeatureDataPlaceholder = GetSpritesByCategory(_currentlySelectedCategory);

            _currentPageNumber = 0;
            _pageCount = (_currentCategoryFeatureDataPlaceholder.Count+1) / 8;
            if((_currentCategoryFeatureDataPlaceholder.Count+1) % 8 != 0){
                _pageCount++;
            }

            DestroyFeatureButtons();
            InstantiateFeatureButtons();
        }

        //placeholder until available features can be read from player inventory
        private List<FeatureData> GetSpritesByCategory(FeatureSlot slot)
        {
            return slot switch
            {
                FeatureSlot.WholeHead => _blankHeadDataPlaceholder,        
                FeatureSlot.Hair => _hairDataPlaceholder,
                FeatureSlot.Eyebrows => _eyebrowsDataPlaceholder,
                FeatureSlot.Eyes => _eyesDataPlaceholder,
                FeatureSlot.Nose => _noseDataPlaceholder,
                FeatureSlot.Mouth => _mouthDataPlaceholder,
                FeatureSlot.FacialHair => _facialHairDataPlaceholder,
                FeatureSlot.Body => _bodyDataPlaceholder,
                FeatureSlot.Hands => _handsDataPlaceholder,
                FeatureSlot.Feet => _feetDataPlaceholder,
                _ => null,
            };
        }
        public FeatureSlot GetCurrentlySelectedCategory()
        {
            return _currentlySelectedCategory;
        }
        public List<FeatureID> GetCurrentlySelectedFeature()
        {
            return _selectedFeatures;
        }

        public void SetCharacterClassID(CharacterClassID id)
        {
            _characterClassID = id;
        }
        public void RestoreDefaultColorToFeature(Action restore)
        {
            _restoreDefaultColor = restore;
        }
    
        public void SetLoadedFeatures(List<FeatureID> features){
            for (int i = 0; i < features.Count; i++){
                _currentCategoryFeatureDataPlaceholder = GetSpritesByCategory((FeatureSlot)i);
                //Debug.Log("Getting sprite at index: " + (int)features[i] + ", Sprite list count is: " + _currentCategorySpritesPlaceholder.Count);
                // Debug.Log("The feature in slot " + ((FeatureSlot)i).ToString() + " is " + features[i].ToString() );

                if(features[i] == FeatureID.Default)
                {
                    features[i] = ResolveCharacterDefaultFeature(i);
                }
                if (features[i] == FeatureID.None){
                    SetFeatureToNone(i);
                }
                else{
                    FeatureData featureData = _currentCategoryFeatureDataPlaceholder.Find(x => x.id == features[i]);
                    SetFeature(featureData, i);
                }
                
                
            }
        }
        public FeatureID ResolveCharacterDefaultFeature(int slotIndex)
        {
            return _characterClassID switch
            {
                CharacterClassID.Confluent => __ConfluenceGirlsOneDefaults[slotIndex],
                CharacterClassID.Intellectualizer => _researcherDefaults[slotIndex],
                CharacterClassID.Desensitizer => _bodybuilderDefaults[slotIndex],
                CharacterClassID.Trickster => _comedianDefaults[slotIndex],
                CharacterClassID.Projector => __grafitiArtistDefaults[slotIndex],
                CharacterClassID.Retroflector => _overeaterDefaults[slotIndex],
                CharacterClassID.Obedient => _preacherDefaults[slotIndex],
                _ => FeatureID.None,
            };
        }

        
    }
}
