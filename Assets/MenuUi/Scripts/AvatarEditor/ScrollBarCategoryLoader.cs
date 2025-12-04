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
    [SerializeField] private AvatarDefaultReference _avatarDefaultReference;
    private List<AvatarPartInfo> _avatarPartInfo;
    private List<string> _testlist;
    // Start is called before the first frame update
    void Start()
    {
        _testlist = new List<string> { "10", "21", "22", "23", "31", "32", "33" };
        foreach (var categoryID in _testlist)
        {
            _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory(categoryID);
            AddCell(_avatarPartInfo[0].IconImage);
        }
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
