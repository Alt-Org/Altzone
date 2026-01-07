using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.AvatarEditor
{
    public class TextHandler : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private List<string> _phraseList;

        public void SetRandomSpeechBubbleText()
        {
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
