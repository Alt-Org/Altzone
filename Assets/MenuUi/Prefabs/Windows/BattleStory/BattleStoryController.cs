using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Common;
using Altzone.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleStoryController : MonoBehaviour
{
    [SerializeField]
    private Button _exitButton;

    [SerializeField]
    private GameObject _emotionBall;

    [SerializeField]
    private Image _tableSprite;
    [SerializeField]
    private RectTransform _pathArea;

    [SerializeField]
    private Transform _startPositionLeft;
    [SerializeField]
    private Transform _startPositionRight;

    [Header("Paths"), SerializeField]
    private List<Route> _routesLeft;
    [SerializeField]
    private List<Route> _routesRight;

    [Header("Ball Emotion Sprites"), SerializeField]
    private List<EmotionObject> _emotionList;
    private List<EmotionObject> _validatedList;

    [Header("Character Animators"),SerializeField]
    private Animator _characterAnimator1;
    [SerializeField]
    private Animator _characterAnimator2;

    [Header("Text lines"), SerializeField]
    private BattleStoryLineHandler _topLineImage;
    [SerializeField]
    private BattleStoryLineHandler _bottomLineImage;
    [SerializeField]
    private List<ConversationLine> _conversationList;

    [Header("Story Play Controls"), SerializeField]
    private TextMeshProUGUI _currentSegmentText;
    [SerializeField]
    private Button _previousSegment;
    [SerializeField]
    private Button _nextSegment;
    [SerializeField]
    private Button _autoPlayButton;

    private List<StorySegment> _storySegments = new();
    private int _currentSegment;
    private int _totalSegments;

    private Coroutine _playbackCoroutine;
    private Coroutine _routeTraversingCoroutine;
    private GameObject _ball;

    // Start is called before the first frame update
    void Start()
    {
        _exitButton.onClick.AddListener(ExitStory);
        _previousSegment.onClick.AddListener(PlayPreviousSegment);
        _nextSegment.onClick.AddListener(PlayNextSegment);
        _autoPlayButton.onClick.AddListener(ToggleAutoPlay);
        Route.OnRouteFinished += DestroyBall;
        StartCoroutine(SetPathArea());
        GenerateStory();
        _totalSegments = _storySegments.Count;
        _currentSegment = 0;
        _playbackCoroutine = StartCoroutine(PlayAnimation());
    }
    private void OnEnable()
    {
        _topLineImage.gameObject.SetActive(false);
        _bottomLineImage.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _exitButton.onClick.RemoveAllListeners();
        _previousSegment.onClick.RemoveAllListeners();
        _nextSegment.onClick.RemoveAllListeners();
        _autoPlayButton.onClick.RemoveAllListeners();
        Route.OnRouteFinished -= DestroyBall;
    }

    private void GenerateStory()
    {
        _validatedList = ValidateEmotions();

        int clipsCount = _validatedList.Count;
        if (clipsCount <= 0) return;
        if (_routesLeft.Count <= 0) return;
        if (_routesRight.Count <= 0) return;

        int prevSelectedValue1 = -1;
        int selectedvalue1 = -1;
        int prevSelectedValue2 = -1;
        int selectedvalue2 = -1;
        for (int i = 0; i < 5; i++)
        {
            do
            {
                selectedvalue1 = Random.Range(0, clipsCount);
            } while (selectedvalue1.Equals(prevSelectedValue1));
            Emotion emotion = _validatedList[selectedvalue1].Emotion;
            prevSelectedValue1 = selectedvalue1;
            int ballAnimation1 = Random.Range(0, _routesLeft.Count);
            _storySegments.Add(new(emotion, 0, ballAnimation1, _conversationList[i * 2].Line));

            do
            {
                selectedvalue2 = Random.Range(0, clipsCount);
            } while (selectedvalue2.Equals(prevSelectedValue2));
            emotion = _validatedList[selectedvalue2].Emotion;
            prevSelectedValue2 = selectedvalue2;
            int ballAnimation2 = Random.Range(0, _routesRight.Count);
            _storySegments.Add(new(emotion, 1, ballAnimation2, _conversationList[i * 2 + 1].Line));
        }
    }

    public IEnumerator PlayAnimation()
    {
        _autoPlayButton.GetComponent<Image>().color = Color.yellow;
        yield return new WaitForSeconds(1f);
        for (int i = _currentSegment; i < _storySegments.Count; i++)
        {
            _currentSegment++;
            yield return PlayAnimationSegment(i);

            yield return new WaitForSeconds(1f);
        }
        _autoPlayButton.GetComponent<Image>().color = Color.white;
        _playbackCoroutine = null;
    }

    private IEnumerator PlayAnimationSegment(int segment)
    {
        if(_ball != null) DestroyBall();

        _currentSegmentText.text = $"{_currentSegment}/{_totalSegments}";

        _ball = _storySegments[segment].Player == 0 ? Instantiate(_emotionBall, _startPositionLeft) : Instantiate(_emotionBall, _startPositionRight);

        _ball.GetComponent<Image>().sprite = GetEmotionData(_storySegments[segment].ClipEmotion).BallSprite;

        if (_storySegments[segment].Player == 0) _ball.GetComponent<RectTransform>().rotation = Quaternion.Euler(new(0, 180, 0));
        bool ballDone = false;

        if (_storySegments[segment].Player == 0)
        {
            if (_routesLeft.Count <= _storySegments[segment].BallRoute || 0 > _storySegments[segment].BallRoute)
            {
                ballDone = true;
            }
            else
            {
                _routeTraversingCoroutine = StartCoroutine(_routesLeft[_storySegments[segment].BallRoute].TraverseRoute(_ball, done => ballDone = done));
                _bottomLineImage.SetText(GetEmotionData(_storySegments[segment].ClipEmotion).LineSprite, _storySegments[segment].Line);
                for (int i = _currentSegment-1; i >= 0; i--)
                {
                    if (_storySegments[i].Player != 0)
                    {
                        _topLineImage.SetText(GetEmotionData(_storySegments[i].ClipEmotion).LineSprite, _storySegments[i].Line);
                        break;
                    }
                }
                _characterAnimator1.Play(GetEmotionData(_storySegments[segment].ClipEmotion).Character1Animation.name);
            }
        }
        else
        {
            if (_routesRight.Count <= _storySegments[segment].BallRoute || 0 > _storySegments[segment].BallRoute)
            {
                ballDone = true;
            }
            else
            {
                _routeTraversingCoroutine = StartCoroutine(_routesRight[_storySegments[segment].BallRoute].TraverseRoute(_ball, done => ballDone = done));
                _topLineImage.SetText(GetEmotionData(_storySegments[segment].ClipEmotion).LineSprite, _storySegments[segment].Line);
                for (int i = _currentSegment - 1; i >= 0; i--)
                {
                    if (_storySegments[i].Player == 0)
                    {
                        _bottomLineImage.SetText(GetEmotionData(_storySegments[i].ClipEmotion).LineSprite, _storySegments[i].Line);
                        break;
                    }
                }
                _characterAnimator2.Play(GetEmotionData(_storySegments[segment].ClipEmotion).Character2Animation.name);
            }
        }

        yield return new WaitUntil(() => ballDone is true);
    }

    private void PlayNextSegment()
    {
        if (_currentSegment < _totalSegments)
        {
            if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
            _autoPlayButton.GetComponent<Image>().color = Color.white;
            DestroyBall();
            _currentSegment++;
            StartCoroutine(PlayAnimationSegment(_currentSegment - 1));
        }
    }

    private void PlayPreviousSegment()
    {
        if (_currentSegment > 1)
        {
            if (_playbackCoroutine != null) StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
            _autoPlayButton.GetComponent<Image>().color = Color.white;
            DestroyBall();
            _currentSegment--;
            StartCoroutine(PlayAnimationSegment(_currentSegment - 1));
        }
    }

    private void ToggleAutoPlay()
    {
        if (_playbackCoroutine != null)
        {
            StopCoroutine(_playbackCoroutine);
            _playbackCoroutine = null;
            _autoPlayButton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            _playbackCoroutine = StartCoroutine(PlayAnimation());
        }
    }

    private void DestroyBall()
    {
        Debug.LogWarning("Ball");
        if (_routeTraversingCoroutine != null)
        {
            StopCoroutine(_routeTraversingCoroutine);
            _routeTraversingCoroutine = null;
        }
        if (_ball == null) return;
        Destroy(_ball);
        _ball = null;
    }

    private EmotionObject GetEmotionData(Emotion emotion)
    {
        if (emotion == Emotion.Blank) return null;

        foreach (EmotionObject emotionObj in _validatedList)
        {
            if(emotionObj.Emotion == emotion) return emotionObj;
        }
        return null;
    }

    private List<EmotionObject> ValidateEmotions()
    {
        List<EmotionObject> validatedList = new ();
        foreach(EmotionObject emotionObj in _emotionList)
        {
            if (emotionObj.Emotion is Emotion.Blank) continue;

            if (validatedList.Any(validatedEmotion => emotionObj.Emotion.Equals(validatedEmotion.Emotion)))
            {
                Debug.LogWarning("Multiple Emotions Objects with same Emotion value detected. Ignoring the latter.");
                continue;
            }

            validatedList.Add(emotionObj);
        }
        return validatedList;
    }

    private IEnumerator SetPathArea()
    {
        yield return new WaitForEndOfFrame();
        Vector2 spriteSize = _tableSprite.sprite.rect.size;
        float spriteRatio = spriteSize.y / spriteSize.x;

        Vector2 areaSize = _pathArea.rect.size;
        float areaRatio = areaSize.y / areaSize.x;
        if(spriteRatio < areaRatio)
        {
            float diff = 1 - spriteRatio/areaRatio;
            _pathArea.anchorMin = new Vector2(0, diff/2);
            _pathArea.anchorMax = new Vector2(1, 1- diff/2);
        }
        else
        {
            float diff = 1 - areaRatio / spriteRatio;
            _pathArea.anchorMin = new Vector2(diff / 2, 0);
            _pathArea.anchorMax = new Vector2(1 - diff / 2, 1);
        }
    }

    private void ExitStory()
    {
        LobbyManager.ExitBattleStory();
    }

}

