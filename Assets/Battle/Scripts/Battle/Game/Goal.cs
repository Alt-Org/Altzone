using UnityConstants;
using UnityEngine;

using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

using TMPro;

using Altzone.Scripts.GA;

using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Battle.Game
{
    internal class Goal : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField] private GameObject _lobbyButton;
        [SerializeField] private GameObject _raidButton;
        [SerializeField] private BoxCollider2D _wallCollider;
        [SerializeField] private int _testLimit;
        [SerializeField] private int _goalNumber;
        [SerializeField] private TMP_Text _countDownText;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip[] _audioClips;
        [Header("End Screen Graphics")]
        [SerializeField] private GameObject _winGraphics;
        [SerializeField] private GameObject _lossGraphics;
        #endregion Serialized Fields

        #region Public Enums
        public enum PlayerRole
        {
            Player,
            Spectator
        }
        #endregion Public Enums

        #region Private

        #region Private - Fields
        private PlayerRole _currentRole = PlayerRole.Player;
        private float _timeLeft = 5.5f;
        private bool _countingDown = false;
        #endregion Private - Fields

        #region Private - Methods

        private void Start()
        {
            if (PhotonNetwork.CurrentRoom.Players.Count > _testLimit)
            {
                _wallCollider.isTrigger = true;
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            GameObject otherGameObject = collision.gameObject;
            if (otherGameObject.CompareTag(Tags.Ball) && PhotonNetwork.CurrentRoom.Players.Count > _testLimit && PhotonNetwork.IsMasterClient)      // && PhotonNetwork.IsMasterClient
            {
                GetComponent<PhotonView>().RPC(nameof(GoalRpc),  RpcTarget.All);
            }
        }

        private void Update()
        {
            if (_countingDown)
            {
                if(_timeLeft > 0)
                {
                    _timeLeft -= Time.deltaTime;
                    _countDownText.text = _timeLeft.ToString("F0");
                }
                else
                {
                    _countingDown = false;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LoadLevel("40-Raid");
                    }
                }
            }
        }

        #region Private - Methods - Photon RPC
        [PunRPC]
        private void GoalRpc()
        {
            if (PhotonNetwork.InRoom)
            {
                //_WallCollider.isTrigger = true;
                Player player = PhotonNetwork.LocalPlayer;
                int playerPos = PhotonBattle.GetPlayerPos(player);
                int teamNumber = PhotonBattle.GetTeamNumber(playerPos);
                Debug.Log($"team {teamNumber} pos {playerPos} {player.GetDebugLabel()}");
                _countingDown = true;

                Context.GetPlayerManager.AnalyticsReportPlayerCharacterWinOrLoss(_goalNumber switch
                {
                    PhotonBattle.TeamBetaValue => PhotonBattle.TeamAlphaValue,
                    PhotonBattle.TeamAlphaValue => PhotonBattle.TeamBetaValue,
                    _ => PhotonBattle.NoTeamValue
                });

                if (PhotonNetwork.IsMasterClient) GameAnalyticsManager.Instance.OnBattleEnd();

                if (_goalNumber != teamNumber)
                {
                    _winGraphics.SetActive(true);
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                        { {"Role", (int)PlayerRole.Player } });
                        _raidButton.SetActive(true);
                    }
                }
                else
                {
                    PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
                    { {"Role", (int)PlayerRole.Spectator } });
                    //PhotonNetwork.LeaveRoom();
                    _lossGraphics.SetActive(true);
                    //LobbyButton.SetActive(true);
                }
            }
        }
        #endregion Private - Methods - Photon RPC

        #endregion Private - Methods

        #endregion Private
    }
}
