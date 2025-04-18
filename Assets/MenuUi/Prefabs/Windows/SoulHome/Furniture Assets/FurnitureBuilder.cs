#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using MenuUI.Scripts.SoulHome;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

//[CreateAssetMenu(menuName = "ALT-Zone/FurnitureBuilder", fileName = "FurnitureBuilder")]
public class FurnitureBuilder : ScriptableObject
{
    [SerializeField, Header("Reference sheets")]
    private StorageFurnitureReference _furnitureReference;
    [SerializeField]
    private SoulHomeFurnitureReference _soulHomeReference;

    [SerializeField, Header("Furniture Prefab bases")]
    private GameObject _furniturePrefabBase;
    [SerializeField]
    private GameObject _trayFurniturePrefabBase;

    [SerializeField, Header("Furniture General info")]
    private string _furnitureName;
    [SerializeField]
    private string _setName;
    [SerializeField]
    private string _creatorName;
    [SerializeField]
    private string _visibleName;
    [SerializeField]
    private string _description;
    [SerializeField]
    private string _diagnoseNumber;

    [SerializeField, Header("Furniture Sprites")]
    private Sprite _furnitureFrontSprite;
    [SerializeField]
    private Sprite _furnitureRightSprite;
    [SerializeField]
    private Sprite _furnitureLeftSprite;
    [SerializeField]
    private Sprite _furnitureBackSprite;

    [SerializeField]
    private Sprite _furniturePoster;


    [SerializeField, Header("Furniture base data")]
    private FurnitureRarity _rarity;
    [SerializeField]
    private FurnitureSize _size;
    [SerializeField]
    private FurnitureSize _rotatedSize;
    [SerializeField]
    private FurniturePlacement _placement;
    [SerializeField]
    private double _weight;
    [SerializeField]
    private float _value;
    [SerializeField]
    private string _material;
    [SerializeField]
    private string _recycling;

    public void BuildFurniture()
    {
        if (Application.isPlaying)
        {
            Debug.LogError("Don't try to add furniture while the game is running.");
            return;
        }
        BaseFurniture baseFurniture = new BaseFurniture()
        {
            Name = _furnitureName+"_"+_setName,
            Rarity = _rarity,
            Size = _size,
            RotatedSize = _rotatedSize,
            Placement = _placement,
            Weight = _weight,
            Value = _value,
            Material = _material,
            Recycling = _recycling

        };

        FurnitureInfoObject furnitureObject = new FurnitureInfoObject()
        {
            Name = _furnitureName,
            Image = _furnitureFrontSprite,
            PosterImage = _furniturePoster,
            VisibleName = _visibleName,
            ArtisticDescription = _description,
            DiagnoseNumber = _diagnoseNumber,
            BaseFurniture = baseFurniture
        };

        FurnitureSetInfo furnitureSetInfo = new FurnitureSetInfo()
        {
            SetName = _setName,
            ArtistName = _creatorName,
            list = new()
            {
                furnitureObject
            }
        };

        string folderPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
        if (folderPath.Contains("."))
            folderPath = folderPath.Remove(folderPath.LastIndexOf('/'));
        Debug.Log(folderPath);

        GameObject newFurniturePrefab = (GameObject)PrefabUtility.InstantiatePrefab(_furniturePrefabBase);
        newFurniturePrefab.name = _furnitureName+"_"+_setName+"_Furniture";

        GameObject newTrayPrefab = (GameObject)PrefabUtility.InstantiatePrefab(_trayFurniturePrefabBase);
        newTrayPrefab.name = _furnitureName + "_" + _setName + "_FurnitureImage";
        newTrayPrefab.GetComponent<TrayFurniture>().FurnitureObject = newFurniturePrefab;

        GameObject obj = PrefabUtility.SaveAsPrefabAsset(newFurniturePrefab, folderPath+"/"+ newFurniturePrefab.name +".prefab");
        DestroyImmediate(newFurniturePrefab);
        GameObject obj2 = PrefabUtility.SaveAsPrefabAsset(newTrayPrefab, folderPath + "/" + newTrayPrefab.name + ".prefab");
        DestroyImmediate(newTrayPrefab);
        obj.GetComponent<FurnitureHandling>().Name = _furnitureName + "_" + _setName;
        obj2.GetComponent<TrayFurniture>().FurnitureObject = obj;
        obj.GetComponent<FurnitureHandling>().TrayFurnitureObject = obj2;

        if(_furnitureFrontSprite != null)
        {
            obj.GetComponent<SpriteRenderer>().sprite = _furnitureFrontSprite;
            obj2.GetComponent<Image>().sprite = _furnitureFrontSprite;
        }
        else if (_furnitureRightSprite != null)
        {
            obj.GetComponent<SpriteRenderer>().sprite = _furnitureRightSprite;
            obj2.GetComponent<Image>().sprite = _furnitureRightSprite;
        }
        else if (_furnitureLeftSprite != null)
        {
            obj.GetComponent<SpriteRenderer>().sprite = _furnitureLeftSprite;
            obj2.GetComponent<Image>().sprite = _furnitureLeftSprite;
        }
        else if (_furnitureBackSprite != null)
        {
            obj.GetComponent<SpriteRenderer>().sprite = _furnitureBackSprite;
            obj2.GetComponent<Image>().sprite = _furnitureBackSprite;
        }
        obj.GetComponent<FurnitureHandling>().FurnitureSpriteFront = _furnitureFrontSprite;
        obj.GetComponent<FurnitureHandling>().FurnitureSpriteRight = _furnitureRightSprite;
        obj.GetComponent<FurnitureHandling>().FurnitureSpriteLeft = _furnitureLeftSprite;
        obj.GetComponent<FurnitureHandling>().FurnitureSpriteBack = _furnitureBackSprite;

        var size = Furniture.GetFurnitureSize(_size);
        obj.GetComponent<BoxCollider2D>().size = new(size.y,size.x);
        obj.GetComponent<BoxCollider2D>().offset = new(0, (size.y/2) * (-2 *( _furnitureFrontSprite.pivot.y-0.5f)));

        bool furnitureAdded = _furnitureReference.AddFurniture(furnitureSetInfo);
        if (!furnitureAdded) return;
        _soulHomeReference.UpdateSheet();

        SoulhomeFurnitureInfoObject soulHomeFurnitureObject = new SoulhomeFurnitureInfoObject()
        {
            Name = _furnitureName,
            FurnitureHandling = obj.GetComponent<FurnitureHandling>(),
            TrayFurniture = obj2.GetComponent<TrayFurniture>()

        };

        SoulhomeFurnitureSetInfo soulHomeFurnitureSetInfo = new SoulhomeFurnitureSetInfo()
        {
            SetName = _setName,
            list = new()
            {
                soulHomeFurnitureObject
            }
        };
        _soulHomeReference.AddFurniture(soulHomeFurnitureSetInfo);
    }


}
#endif
#region Editor Code
#if UNITY_EDITOR
[CustomEditor(typeof(FurnitureBuilder))]
public class FurnitureBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        FurnitureBuilder script = (FurnitureBuilder)target;

        if (GUILayout.Button("Build Furniture"))
        {
            script.BuildFurniture();
        }
    }
}
#endif
#endregion