[Serializable]
public class EmotionObject
{
    [SerializeField]
    private Emotion _emotion;
    [SerializeField]
    private Sprite _ballSprite;
    [SerializeField]
    private AnimationClip _character1Animation;
    [SerializeField]
    private AnimationClip _character2Animation;
    [SerializeField]
    private Sprite _lineSprite;

    public Emotion Emotion { get => _emotion;}
    public Sprite BallSprite { get => _ballSprite;}
    public AnimationClip Character1Animation { get => _character1Animation;}
    public AnimationClip Character2Animation { get => _character2Animation;}
    public Sprite LineSprite { get => _lineSprite;}
}

public class StorySegment
{
    private Emotion _clipEmotion;
    private int _ballRoute;
    private string _line;
    private int _player;

    public Emotion ClipEmotion { get => _clipEmotion; }
    public int BallRoute { get => _ballRoute;}
    public string Line { get => _line;}
    public int Player { get => _player;}

    public StorySegment(Emotion emotion, int player, int route, string line)
    {
        _clipEmotion = emotion;
        _player = player;
        _line = line;
        _ballRoute = route;
    }

}

[Serializable]
public class Route
{
    [SerializeField]
    private Transform _startPoint;
    [SerializeField]
    private float _defaultSpeed = 10f;
    [SerializeField]
    private List<RouteSection> _routesSection;
    [SerializeField]
    private Transform _endPoint;

