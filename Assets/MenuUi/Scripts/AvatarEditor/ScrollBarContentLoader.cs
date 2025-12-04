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

    // Start is called before the first frame update
    void Start()
    {
        _avatarPartInfo = _avatarPartsReference.GetAvatarPartsByCategory("10");
        foreach (var part in _avatarPartInfo)
        {
            AddCell(part.IconImage);
        }
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
}
