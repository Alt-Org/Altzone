using Math = System.Math;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using Photon.Pun;
using UnityEngine;

public class SlingControllerTest : MonoBehaviour
{
    [SerializeField] private BallHandlerTest _ball;
    [SerializeField] private double _slingDelaySec;
    [SerializeField] private float _startingDistance;
    [SerializeField] private int _defaultSpeed;

    private PhotonView _photonView;

    private class Team
    {
        public List<Transform> List = new();
        public Transform FrontPlayer;
        public Transform BackPlayer;
        public float DistanceSquared;
        public Vector3 LaunchVector;
    };
    private Team[] _teams;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(SlingRpc), RpcTarget.All, PhotonNetwork.Time + _slingDelaySec);
        }

        StartCoroutine(nameof(InitializeTeams));
    }

    private IEnumerator InitializeTeams()
    {
        yield return new WaitForSeconds(1.0f);

        _teams = new Team[2];
        _teams[0] = new Team();
        _teams[1] = new Team();

        foreach (PlayerDriverPhoton driver in Object.FindObjectsOfType<PlayerDriverPhoton>())
        {
            switch (driver.TeamNumber)
            {
                case PhotonBattle.TeamAlphaValue:
                    _teams[0].List.Add(driver.ActorTransform);
                    break;

                case PhotonBattle.TeamBetaValue:
                    _teams[1].List.Add(driver.ActorTransform);
                    break;
            }
        }
    }

    [PunRPC]
    private void SlingRpc(double launchTimeS)
    {
        StartCoroutine(SlingDelayd((float)Math.Max(launchTimeS - PhotonNetwork.Time, 0.0)));
    }

    private IEnumerator SlingDelayd(float waitTimeS)
    {
        yield return new WaitForSeconds(waitTimeS);

        foreach (Team team in _teams)
        {
            team.DistanceSquared = -1;

            if (team.List.Count != 2) continue;

            float player0YDistance = Mathf.Abs(team.List[0].position.y);
            float player1YDistance = Mathf.Abs(team.List[1].position.y);

            if (player0YDistance == player1YDistance) continue;

            if (player0YDistance < player1YDistance)
            {
                team.FrontPlayer = team.List[0];
                team.BackPlayer = team.List[1];
            }
            else
            {
                team.FrontPlayer = team.List[1];
                team.BackPlayer = team.List[0];
            }

            team.LaunchVector = team.FrontPlayer.position - team.BackPlayer.position;
            team.DistanceSquared = team.LaunchVector.sqrMagnitude;
        }

        Vector3 launchPosition;
        Vector3 launchDirection;
        float launchSpeed;
        if (_teams[0].DistanceSquared >= 0 || _teams[1].DistanceSquared >= 0)
        {
            Team team = _teams[0].DistanceSquared > _teams[1].DistanceSquared ? _teams[0] : _teams[1];
            launchSpeed = Mathf.Sqrt(team.DistanceSquared) * 2f;
            launchDirection = team.LaunchVector.normalized;
            launchPosition = team.FrontPlayer.position + launchDirection * _startingDistance;
        }
        else
        {
            launchDirection = new Vector3(0.5f, 0.5f);
            launchSpeed = _defaultSpeed;
            launchPosition = Vector3.zero;
        }

        _ball.Launch(launchPosition, launchDirection, launchSpeed);
    }
}
