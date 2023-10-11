using Math = System.Math;
using Action = System.Action;
using System.Collections;
using System.Collections.Generic;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
using UnityEngine;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;

public class SlingControllerReady
{ }

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
    [SerializeField] private float _slingSpeedMultiplier;
    [SerializeField] private float _startingDistance;
    [SerializeField] private int _defaultSlingSpeed;

    [Header("Indicator")]
    [SerializeField] private float _indicatorLengthMultiplier;
    [SerializeField] private float _indicatorWidthMultiplier;

    private PhotonView _photonView;
    private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;

    private class Team
    {
        public List<Transform> List = new();
        public Transform FrontPlayer;
        public Transform BackPlayer;
        public float Distance;
        public Vector3 LaunchDirection;
    };
    private Team[] _teams;
    private Team _currentTeam;

    private class SlingIndicator
    {
        public Transform Transform;
        public SpriteRenderer SpriteRenderer;

        public SlingIndicator(GameObject gameObject)
        {
            Transform = gameObject.transform;
            SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        }
    }
    private SlingIndicator _slingIndicator;

    private bool _teamsAreReadyForGameplay = false;
    private bool _slingMode = false;

    void Start()
    {
        _photonView = GetComponent<PhotonView>();
        _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        _slingIndicator = new SlingIndicator(transform.Find("Sling Indicator").gameObject);
    }

    private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
    {
        _teams = new Team[2];
        _teams[0] = new Team();
        foreach (IDriver driver in data.TeamAlpha.GetAllDrivers()) _teams[0].List.Add(driver.ActorTransform);
        _teams[1] = new Team();
        foreach (IDriver driver in data.TeamBeta.GetAllDrivers()) _teams[1].List.Add(driver.ActorTransform);
        _teamsAreReadyForGameplay = true;

        this.Publish(new SlingControllerReady());
    }

    public bool SlingMode => _slingMode;

    public void SlingActivate() { SlingActivate(_aimingTimeSec); }
    public void SlingActivate(double aimingTimeSec)
    {
        int teamNumber = new[] { PhotonBattle.TeamAlphaValue, PhotonBattle.TeamBetaValue }[Random.Range(0, 2)];
        SlingActivate(teamNumber, aimingTimeSec);
    }

    public void SlingActivate(int teamNumber) { SlingActivate(teamNumber, _aimingTimeSec); }
    public void SlingActivate(int teamNumber, double aimingTimeSec)
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

        _photonView.RPC(nameof(SlingRpc), RpcTarget.All, teamNumber, _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(aimingTimeSec));
    }

    [PunRPC]
    private void SlingRpc(int teamNumber, int launchUpdateNumber)
    {
        Sling(teamNumber, launchUpdateNumber);
    }

    private void Sling(int teamNumber, int launchUpdateNumber)
    {
        switch (teamNumber)
        {
            case PhotonBattle.TeamAlphaValue:
                _currentTeam = _teams[0];
                break;

            case PhotonBattle.TeamBetaValue:
                _currentTeam = _teams[1];
                break;
        }
        _slingMode = true;

        _syncedFixedUpdateClock.ExecuteOnUpdate(launchUpdateNumber, -1, () =>
        {
            Vector3 launchPosition;
            Vector3 launchDirection;
            float launchSpeed;

            if (_currentTeam.Distance >= 0)
            {
                launchSpeed = _currentTeam.Distance * _slingSpeedMultiplier;
                launchDirection = _currentTeam.LaunchDirection;
                launchPosition = _currentTeam.FrontPlayer.position + launchDirection * _startingDistance;
            }
            else
            {
                teamNumber = PhotonBattle.NoTeamValue;
                launchDirection = new Vector3(0.5f, 0.5f);
                launchSpeed = _defaultSlingSpeed;
                launchPosition = Vector3.zero;
            }

            _slingMode = false;
            _slingIndicator.SpriteRenderer.enabled = false;

            _ball.Launch(launchPosition, launchDirection, launchSpeed);
            this.Publish(new BallSlinged(teamNumber));
        });
    }

    private void Update()
    {
        if (!_slingMode) return;
        SlingUpdate();
        SlingIndicatorsUpdate();
    }

    private void SlingUpdate()
    {
        _currentTeam.Distance = -1;

        if (_currentTeam.List.Count != 2) return;

        float player0YDistance = Mathf.Abs(_currentTeam.List[0].position.y);
        float player1YDistance = Mathf.Abs(_currentTeam.List[1].position.y);

        if (player0YDistance == player1YDistance) return;

        if (player0YDistance < player1YDistance)
        {
            _currentTeam.FrontPlayer = _currentTeam.List[0];
            _currentTeam.BackPlayer = _currentTeam.List[1];
        }
        else
        {
            _currentTeam.FrontPlayer = _currentTeam.List[1];
            _currentTeam.BackPlayer = _currentTeam.List[0];
        }

        Vector3 launchVector = _currentTeam.FrontPlayer.position - _currentTeam.BackPlayer.position;
        _currentTeam.Distance = launchVector.magnitude;
        _currentTeam.LaunchDirection = launchVector / _currentTeam.Distance;
    }

    private void SlingIndicatorsUpdate()
    {
        if (_currentTeam.Distance >= 0)
        {
            float length = _currentTeam.Distance * _indicatorLengthMultiplier + 2.5f;
            _slingIndicator.Transform.position = _currentTeam.BackPlayer.position + (_currentTeam.LaunchDirection * length * 0.5f);
            _slingIndicator.Transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(_currentTeam.LaunchDirection.y, _currentTeam.LaunchDirection.x) * (360 / (Mathf.PI * 2.0f)), Vector3.forward);
            _slingIndicator.SpriteRenderer.size = new Vector2(length * 2.0f, _currentTeam.Distance * _indicatorWidthMultiplier * 2.0f);
            _slingIndicator.SpriteRenderer.enabled = true;
        }
        else
        {
            _slingIndicator.SpriteRenderer.enabled = false;
        }
    }
}
