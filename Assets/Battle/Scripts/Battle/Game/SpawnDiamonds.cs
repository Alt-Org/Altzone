using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Photon.Pun;
using Battle.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using Battle.Scripts.Battle;

internal class SpawnDiamonds : MonoBehaviour
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
        SpawnDiamond(spawnPoint);
    }

    private class DiamondSpawnArgs
    {
        public int PrefabIndex;
        public Vector2 Velocity;

        public DiamondSpawnArgs(int prefabIndex, Vector2 velocity)
        {
            PrefabIndex = prefabIndex;
            Velocity = velocity;
        }
    }

    private bool _rotateDiamonds;

    private readonly List<DiamondSpawnArgs> _diamondSpawnList = new();

    private void Start()
    {
        GenerateDiamondSpawnList();

        this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
    }

    private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        _rotateDiamonds = data.LocalPlayer.TeamNumber == PhotonBattle.TeamBetaValue;
    }

    /*
    private void Update()
    {
        // Spawn point following the ball position
        if (_followTarget != null)
        {
            _spawnPoint.position = _followTarget.transform.position;
        }
    }
    */

    private void GenerateDiamondSpawnList()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _diamondSpawnList.Clear();

            for (int i = 0; i < _diamondObjects.Count; i++)
            {
                // Calculate random velocity
                Vector2 randomDirection = Random.insideUnitCircle.normalized;
                Vector2 velocity = randomDirection * (_diamondSpeed * 0.08f);

                _diamondSpawnList.Add(new(i, velocity));
            }

            int[] ints = new int[_diamondSpawnList.Count];
            float[] floats = new float[_diamondSpawnList.Count * 2];

            // Convert _diamondSpawnList to int and float array
            for (int i = 0; i < _diamondSpawnList.Count; i++)
            {
                ints[i] = _diamondSpawnList[i].PrefabIndex;
                floats[i * 2 + 0] = _diamondSpawnList[i].Velocity.x;
                floats[i * 2 + 1] = _diamondSpawnList[i].Velocity.y;
            }

            View.RPC(nameof(SynchronizeDiamondSpawnListRPC), RpcTarget.Others, ints, floats);
        }
    }

    [PunRPC]
    private void SynchronizeDiamondSpawnListRPC(int[] ints, float[] floats)
    {
        _diamondSpawnList.Clear();

        // Convert int and float array to _diamondSpawnList
        for (int i = 0; i < ints.Length; i++)
        {
            _diamondSpawnList.Add(new(
                ints[i],
                new Vector2(
                    floats[i * 2 + 0],
                    floats[i * 2 + 1]
                )
            ));
        }
    }

    private void SpawnDiamond(Vector3 spawnPoint)
    {
        int chance = Random.Range(0, _maxChance);

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

            foreach (DiamondSpawnArgs diamondSpawnArgs in _diamondSpawnList)
            {
                GameObject diamond = Instantiate(_diamondObjects[diamondSpawnArgs.PrefabIndex], spawnPos, Quaternion.Euler(0f, 0f, _rotateDiamonds ? 180f : 0f));
                diamond.transform.parent = transform;
                diamond.SetActive(true);

                Rigidbody2D rb = diamond.GetComponent<Rigidbody2D>();

                bool isTopSide = spawnPoint.y > 0;

                // Set velocity
                rb.velocity = diamondSpawnArgs.Velocity;

                Collider2D diamondCollider = diamond.GetComponent<Collider2D>();

                // Add DiamondMovement script to handle stopping at bottom boundary
                diamond.AddComponent<DiamondMovement>().Initialize(rb, _bottomBoundary, _topBoundary, isTopSide);
            }

            GenerateDiamondSpawnList();
        }
    }
}
