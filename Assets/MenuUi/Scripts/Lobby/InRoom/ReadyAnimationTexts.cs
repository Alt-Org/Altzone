using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ReadyAnimationTexts : MonoBehaviour
{
    [SerializeField] private TMP_Text _text1;
    [SerializeField] private TMP_Text _text2;

    [SerializeField] private string _textString1;
    [SerializeField] private string _textString2;
    [SerializeField] private string _textString3;
    [SerializeField] private string _textString4;


    public void AnimationTextChange(int value)
    {
        switch (value)
        {
            case 1:
                _text1.gameObject.SetActive(true);
                _text2.gameObject.SetActive(false);
                _text1.text = _textString1;
                break;
            case 2:
                _text1.gameObject.SetActive(true);
                _text2.gameObject.SetActive(false);
                _text1.text = _textString2;
                break;
            case 3:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(true);
                _text2.text = _textString3;
                break;
            case 4:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(true);
                _text2.text = _textString4;
                break;
            case 5:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(false);
                break;
        }
    }
}
