using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class SpawnDiamonds : MonoBehaviour
{
    [SerializeField] Transform _spawnPoint;
    [SerializeField] GameObject _followTarget;
    [SerializeField] float _minSpawnTime;
    [SerializeField] float _maxSpawnTime;
    [SerializeField] List<GameObject> _diamondObjects;
    [SerializeField] float _diamondSpeed;
    [SerializeField] float _bottomBoundary;
    [SerializeField] float _topBoundary;
    [SerializeField] int _maxChance;

    public int PlayerLimit = 4;
    public bool StartBool;
    public PhotonView View;

    public void DiamondSpawner(Vector3 spawnPoint)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            SpawnDiamond(spawnPoint);
        }
    }

    private void Update()
    {
        // Spawn point following the ball position
        if (_followTarget != null)
        {
            _spawnPoint.position = _followTarget.transform.position;
        }
    }

    public void SpawnDiamond(Vector3 spawnPoint)
    {
        int chance = Random.Range(0, _maxChance);
        View.RPC("DiamondRPC", RpcTarget.All, chance, spawnPoint);
    }

    [PunRPC]
    private void DiamondRPC(int chance, Vector3 spawnPoint)
    {
        if (StartBool)
        {
            if (GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Length >= PlayerLimit)
            {
                StartBool = false;
            }
        }

        if (!StartBool)
        {
            Vector3 spawnPos = new Vector3(spawnPoint.x, spawnPoint.y, Random.Range(-spawnPoint.z / 2, spawnPoint.z / 2));

            foreach (GameObject diamondPrefab in _diamondObjects)
            {
                var diamond = Instantiate(diamondPrefab, spawnPos, Quaternion.Euler(0f, 0f, 90f));
                diamond.transform.parent = transform;
                diamond.SetActive(true);

                Rigidbody2D rb = diamond.GetComponent<Rigidbody2D>();

                bool isTopSide = spawnPoint.y > 0;

                // Set random velocity
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                rb.velocity = randomDirection * _diamondSpeed * 0.08f;

                Collider2D diamondCollider = diamond.GetComponent<Collider2D>();

                // Add DiamondMovement script to handle stopping at bottom boundary
                diamond.AddComponent<DiamondMovement>().Initialize(rb, _bottomBoundary, _topBoundary, isTopSide);
            }
        }
    }
}
