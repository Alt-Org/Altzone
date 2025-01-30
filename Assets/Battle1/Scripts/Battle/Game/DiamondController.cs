using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
/*using Battle1.PhotonUnityNetworking.Code;*/
using Battle1.Scripts.Battle.Players;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Game
{
    internal class DiamondController : MonoBehaviour
    {
        [SerializeField, Tooltip("0: Speed\n1: Resistance\n2: Attack\n3: HP")] GameObject[] _diamondObjects;
        [SerializeField] BattleUIController  _battleUIController;
        [SerializeField] float _minHorizontalSpeed, _maxHorizontalSpeed;
        [SerializeField] float _minVerticalSpeed, _maxVerticalSpeed;
        [SerializeField] float _bottomBoundary;
        [SerializeField] float _topBoundary;

        public void DiamondSpawner(Vector3 spawnPoint)
        {
          /*  SpawnDiamond(spawnPoint);*/
        }

        public void OnDiamondPickup(DiamondType diamondType, BattleTeamNumber teamNumber)
        {
           /* if (!PhotonNetwork.IsMasterClient) return;*/

            int newDiamondCount = 0;
            DiamondData diamondData = GetDiamondData(diamondType);
            switch(teamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    newDiamondCount = diamondData.TeamAlphaCount + 1;
                    break;

                case BattleTeamNumber.TeamBeta:
                    newDiamondCount = diamondData.TeamBetaCount + 1;
                    break;
            }
           /* _photonView.RPC(nameof(UpdateDiamondCountRPC), RpcTarget.All, diamondType, teamNumber, newDiamondCount);*/
        }

        private class DiamondSpawnArgs
        {
            public DiamondType DiamondType;
            public Vector2 Velocity;

            public DiamondSpawnArgs(DiamondType prefabIndex, Vector2 velocity)
            {
                DiamondType = prefabIndex;
                Velocity = velocity;
            }
        }

        private class DiamondData
        {
            public int PrefabIndex;
            public int TeamAlphaCount;
            public int TeamBetaCount;

            public DiamondData(int prefabIndex)
            {
                PrefabIndex = prefabIndex;
                TeamAlphaCount = 0;
                TeamBetaCount = 0;
            }
        }

     /*   private PhotonView _photonView;*/
        private bool _rotateDiamonds;

        private DiamondData[] _diamondDataArray;
        private readonly List<DiamondSpawnArgs> _diamondSpawnList = new();

        private void Start()
        {
          /*  _photonView = GetComponent<PhotonView>();*/
            GenerateDiamondSpawnList();
          /*  GenerateDiamondDataArray();*/

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _rotateDiamonds = data.LocalPlayer.BattleTeam.TeamNumber == BattleTeamNumber.TeamBeta;
        }

        private DiamondData GetDiamondData(DiamondType diamondType)
        {
            return diamondType switch
            {
                DiamondType.DiamondSpeed      => _diamondDataArray[0],
                DiamondType.DiamondResistance => _diamondDataArray[1],
                DiamondType.DiamondAttack     => _diamondDataArray[2],
                DiamondType.DiamondHP         => _diamondDataArray[3],
                _ => null,
            };
        }

        private void GenerateDiamondSpawnList()
        {
           /* if (PhotonNetwork.IsMasterClient)
            {*/
                _diamondSpawnList.Clear();

                for (int i = 0; i < _diamondObjects.Length; i++)
                {
                    float horizontalSpeed = Random.Range(_minHorizontalSpeed, _maxHorizontalSpeed) * (Random.Range(0, 2) * 2 - 1);
                    Vector2 velocity = new Vector2(horizontalSpeed, Random.Range(_minVerticalSpeed, _maxVerticalSpeed));
                    _diamondSpawnList.Add(new((DiamondType)i, velocity));
                }

                int[] ints = new int[_diamondSpawnList.Count];
                float[] floats = new float[_diamondSpawnList.Count * 2];

                // Convert _diamondSpawnList to int and float array
                for (int i = 0; i < _diamondSpawnList.Count; i++)
                {
                    ints[i] = (int)_diamondSpawnList[i].DiamondType;
                    floats[i * 2 + 0] = _diamondSpawnList[i].Velocity.x;
                    floats[i * 2 + 1] = _diamondSpawnList[i].Velocity.y;
                }

               /* _photonView.RPC(nameof(SynchronizeDiamondSpawnListRPC), RpcTarget.Others, ints, floats);*/
            }/**/
        }

   /*     private void GenerateDiamondDataArray()
        {
            _diamondDataArray = new DiamondData[_diamondObjects.Length];
            for (int i = 0; i < _diamondObjects.Length; i++)
            {
                _diamondDataArray[i] = new DiamondData(i);
            }
        }*/

       /* [PunRPC]
        private void SynchronizeDiamondSpawnListRPC(int[] ints, float[] floats)
        {
            _diamondSpawnList.Clear();

            // Convert int and float array to _diamondSpawnList
            for (int i = 0; i < ints.Length; i++)
            {
                _diamondSpawnList.Add(new(
                    (DiamondType)ints[i],
                    new Vector2(
                        floats[i * 2 + 0],
                        floats[i * 2 + 1]
                    )
                ));
            }
        }*/

       /* [PunRPC]
        private void UpdateDiamondCountRPC(DiamondType diamondType, BattleTeamNumber teamNumber, int count)
        {
            DiamondData diamondData = GetDiamondData(diamondType);
            switch(teamNumber)
            {
                case BattleTeamNumber.TeamAlpha:
                    diamondData.TeamAlphaCount = count;
                    break;

                case BattleTeamNumber.TeamBeta:
                    diamondData.TeamBetaCount = count;
                    break;
            }
            _battleUIController.UpdateDiamondCountText(teamNumber, diamondType, count);
        }*/

     /*   private void SpawnDiamond(Vector3 spawnPoint)
        {
            Vector3 spawnPos = new Vector3(spawnPoint.x, spawnPoint.y, Random.Range(-spawnPoint.z / 2, spawnPoint.z / 2));
            SyncedFixedUpdateClock syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            int diamondDisappearUpdateNumber = syncedFixedUpdateClock.UpdateCount + syncedFixedUpdateClock.ToUpdates(10);
            foreach (DiamondSpawnArgs diamondSpawnArgs in _diamondSpawnList)
            {
                int diamondPrefabIndex = GetDiamondData(diamondSpawnArgs.DiamondType).PrefabIndex;
                GameObject diamond = Instantiate(_diamondObjects[diamondPrefabIndex], spawnPos, Quaternion.Euler(0f, 0f, _rotateDiamonds ? 180f : 0f));
                diamond.transform.parent = transform;
                diamond.SetActive(true);

                Rigidbody2D rb = diamond.GetComponent<Rigidbody2D>();

                bool isTopSide = spawnPoint.y > 0;

                // Set velocity
                rb.velocity = new Vector2(diamondSpawnArgs.Velocity.x, isTopSide ? -diamondSpawnArgs.Velocity.y : diamondSpawnArgs.Velocity.y);

                // Add DiamondMovement script to handle stopping at bottom boundary
                diamond.GetComponent<Diamond>().InitInstance(rb, _bottomBoundary, _topBoundary, isTopSide, diamondDisappearUpdateNumber, diamondSpawnArgs.DiamondType);
            }
            GenerateDiamondSpawnList();
        }*/
    }
