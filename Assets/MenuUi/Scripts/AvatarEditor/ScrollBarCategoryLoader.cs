using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using Altzone.Scripts.AvatarPartsInfo;

public class ScrollBarCategoryLoader : MonoBehaviour
{
    [SerializeField] private AvatarPartsReference _avatarPartsReference;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _avatarPartCategoryGridCellPrefab;
    private GameObject _emptyCell;
    [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
    private List<AvatarPartInfo> _avatarPartInfo;
    private List<string> _allCategoryIds;
    // Start is called before the first frame update
    void Start()
    {
        _emptyCell = new GameObject("EmptyCell", typeof(RectTransform));
        Instantiate(_emptyCell, _content);
        _allCategoryIds = _avatarPartsReference.GetAllCategoryIds();

        foreach (var categoryID in _allCategoryIds)
        {
            _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryID);
            AddCell(_avatarPartInfo[1].IconImage);
        }
        Instantiate(_emptyCell, _content);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void AddCell(Sprite sprite)
    {
        GameObject _gridCell = Instantiate(_avatarPartCategoryGridCellPrefab, _content);
        Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
        _avatarPart.sprite = sprite;

    }
}
