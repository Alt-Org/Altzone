using System.Collections;
using System.Collections.ObjectModel;
using UnityEngine;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game; 

public class FurnitureTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(TestFurnitureLoad());
    }

    private IEnumerator TestFurnitureLoad()
    {
        var store = Storefront.Get(); 
        ReadOnlyCollection<GameFurniture> allItems = null;

        
        yield return store.GetAllGameFurnitureYield(result => allItems = result);

        if (allItems != null && allItems.Count > 0)
        {
            Debug.Log($"Loaded {allItems.Count} furniture items.");
            foreach (var item in allItems)
            {
                var info = item.FurnitureInfo;
                Debug.Log($"🪑 {info.VisibleName} - Value: {item.Value}");
            }
        }
        else
        {
            Debug.LogWarning("No furniture data found or failed to load.");
        }
    }
}
