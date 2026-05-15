using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideAngelButtonScript : MonoBehaviour
{

    [SerializeField] private GameObject _speechBubble;
    [SerializeField] private TMP_Text _speechText;
    [SerializeField] private List<string> _phrasesFinnish;
    [SerializeField] private List<string> _phrasesEnglish;

    private bool isBubbleOpen;


    public void BubbleOpenerCloser()
    {
        isBubbleOpen = _speechBubble.activeSelf;
        if (isBubbleOpen)
        {
            _speechBubble.SetActive(false);
        }
        else if (!isBubbleOpen)
        {
            _speechBubble.SetActive(true);
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                _speechText.text = _phrasesFinnish[Random.Range(0,_phrasesFinnish.Count)];
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                _speechText.text = _phrasesEnglish[Random.Range(0, _phrasesEnglish.Count)];
            }
        }

    }
}
