using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{

    public class AvatarEditorController : MonoBehaviour
    {
        [SerializeField]private Transform _characterImageParent;
        [SerializeField]private Transform _featureButtonsParent;
        [SerializeField]private List<Sprite> _eyeSpritesPlaceholder;
        [SerializeField]private List<Sprite> _hairSpritesPlaceholder;
        [SerializeField]private GameObject _featureButtonPrefab;
        [SerializeField]private GameObject _defaultFeatureButtonPrefab;
        [SerializeField]private FeatureSlot _defaultCategory;
        [SerializeField]private List<Button> _categoryButtons;
        [SerializeField]private List<Transform> _featureButtonPositions;
        [SerializeField]private Animator animator;
        private FeatureSlot _currentlySelectedCategory;
        private List<Sprite> _currentCategorySpritesPlaceholder = new();

        private int _currentPageNumber = 0;
        private int _pageCount = 0;
        private Transform _characterImage;
        public void OnEnable()
        {
            for(int i = 0; i < _categoryButtons.Count; i++){
                int j = i;
                _categoryButtons[i].onClick.AddListener
                    (delegate {SwitchFeatureCategory((FeatureSlot)j); });
            } 



            
            SwitchFeatureCategory(_defaultCategory);
            // DestroyFeatureButtons();
            // InstantiateFeatureButtons();
        }
        public void LoadNextPage(){
            if(_currentPageNumber < _pageCount-1){
                _currentPageNumber++;
                DestroyFeatureButtons();
                InstantiateFeatureButtons();
                animator.Play("PageFlip");
                
            }
        }
        public void LoadPreviousPage(){
            if (_currentPageNumber > 0){
                _currentPageNumber--;
                DestroyFeatureButtons();
                InstantiateFeatureButtons();
                animator.Play("BackPageFlip");
            }
        }
        private void InstantiateFeatureButtons()
        {
            for (int i = 0; i < 8; i++)
            {
                if(i == 0 && _currentPageNumber == 0){
                    FeatureButton featureButton = Instantiate(_defaultFeatureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { SetDefaultFeature(featureButton._slot+3); });
                }
                else if ((i + (8*_currentPageNumber) < _currentCategorySpritesPlaceholder.Count) || 
                (i + (8*_currentPageNumber) == _currentCategorySpritesPlaceholder.Count && (_currentPageNumber != 0||i!=8)) )
                {
                    FeatureButton featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._sprite = _currentCategorySpritesPlaceholder[i+ (8*_currentPageNumber)-1];
                    featureButton._slot = _currentlySelectedCategory;
                    featureButton.gameObject.GetComponent<Image>().sprite = featureButton._sprite;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureClicked(featureButton._sprite, featureButton._slot+3); });
                }
            }
        }
        private void DestroyFeatureButtons(){
            foreach(Transform pos in _featureButtonPositions){
                if(pos.childCount > 0){
                    foreach(Transform child in pos){
                        Destroy(child.gameObject);
                    }
                }
            }
        }
        private void FeatureClicked(Sprite featureToChange, FeatureSlot slot){
            Debug.Log("switching feature in slot: " + slot.ToString());
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = featureToChange;
            _characterImage.GetChild((int)slot).GetComponent<Image>().color = new Color(255, 255, 255,255);
        }
        private void SetDefaultFeature(FeatureSlot slot){
            Debug.Log("Setting default for slot: " + slot.ToString());
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().color = new Color(255, 255, 255,0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = null;
        }
        private void SwitchFeatureCategory(FeatureSlot slot){
            _currentlySelectedCategory = slot;
            Debug.Log("Switching category into: " + _currentlySelectedCategory);

            //placeholder until available features can be read from player inventory
            switch (_currentlySelectedCategory){
                case FeatureSlot.Eyes:
                    _currentCategorySpritesPlaceholder = _eyeSpritesPlaceholder;
                    break;
                case FeatureSlot.Hair:
                    _currentCategorySpritesPlaceholder = _hairSpritesPlaceholder;
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
    }
}
