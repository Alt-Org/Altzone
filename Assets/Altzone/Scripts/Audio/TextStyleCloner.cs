using System.Collections;
using UnityEngine;

namespace Altzone.Scripts.Audio
{
    public class TextStyleCloner : MonoBehaviour
    {
        [SerializeField] private bool _executeOnStart = true;
        [SerializeField] private TMPro.TMP_Text _referenceText;
        [SerializeField] private TMPro.TMP_Text _targetText;

        private Color _refColor;

        private void Awake() { HideReferenceText(); }

        private void Start() { if (_executeOnStart) StartCoroutine(EndOfFrame()); }

        private void HideReferenceText()
        {
            //Color invisibleColor = new Color (0f, 0f, 0f, 0f);

            _refColor = _referenceText.color;
            _referenceText.color = new Color(0f, 0f, 0f, 0f);
        }

        public void StartCorrection(bool keepAlpha = false) { SetTextSettings(keepAlpha); }

        private IEnumerator EndOfFrame()
        {
            yield return new WaitForEndOfFrame();

            SetTextSettings(true);
        }

        private void SetTextSettings(bool keepAlpha)
        {
            if (_targetText == null) _targetText = GetComponent<TMPro.TMP_Text>();

            if (_targetText != null)
            {
                _targetText.fontSize = _referenceText.fontSize;

                if (!keepAlpha)
                    _targetText.color = _referenceText.color;
                else
                {
                    _refColor.a = _targetText.color.a;
                    _targetText.color = _refColor;
                }

                _targetText.font = _referenceText.font;
            }
            else
                Debug.LogError("_targetText was not set and it could not be found on the same object!");
        }
    }
}
