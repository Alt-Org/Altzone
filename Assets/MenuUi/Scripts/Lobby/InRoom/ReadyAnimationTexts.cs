using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class ReadyAnimationTexts : MonoBehaviour
{
    [SerializeField] private TMP_Text _text1;
    [SerializeField] private TMP_Text _text2;

    [SerializeField, FormerlySerializedAs("_textString1")] private string _textString1FI;
    [SerializeField] private string _textString1EN;
    [SerializeField, FormerlySerializedAs("_textString2")] private string _textString2FI;
    [SerializeField] private string _textString2EN;
    [SerializeField, FormerlySerializedAs("_textString3")] private string _textString3FI;
    [SerializeField] private string _textString3EN;
    [SerializeField, FormerlySerializedAs("_textString4")] private string _textString4FI;
    [SerializeField] private string _textString4EN;

    private string TextString1
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                return _textString1FI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                return _textString1EN;
            }
            else
                return _textString1FI;
        }
    }

    private string TextString2
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                return _textString2FI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                return _textString2EN;
            }
            else
                return _textString2FI;
        }
    }

    private string TextString3
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                return _textString3FI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                return _textString3EN;
            }
            else
                return _textString3FI;
        }
    }

    private string TextString4
    {
        get
        {
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish)
            {
                return _textString4FI;
            }
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English)
            {
                return _textString4EN;
            }
            else
                return _textString4FI;
        }
    }

    public void AnimationTextChange(int value)
    {
        switch (value)
        {
            case 1:
                _text1.gameObject.SetActive(true);
                _text2.gameObject.SetActive(false);
                _text1.text = TextString1;
                break;
            case 2:
                _text1.gameObject.SetActive(true);
                _text2.gameObject.SetActive(false);
                _text1.text = TextString2;
                break;
            case 3:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(true);
                _text2.text = TextString3;
                break;
            case 4:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(true);
                _text2.text = TextString4;
                break;
            case 5:
                _text1.gameObject.SetActive(false);
                _text2.gameObject.SetActive(false);
                break;
        }
    }
}
