using System.Collections;
using UnityEngine;

public class TextSizeCloner : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _referenceText;
    [SerializeField] private TMPro.TMP_Text _targetText;
    [SerializeField] private bool _hideReferenceInGame = true;
    
    private void Start() { StartCoroutine(EndOfFrame()); }

    private IEnumerator EndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        if (_targetText != null)
        {
            _targetText.fontSize = _referenceText.fontSize;
            _targetText.color = _referenceText.color;
            _targetText.font = _referenceText.font;
        }
        else
        {
            TMPro.TMP_Text tMPText = GetComponent<TMPro.TMP_Text>();

            if (tMPText != null)
            {
                tMPText.fontSize = _referenceText.fontSize;
                tMPText.color = _referenceText.color;
                tMPText.font = _referenceText.font;
            }
        }

        if (_hideReferenceInGame) _referenceText.gameObject.SetActive(false);
    }
}
