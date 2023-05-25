using Math = System.Math;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;

public class SlingControllerTest : MonoBehaviour
{
    [SerializeField] private BallHandlerTest _ball;
    [SerializeField] private double _slingDelaySec;
    [SerializeField] private float _startingDistance;
    [SerializeField] private int _defaultSpeed;
    [SerializeField] private float _indicatorLengthMultiplier;
    [SerializeField] private float _indicatorWidthMultiplier;
    [SerializeField] private float _indicatorNormalOpacity;
    [SerializeField] private float _indicatorLowOpacity;

    private PhotonView _photonView;

    private class Team
    {
        public List<Transform> List = new();
        public Transform FrontPlayer;
        public Transform BackPlayer;

        public float Distance;
        public Vector3 LaunchDirection;

        /*
        public float DistanceSquared;
        public Vector3 LaunchVector;
        */
    };
    private Team[] _teams;

    private class SlingIndicator
    {
        public Transform Transform;
        public SpriteRenderer SpriteRenderer;

        public SlingIndicator(Transform transform, SpriteRenderer spriteRenderer)
        {
            Transform = transform;
            SpriteRenderer = spriteRenderer;
        }
    }
    private SlingIndicator[] _slingIndicators;

    bool _slingMode = false;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        _slingIndicators = new SlingIndicator[2];
        {
            int i = 0;
            foreach (Transform child in transform)
            {
                _slingIndicators[i] = new SlingIndicator(child, child.GetComponent<SpriteRenderer>());
                i++;
            }
        }
        StartCoroutine(nameof(InitializeTeams));
    }

    void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            _photonView.RPC(nameof(SlingRpc), RpcTarget.All, PhotonNetwork.Time + _slingDelaySec);
        }
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
        _slingMode = true;
        yield return new WaitForSeconds(waitTimeS);

        Vector3 launchPosition;
        Vector3 launchDirection;
        float launchSpeed;
        if (_teams[0].Distance >= 0 || _teams[1].Distance >= 0)
        {
            Team team = _teams[0].Distance > _teams[1].Distance ? _teams[0] : _teams[1];
            launchSpeed = team.Distance * 2f;
            launchDirection = team.LaunchDirection;
            launchPosition = team.FrontPlayer.position + launchDirection * _startingDistance;
        }
        else
        {
            launchDirection = new Vector3(0.5f, 0.5f);
            launchSpeed = _defaultSpeed;
            launchPosition = Vector3.zero;
        }

        _slingMode = false;
        foreach (SlingIndicator slingIndicator in _slingIndicators)
        {
            slingIndicator.SpriteRenderer.enabled = false;
        }
        _ball.Launch(launchPosition, launchDirection, launchSpeed);
    }

    private void Update()
    {
        if (!_slingMode) return;

        foreach (Team team in _teams)
        {
            team.Distance = -1;

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

            Vector3 launchVector = team.FrontPlayer.position - team.BackPlayer.position;
            team.Distance = launchVector.magnitude;
            team.LaunchDirection = launchVector / team.Distance;
        }

        {
            Team teamWithMoreDistance = _teams[0].Distance > _teams[1].Distance ? _teams[0] : _teams[1];
            int i = 0;
            foreach (Team team in _teams)
            {
                if (team.Distance >= 0)
                {
                    float length = team.Distance * _indicatorLengthMultiplier + 2.5f;
                    _slingIndicators[i].Transform.position = team.BackPlayer.position + (team.LaunchDirection * length * 0.5f);
                    _slingIndicators[i].Transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(team.LaunchDirection.y, team.LaunchDirection.x) * (360 / (Mathf.PI * 2.0f)), Vector3.forward);
                    _slingIndicators[i].SpriteRenderer.size = new Vector2(length * 2.0f, team.Distance * _indicatorWidthMultiplier * 2.0f);
                    _slingIndicators[i].SpriteRenderer.color = new Color(1.0f, 1.0f, 1.0f, team == teamWithMoreDistance ? _indicatorNormalOpacity : _indicatorLowOpacity);
                    _slingIndicators[i].SpriteRenderer.enabled = true;
                }
                else
                {
                    _slingIndicators[i].SpriteRenderer.enabled = false;
                }
                i++;
            }
        }
    }
}
