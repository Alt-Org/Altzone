using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClanMemberPlaque : MonoBehaviour
{
    [SerializeField] private TMP_Text _rankingPositionText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _roleText;
    [SerializeField] private GameObject _rosetteObject;

    public void SetPosition(int position)
    {
        if(_rankingPositionText != null) _rankingPositionText.text = position.ToString();
    }

    public void SetName(string name)
    {
        if(_nameText != null) _nameText.text = name ?? "";
    }

    public void SetRole(string role)
    {
        if(_roleText != null) _roleText.text = string.IsNullOrEmpty(role) ? "Member" : role;
    }

    public void SetActivityRosette(bool isActive)
    {
        if(_rosetteObject != null) _rosetteObject.SetActive(isActive);
    }
}
