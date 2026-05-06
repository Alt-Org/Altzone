using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Raid_EndMenu : MonoBehaviour
{
    [SerializeField]
    public RectTransform collectedFurniture;
    public RectTransform content;
    public Raid_InventoryItem itemPrefab;

    // SetCollectedLoot for display when showing EndMenu
    public void SetCollectedLoot(List<GameFurniture> lootList)
    {
        for (int i=0;i<lootList.Count;i++)
        {
            Raid_InventoryItem UIItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity);
            UIItem.transform.SetParent(content);
            UIItem.transform.localScale = new Vector3(1, 1, 0);
            UIItem.SetData(lootList[i]);
        }
    }

    public void ReturnToLobby()
    {
        SceneManager.LoadScene("10-MenuUI");
    }
    //TODO: This is used for testing / debugging only, remove when releasing demo
    public void Restart() 
    {
        SceneManager.LoadScene("40-Raid");
    }
}
