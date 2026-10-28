using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class TextHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private List<string> _finnishPhraseList;
        [SerializeField] private List<string> _englishPhraseList;
        private List<string> _phraseList;

        public void SetRandomSpeechBubbleText()
        {
            LanguageType language = Localization.Language;

            if (language == LanguageType.Finnish)
            {
                _phraseList = _finnishPhraseList;
            }
            else if (language == LanguageType.English)
            {
                _phraseList = _englishPhraseList;
            }
            else
            {
                Debug.LogWarning($"No phraselist for language: {language}");
                return;
            }

            if (_phraseList.Count == 0)
            {
                Debug.LogError("Phrase List is empty");
                return;
            }

            int randomIndex = Random.Range(0, _phraseList.Count);
            string selectedPhrase = _phraseList[randomIndex];

            _text.text = selectedPhrase;
        }
    }
}
