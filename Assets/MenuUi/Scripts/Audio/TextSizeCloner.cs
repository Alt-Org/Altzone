using System.Collections;
using UnityEngine;

public class TextSizeCloner : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text _referenceText;
    [SerializeField] private TMPro.TMP_Text _targetText;

    
    private void Start() { StartCoroutine(EndOfFrame()); }

    private IEnumerator EndOfFrame()
    {
        yield return new WaitForEndOfFrame();

        _targetText.fontSize = _referenceText.fontSize;
        Debug.LogError(_referenceText.fontSize);
    }
}
