using Math = System.Math;
using Action = System.Action;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;

public class BallSlinged
{
    public readonly int SlingingTeamNumber;

    public BallSlinged(int slingingTeamNumber)
    {
        SlingingTeamNumber = slingingTeamNumber;
    }
}


public class SlingControllerTest : MonoBehaviour
{
    [Header("Sling")]
    [SerializeField] private BallHandlerTest _ball;
    [SerializeField] private double _aimingTimeSec;
    [SerializeField] private float _startingDistance;
    [SerializeField] private int _defaultSpeed;
    [SerializeField] private bool _autoStart;

    [Header("Indicator")]
    [SerializeField] private float _indicatorLengthMultiplier;
    [SerializeField] private float _indicatorWidthMultiplier;
    [SerializeField] private float _indicatorNormalOpacity;
    [SerializeField] private float _indicatorLowOpacity;

    private PhotonView _photonView;
    private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;

    private class Team
    {
        public bool SlingActive = false;
        public List<Transform> List = new();
        public Transform FrontPlayer;
        public Transform BackPlayer;
        public float Distance;
        public Vector3 LaunchDirection;
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

    bool _teamsAreReadyForGameplay = false;
    bool _slingMode = false;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
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
    }

    void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        _teams = new Team[2];
        _teams[0] = new Team();
        foreach (IDriver driver in data.TeamAlpha.GetAllDrivers()) _teams[0].List.Add(driver.ActorTransform);
        _teams[1] = new Team();
        foreach (IDriver driver in data.TeamBeta.GetAllDrivers()) _teams[1].List.Add(driver.ActorTransform);
        _teamsAreReadyForGameplay = true;

        if (PhotonNetwork.IsMasterClient && _autoStart)
        {
            SlingActivate(_aimingTimeSec);
        }
    }

    public bool SlingMode => _slingMode;

    public void SlingActivate(double aimingTimeSec)
    {
        SlingModeActivate(true, true, aimingTimeSec);
    }

    public void SlingActivate(int teamNumber, double aimingTimeSec)
    {
        switch (teamNumber)
        {
            case PhotonBattle.TeamAlphaValue:
                SlingModeActivate(true, false, aimingTimeSec);
                break;

            case PhotonBattle.TeamBetaValue:
                SlingModeActivate(false, true, aimingTimeSec);
                break;
        }
    }

    private void SlingModeActivate(bool teamAlpha, bool teamBeta, double aimingTimeSec)
    {
        if (!_teamsAreReadyForGameplay)
        {
            Debug.Log("Teams are not ready for gameplay");
            return;
        }

        if (_slingMode)
        {
            Debug.Log("Sling already active");
            return;
        }

        _photonView.RPC(nameof(SlingRpc), RpcTarget.All, teamAlpha, teamBeta, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(aimingTimeSec));
    }

    [PunRPC]
    private void SlingRpc(bool teamAlpha, bool teamBeta, int launchUpdateNumber)
    {
        Sling(teamAlpha, teamBeta, launchUpdateNumber);
    }

    private void Sling(bool teamAlpha, bool teamBeta, int launchUpdateNumber)
    {
        _teams[0].SlingActive = teamAlpha;
        _teams[1].SlingActive = teamBeta;
        _slingMode = true;
        _syncedFixedUpdateClock.ExecuteOnUpdate(launchUpdateNumber, -1, () =>
        {
            int slingingTeamNumber;
            Vector3 launchPosition;
            Vector3 launchDirection;
            float launchSpeed;
            if (_teams[0].Distance >= 0 || _teams[1].Distance >= 0)
            {
                bool b = _teams[0].Distance > _teams[1].Distance;
                slingingTeamNumber = b ? PhotonBattle.TeamAlphaValue : PhotonBattle.TeamBetaValue;
                Team team = b ? _teams[0] : _teams[1];
                launchSpeed = team.Distance * 2f;
                launchDirection = team.LaunchDirection;
                launchPosition = team.FrontPlayer.position + launchDirection * _startingDistance;
            }
            else
            {
                slingingTeamNumber = PhotonBattle.NoTeamValue;
                launchDirection = new Vector3(0.5f, 0.5f);
                launchSpeed = _defaultSpeed;
                launchPosition = Vector3.zero;
            }

            foreach (Team team in _teams)
            {
                team.SlingActive = false;
            }

            _slingMode = false;
            foreach (SlingIndicator slingIndicator in _slingIndicators)
            {
                slingIndicator.SpriteRenderer.enabled = false;
            }
            _ball.Launch(launchPosition, launchDirection, launchSpeed);
            this.Publish(new BallSlinged(slingingTeamNumber));
        });
    }

    private void Update()
    {
        if (!_slingMode) return;

        foreach (Team team in _teams)
        {
            team.Distance = -1;

            if (!team.SlingActive || team.List.Count != 2) continue;

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
