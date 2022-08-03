using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace MenuUI.Scripts.Credits
{

     internal class CreditScroll : MonoBehaviour
    {
        public  TextMeshProUGUI text;
        public  float scrollSpeed = 10.0f;
        private RectTransform _textRectTransform;

        void Start()
        {
            _textRectTransform = text.GetComponent<RectTransform>();

            StartCoroutine("DoSomething");
           
        }

        IEnumerator DoSomething()
        {
            float width = text.preferredWidth;
            
            float scrollPosition = -1000;

            while (true)
            {
                _textRectTransform.localPosition = new Vector3(475, scrollPosition % width*3, 0);
                scrollPosition += scrollSpeed * 20 * Time.deltaTime;
                yield return null;
            }
        }
    }
}


