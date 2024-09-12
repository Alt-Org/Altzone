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
        [SerializeField]private List<Sprite> _featureSpritesPlaceholder;
        [SerializeField]private GameObject _featureButtonPrefab;
        [SerializeField]private List<Transform> _featureButtonPositions = new();
        private int _currentPageNumber = 0;
        private int _pageCount = 0;
        private Transform _characterImage;
        public void OnEnable()
        {
            _currentPageNumber = 0;
            _pageCount = _featureSpritesPlaceholder.Count / 8;
            if(_featureSpritesPlaceholder.Count % 8 != 0){
                _pageCount++;
            }
            DestroyFeatureButtons();
            InstantiateFeatureButtons();
        }
        public void LoadNextPage(){
            if(_currentPageNumber < _pageCount-1){
                _currentPageNumber++;
                DestroyFeatureButtons();
                InstantiateFeatureButtons();
            }
        }
        public void LoadPreviousPage(){
            if (_currentPageNumber > 0){
                _currentPageNumber--;
                DestroyFeatureButtons();
                InstantiateFeatureButtons();
            }
        }
        private void InstantiateFeatureButtons()
        {
            for (int i = 0; i < 8; i++)
            {
                if (i + (8*_currentPageNumber) < _featureSpritesPlaceholder.Count)
                {
                    FeatureButton featureButton = Instantiate(_featureButtonPrefab, _featureButtonPositions[i]).GetComponent<FeatureButton>();
                    featureButton._sprite = _featureSpritesPlaceholder[i+ (8*_currentPageNumber)];
                    featureButton.gameObject.GetComponent<Image>().sprite = featureButton._sprite;
                    featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                    (delegate { FeatureClicked(featureButton._sprite, featureButton._slot); });
                }
            }
        }
        private void DestroyFeatureButtons(){
            foreach(Transform pos in _featureButtonPositions){
                if(pos.childCount > 0){
                    Destroy(pos.GetChild(0).gameObject);
                }
            }
        }
        private void FeatureClicked(Sprite featureToChange, FeatureSlot slot){
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = featureToChange;
        }
    }
}
