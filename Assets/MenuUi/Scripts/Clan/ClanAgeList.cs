using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClanAgeList : MonoBehaviour
{
    [SerializeField] private GameObject _ageListItemPrefab;
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Transform _listParent;

    [SerializeField] private List<AgeIcon> _ageIcons = new List<AgeIcon>();

    [Serializable]
    private struct AgeIcon
    {
        public ClanAge age;
        public Sprite icon;
    }

    public ClanAge ClanAgeRange { get; private set; } = ClanAge.None;

    public void Initialize(ClanAge firstSelected)
    {
        ClanAgeRange = firstSelected;

        foreach (Transform child in _listParent)
        {
            Destroy(child.gameObject);
        }

        foreach (ClanAge age in Enum.GetValues(typeof(ClanAge)).Cast<ClanAge>())
        {
            if (age == ClanAge.None || age == ClanAge.Toddlers)
            {
                continue;
            }

            Sprite icon = GetAgeSprite(age);

            GameObject listItem = Instantiate(_ageListItemPrefab, _listParent);
            var toggle = listItem.GetComponent<Toggle>();
            toggle.group = _toggleGroup;

            var item = listItem.GetComponent<ClanAgeListItem>();
            item.Initialize(age, icon, age == firstSelected, isOn =>
            {
                if (isOn)
                {
                    ClanAgeRange = age;
                }
            });
        }
    }

    private Sprite GetAgeSprite(ClanAge age)
    {
        foreach (var ageIcon in _ageIcons)
        {
            if(ageIcon.age  == age) return ageIcon.icon;
        }
        return null;
    }

    public Sprite GetSelectedAgeSprite()
    {
        return GetAgeSprite(ClanAgeRange);
    }

}
