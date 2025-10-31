using System;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AvatarEditorFeatureButtonsHandler : MonoBehaviour
{
    [SerializeField] private TMP_Text _xText;
    [SerializeField] private List<Button> _slots;

    public void SetOff(int buttonSlot)
    {
        _slots[buttonSlot].GetComponent<Image>().enabled = false;

        if (buttonSlot == 0)
            _xText.enabled = false;
    }

    /// <summary>
    /// Used for feature button indexes 1 to 7.
    /// </summary>
    public void SetOnClick(Action<AvatarPartInfo, int> method, AvatarPartInfo avatarPartInfo, int characterSlot, int buttonSlot)
    {
        if (_slots[buttonSlot].onClick != null)
            _slots[buttonSlot].onClick.RemoveAllListeners();

        if (buttonSlot == 0)
            _xText.enabled = false;

        _slots[buttonSlot].GetComponent<Image>().enabled = true;
        _slots[buttonSlot].GetComponent<Image>().sprite = avatarPartInfo.IconImage;
        _slots[buttonSlot].onClick.AddListener(() => method(avatarPartInfo, characterSlot));
    }

    /// <summary>
    /// Used for feature button index 0 named "Clear feature" button or X.
    /// </summary>
    public void SetOnClick(Action<int> method, int characterSlot, int buttonSlot)
    {
        if (_xText.GetComponent<Button>().onClick != null)
            _xText.GetComponent<Button>().onClick.RemoveAllListeners();

        _xText.enabled = true;

        _slots[buttonSlot].GetComponent<Image>().enabled = false;
        _xText.GetComponent<Button>().onClick.AddListener(() => method(characterSlot));
    }

    /// <summary>
    /// Used for feature button indexes 1 to 7 to set the character color.
    /// </summary>
    public void SetOnClick(Action<Color> method, Sprite image, Color color, int buttonSlot)
    {
        if (_slots[buttonSlot].onClick != null)
            _slots[buttonSlot].onClick.RemoveAllListeners();

        if (buttonSlot == 0)
            _xText.enabled = false;

        _slots[buttonSlot].GetComponent<Image>().enabled = true;
        _slots[buttonSlot].GetComponent<Image>().sprite = image;
        _slots[buttonSlot].GetComponent<Image>().color = color;
        _slots[buttonSlot].onClick.AddListener(() => method(color));
    }

    public void HideLeftSide()
    {
        _slots[0].enabled = false;
        _slots[1].enabled = false;
        _slots[2].enabled = false;
        _slots[3].enabled = false;

        _slots[0].gameObject.SetActive(false);
        _slots[1].gameObject.SetActive(false);
        _slots[2].gameObject.SetActive(false);
        _slots[3].gameObject.SetActive(false);
    }

    public void HideRightSide()
    {
        _slots[4].enabled = false;
        _slots[5].enabled = false;
        _slots[6].enabled = false;
        _slots[7].enabled = false;

        _slots[4].gameObject.SetActive(false);
        _slots[5].gameObject.SetActive(false);
        _slots[6].gameObject.SetActive(false);
        _slots[7].gameObject.SetActive(false);
    }

    public void ShowLeftSide()
    {
        _slots[0].enabled = true;
        _slots[1].enabled = true;
        _slots[2].enabled = true;
        _slots[3].enabled = true;

        _slots[0].gameObject.SetActive(true);
        _slots[1].gameObject.SetActive(true);
        _slots[2].gameObject.SetActive(true);
        _slots[3].gameObject.SetActive(true);
    }

    public void ShowRightSide()
    {
        _slots[4].enabled = true;
        _slots[5].enabled = true;
        _slots[6].enabled = true;
        _slots[7].enabled = true;

        _slots[4].gameObject.SetActive(true);
        _slots[5].gameObject.SetActive(true);
        _slots[6].gameObject.SetActive(true);
        _slots[7].gameObject.SetActive(true);
    }
}
