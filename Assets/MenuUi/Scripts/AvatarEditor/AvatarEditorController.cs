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
        private Transform _characterImage;
        public void Start(){
            foreach(FeatureButton featureButton in _featureButtonsParent.GetComponentsInChildren<FeatureButton>()){
                featureButton.gameObject.GetComponent<Button>().onClick.AddListener
                (delegate{FeatureClicked(featureButton._sprite,featureButton._slot);});
            }
        }

        public void FeatureClicked(Sprite featureToChange, FeatureSlot slot){
            _characterImage = _characterImageParent.GetChild(0);
            _characterImage.GetChild((int)slot).GetComponent<Image>().sprite = featureToChange;
        }
    }
}