    private float _baseSpeed = 400f;

    public List<RouteSection> RoutesSection { get => _routesSection;}
    public Transform StartPoint { get => _startPoint; }
    public Transform EndPoint { get => _endPoint;}
    public float DefaultSpeed { get => _defaultSpeed;}

    public delegate void RouteFinished();
    public static event RouteFinished OnRouteFinished;

    public IEnumerator TraverseRoute(GameObject ball, Action<bool> callback)
    {
        float defaultBallSize = ball.GetComponent<RectTransform>().rect.width;
        float ballsize = defaultBallSize * 1.2f;
        ball.GetComponent<RectTransform>().sizeDelta = new(ballsize,ballsize);
        float baseSpeed = GetScaledSpeed();
        float speed = baseSpeed;
        float distance;
        float duration;
        float currentTime;

        Vector2 startPosition = _startPoint.position;
        float depth = Mathf.Abs(_endPoint.position.y - _startPoint.position.y);

        foreach (RouteSection route in _routesSection)
        {
            speed = baseSpeed*route.Speed;
            Vector2 nextPoint = route.PathSectionEndPoint.position;
            distance = Mathf.Abs(Vector2.Distance(startPosition, nextPoint));
            duration = distance / speed;
            currentTime = 0;
            while (Mathf.Abs(Vector2.Distance(ball.transform.position, nextPoint)) > Mathf.Epsilon && currentTime / duration < 1)
            {
                yield return null;
                currentTime += Time.deltaTime;
                Vector2 pos = Vector2.Lerp(startPosition, nextPoint, currentTime / duration);
                float yPos = startPosition.y + (nextPoint.y - startPosition.y) * route.PathSectionCurve.Evaluate(Mathf.Clamp(currentTime / duration,0,1));
                ball.transform.position = new(pos.x, yPos);
                float depthDistance = Mathf.Abs(yPos - _startPoint.position.y)/depth;
                ballsize = defaultBallSize * Mathf.Lerp(1.2f, 0.8f, depthDistance);
                ball.GetComponent<RectTransform>().sizeDelta = new(ballsize, ballsize);
            }
            startPosition = nextPoint;
        }
        if (Mathf.Abs(Vector2.Distance(_endPoint.position, startPosition)) <= Mathf.Epsilon)
        {
            callback(true);
            OnRouteFinished?.Invoke();
            yield break;
        }
        speed = baseSpeed * _defaultSpeed;
        distance = Mathf.Abs(Vector2.Distance(startPosition, _endPoint.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endPoint.position)) > Mathf.Epsilon && currentTime / duration < 1)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(startPosition, _endPoint.position, currentTime / duration);
            ball.transform.position = pos;
        }
        callback(true);
        OnRouteFinished?.Invoke();
    }

    private float GetScaledSpeed()
    {
        return Mathf.Abs(Vector2.Distance(_endPoint.position, _startPoint.position))/10;
    }
}

[Serializable]
public class RouteSection
{
    [SerializeField]
    private Transform _pathSectionEndPoint;
    [SerializeField]
    private AnimationCurve _pathSectionCurve;
    [SerializeField]
    private float _speed;

    public Transform PathSectionEndPoint { get => _pathSectionEndPoint; }
    public AnimationCurve PathSectionCurve { get => _pathSectionCurve; }
    public float Speed { get => _speed; }
}

[Serializable]
public class ConversationLine
{
    /// <summary>
    /// Character conversation line>.
    /// </summary>
    [TextArea(1, 3)]
    public string Line;

    public int Character;
}
