using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarContentLoader : MonoBehaviour
{
    private AvatarPartsReference _avatarPartsReference;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _gridCellPrefab;
    [SerializeField] private Sprite _testSprite;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnEnable()
    {
        for (int i = 0; i < 14; i++)
        {
            AddCell(_testSprite);
        }
    }

    private void AddCell(Sprite sprite)
    {
        GameObject _gridCell = Instantiate(_gridCellPrefab, _content);
        Image _avatarPart = _gridCell.transform.Find("FeatureImage").GetComponent<Image>();
        _avatarPart.sprite = sprite;

    }
}
