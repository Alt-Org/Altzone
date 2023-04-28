using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
 
public class SpawnDiamondBounds : MonoBehaviour 
{
    [SerializeField] GameObject DiamondObject;
    [SerializeField] GameObject DiamondObject2;
    [SerializeField] GameObject Diamond;
    [SerializeField] Transform SpawnPoints;
    [SerializeField] Transform SpawnPoint;
    [SerializeField] float SpawnSpace;

    [SerializeField] float MinSpawnTime;
    [SerializeField] float MaxSpawnTime;
    private int Change;
    [SerializeField] int MaxChange;

    //private GameObject[] SpawnPointsArray;
    public List<float> SpawnPointsArray = new List<float>();

    public Vector3 center;
    public Vector3 size;
    public int SpawnY;
    public int PlayerLimit = 4;
    public bool StartBool;  //true
    public PhotonView View;

    void Start()
    {
        View = transform.GetComponent<PhotonView>();
        int i = 1;
        foreach (Transform t in SpawnPoints)
        { 
            if (t != SpawnPoint)
            {
                t.position = new Vector2(t.position.x, SpawnPoint.position.y - SpawnSpace*i); 
                i = i + 1;
            }
            SpawnPointsArray.Add(t.position.y);
            //t.gameObject.SetActive(true);
        }
        if (PhotonNetwork.IsMasterClient)   
        {
            StartCoroutine(SpawnDiamond());
        }
    }

    public IEnumerator SpawnDiamond()
    {
        yield return new WaitForSeconds(Random.Range(MinSpawnTime, MaxSpawnTime));
        SpawnY = Random.Range(0, SpawnPointsArray.Count);
        Change = Random.Range(0, MaxChange);
        View.RPC("DiamondRPC",  RpcTarget.All, SpawnY, Change);
    }

    [PunRPC]
    private void DiamondRPC(int SpawnY, int Change)
    {
        if (StartBool == true)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Length >= PlayerLimit)
            {
                StartBool = false;
            }
        }
        if (StartBool == false)
        {
            if (Change < MaxChange - 1)
            {
                Diamond = DiamondObject;
            }
            if (Change == MaxChange - 1)
            {
                Diamond = DiamondObject2;
            }
            Vector3 pos = new Vector3(SpawnPoint.position.x, SpawnPointsArray[SpawnY], Random.Range(-size.z/2, size.z/2));  //pos = center + new vector3(center.x, )...
            var DiamondParent = GameObject.Instantiate(Diamond, pos, Quaternion.Euler (0f, 0f, 90f));   // transform.TransformPoint(pos)
            DiamondParent.transform.parent = transform;
            DiamondParent.SetActive(true);
        }
        
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnDiamond());
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawCube(center, size);
    }
}