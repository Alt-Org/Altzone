using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.AvatarPartsInfo;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarContentLoader : MonoBehaviour
{
    [SerializeField] private AvatarPartsReference _avatarPartsReference;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _gridCellPrefab;
    [SerializeField] private Sprite _testSprite;
    private List<AvatarPartInfo> _avatarPartInfo;
    private List<string> _allAvatarCategoryIds;

    // Start is called before the first frame update
    void Start()
    {
        _allAvatarCategoryIds = _avatarPartsReference.GetAllCategoryIds();


        RefreshFeatureListItems("21");
        DestroyFeatureListItems();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnEnable()
    {

    }

    private void AddCell(Sprite sprite)
    {
        GameObject _gridCell = Instantiate(_gridCellPrefab, _content);
        Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
        _avatarPart.sprite = sprite;
    }

    private void RefreshFeatureListItems(string categoryId)
    {
        DestroyFeatureListItems();
        _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryId);
        foreach (var part in _avatarPartInfo)
        {
            AddCell(part.IconImage);
        }
    }

    private void DestroyFeatureListItems()
    {
        foreach (Transform child in _content.transform)
        {
            Destroy(child.gameObject);
        }
    }  
}
