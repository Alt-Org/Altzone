using System.Collections;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Ui
{
    /// <summary>
    /// Utility class to visualize "slingshot" between one or two team member(s).
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    internal class SlingshotTrajectory : MonoBehaviour
    {
        [Header("Team Settings"), SerializeField] private int _teamNumber;

        [Header("Line Settings"), SerializeField] private GameObject _lineObject;
        [SerializeField] private float _lineLengthMultiplier = 2.0f;

        private PhotonView _photonView;
        private LineRenderer _line;
        private bool _playerIsOn;
        private IPlayerManager _playerManager;

        private void Awake()
        {
            Debug.Log($"{name} team {_teamNumber}");
            _photonView = PhotonView.Get(this);
            _playerManager = FindObjectOfType<PlayerManager>();
            Assert.IsNotNull(_playerManager, "_playerManager != null");
            _line = _lineObject.GetComponent<LineRenderer>();
            Assert.IsNotNull(_line, "_line != null");
            HideSling();
        }

        private void OnEnable()
        {
            this.Subscribe<UiEvents.SlingshotStart>(SlingStart);
            this.Subscribe<UiEvents.SlingshotEnd>(SlingEnd);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        #region Event handlers

        private void SlingStart(UiEvents.SlingshotStart start)
        {
            var team = start.TeamTrackers.Find(x => x.TeamNumber == _teamNumber);
            if (team == null || team.PlayerCount == 0)
            {
                return;
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{name} team {_teamNumber} found {team}");
            _photonView.RPC(nameof(SlingStartRpc), RpcTarget.All, team.TeamNumber);
        }

        private void SlingEnd(UiEvents.SlingshotEnd end)
        {
            if (!_playerIsOn)
            {
                return;
            }
            var team = end.TeamTrackers.Find(x => x.TeamNumber == _teamNumber);
            if (team == null || team.PlayerCount == 0)
            {
                return;
            }
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"{name} team {_teamNumber} found {team}");
            _photonView.RPC(nameof(SlingEndRpc), RpcTarget.All, team.TeamNumber);
        }

        #endregion

        #region Sling implementation

        private void ShowSling()
        {
            var battleTeam = _playerManager.GetBattleTeam(_teamNumber);
            if (battleTeam == null)
            {
                // No can do if team has exited.
                return;
            }
            _playerIsOn = true;
            _lineObject.SetActive(true);
            StartCoroutine(UpdateSling(battleTeam));
        }

        private void HideSling()
        {
            _playerIsOn = false;
            _line.SetPosition(0, Vector2.zero);
            _line.SetPosition(1, Vector2.zero);
            _lineObject.SetActive(false);
        }

        private IEnumerator UpdateSling(IBattleTeam battleTeam)
        {
            while (_playerIsOn)
            {
                // Because of how this is setup, the line of that shows where the ball will be shot,
                // changes according to which player will shoot the ball according to the conditions
                // set within the GetBallDropPositionAndDirection()
                battleTeam.GetBallDropPositionAndDirection(out var dropPosition, out var direction);
                _line.SetPosition(0, dropPosition);
                _line.SetPosition(1, dropPosition + (direction * _lineLengthMultiplier));
                yield return null;
            }
        }

        #endregion

        #region Photon RPC

        [PunRPC]
        private void SlingStartRpc(int teamNumber)
        {
            Assert.AreEqual(_teamNumber, teamNumber);
            ShowSling();
        }

        [PunRPC]
        private void SlingEndRpc(int teamNumber)
        {
            Assert.AreEqual(_teamNumber, teamNumber);
            HideSling();
        }

        #endregion
    }
}