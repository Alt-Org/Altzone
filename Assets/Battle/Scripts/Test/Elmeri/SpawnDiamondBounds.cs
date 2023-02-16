using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 
public class SpawnDiamondBounds : MonoBehaviour 
{
    [SerializeField] GameObject Diamond;
    [SerializeField] Transform SpawnPoints;

    //private GameObject[] SpawnPointsArray;
    private List<float> SpawnPointsArray = new List<float>();

    public Vector3 center;
    public Vector3 size;
    private float SpawnY;

    void Start()
    {
        foreach (Transform t in SpawnPoints)
        { 
            SpawnPointsArray.Add(t.position.y);
            //t.gameObject.SetActive(true);
        }
        StartCoroutine(SpawnDiamond());
    }

    public IEnumerator SpawnDiamond()
    {
        SpawnY = Random.Range (0, SpawnPointsArray.Count);

        yield return new WaitForSeconds(Random.Range(5f, 10f));
        Vector3 pos = center + new Vector3(0, SpawnY, Random.Range(-size.z/2, size.z/2));
        var DiamondParent = GameObject.Instantiate(Diamond, pos, Quaternion.Euler (0f, 0f, 90f));   // transform.TransformPoint(pos)
        DiamondParent.transform.parent = transform;
        StartCoroutine(SpawnDiamond());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(center, size);
    }
}