using System.Collections;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class TextStyleCloner : MonoBehaviour
    {
        [SerializeField] private bool _executeOnStart = true;
        [SerializeField] private TMPro.TMP_Text _referenceText;
        [SerializeField] private TMPro.TMP_Text _targetText;

        private void Awake() { HideReferenceText(); }

        private void Start() { if (_executeOnStart) StartCoroutine(EndOfFrame()); }

        private void HideReferenceText() { _referenceText.gameObject.SetActive(false); }

        private IEnumerator EndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            SetTextSettings();
        }

        public void SetTextSettings()
        {
            if (!_targetText) _targetText = GetComponent<TMPro.TMP_Text>();

            if (_targetText)
            {
                _targetText.fontSize = _referenceText.fontSize;
                _targetText.color = _referenceText.color;
                _targetText.font = _referenceText.font;
            }
            else
                Debug.LogError("_targetText was not set and it could not be found on the same object!");
        }
    }
}
