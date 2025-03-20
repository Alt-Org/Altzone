using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Lobby;
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
    private Transform _endStartPositionLeft;
    [SerializeField]
    private Transform _endStartPositionRight;

    [Header("Paths"), SerializeField]
    private List<Route> _routesLeft;
    [SerializeField]
    private List<Route> _routesRight;

    [Header("Ball Emotion Sprites"), SerializeField]
    private List<EmotionObject> _emotionList;

    [Header("Character Animators"),SerializeField]
    private Animator _characterAnimator1;
    [SerializeField]
    private Animator _characterAnimator2;

    // Start is called before the first frame update
    void Start()
    {
        _exitButton.onClick.AddListener(ExitStory);
        StartCoroutine(PlayAnimation());
    }


    public IEnumerator PlayAnimation()
    {
        List<EmotionObject> validatedList = ValidateEmotions();

        int clipsCount = validatedList.Count;
        if (clipsCount <= 0) yield break;
        List<Emotion> randomClipOrder1 = new();
        List<int> randomBallOrder1 = new();

        List<Emotion> randomClipOrder2 = new();
        List<int> randomBallOrder2 = new();
        int prevSelectedValue1 = -1;
        int selectedvalue1 = -1;
        int prevSelectedValue2 = -1;
        int selectedvalue2 = -1;
        for (int i= 0; i<5; i++)
        {
            do
            {
                selectedvalue1 = Random.Range(0, clipsCount);
            } while(selectedvalue1.Equals(prevSelectedValue1));
            randomClipOrder1.Add(validatedList[selectedvalue1].Emotion);
            prevSelectedValue1 = selectedvalue1;
            int ballAnimation1 = Random.Range(0, 3);
            randomBallOrder1.Add(ballAnimation1);

            do
            {
                selectedvalue2 = Random.Range(0, clipsCount);
            } while (selectedvalue2.Equals(prevSelectedValue2));
            randomClipOrder2.Add(validatedList[selectedvalue2].Emotion);
            prevSelectedValue2 = selectedvalue2;
            int ballAnimation2 = Random.Range(0, 3);
            randomBallOrder2.Add(ballAnimation2);
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < randomClipOrder1.Count; i++)
        {
            //Debug.LogWarning($"Character 1: {randomClipOrder1[i]}:{validatedList.First(x => x.Emotion == randomClipOrder1[i]).Character1Animation.name}, Ball 1: {randomBallOrder1[i]}");
            _characterAnimator1.Play(GetEmotionData(randomClipOrder1[i]).Character1Animation.name);
            GameObject ball = Instantiate(_emotionBall, _endStartPositionLeft);

            ball.GetComponent<Image>().sprite = GetEmotionData(randomClipOrder1[i]).BallSprite;
            
            ball.GetComponent<RectTransform>().rotation = Quaternion.Euler(new(0, 180, 0));
            bool ballDone = false;

            if (_routesLeft.Count <= randomBallOrder1[i] || 0 > randomBallOrder1[i])
            {
                ballDone = true;
            }
            else
            {
                StartCoroutine(_routesLeft[randomBallOrder1[i]].TraverseRoute(ball, done => ballDone = done));
            }

            yield return new WaitUntil(() => ballDone is true);
            Destroy(ball);

            yield return new WaitForSeconds(0.5f);
            //Debug.LogWarning($"Character 2: {randomClipOrder2[i]}:{validatedList.First(x => x.Emotion == randomClipOrder2[i]).Character2Animation.name}, Ball 2: {randomBallOrder2[i]}");
            _characterAnimator2.Play(GetEmotionData(randomClipOrder2[i]).Character2Animation.name);
            GameObject ball2 = Instantiate(_emotionBall, _endStartPositionRight);

            ball2.GetComponent<Image>().sprite = GetEmotionData(randomClipOrder2[i]).BallSprite;

            ballDone = false;

            if (_routesRight.Count <= randomBallOrder2[i] || 0 > randomBallOrder2[i])
            {
                ballDone = true;
            }
            else
            {
                StartCoroutine(_routesRight[randomBallOrder2[i]].TraverseRoute(ball2, done => ballDone = done));
            }

            yield return new WaitUntil(() => ballDone is true);
            Destroy(ball2);
            yield return new WaitForSeconds(0.5f);
        }
    }

    private EmotionObject GetEmotionData(Emotion emotion)
    {
        if (emotion == Emotion.Blank) return null;

        foreach (EmotionObject emotionObj in _emotionList)
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

    private void ExitStory()
    {
        LobbyManager.ExitBattleStory();
    }

}

public enum Emotion
{
    Blank,
    Anger,
    Joy,
    Love,
    Playful,
    Sorrow
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

    public Emotion Emotion { get => _emotion;}
    public Sprite BallSprite { get => _ballSprite;}
    public AnimationClip Character1Animation { get => _character1Animation;}
    public AnimationClip Character2Animation { get => _character2Animation;}
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

    public IEnumerator TraverseRoute(GameObject ball, Action<bool> callback)
    {
        float baseSpeed = GetScaledSpeed();
        float speed = baseSpeed;
        float distance;
        float duration;
        float currentTime;

        Vector2 startPosition = _startPoint.position;

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
            }
            startPosition = nextPoint;
        }
        Debug.LogWarning("...");
        if (Mathf.Abs(Vector2.Distance(_endPoint.position, startPosition)) <= Mathf.Epsilon)
        {
            callback(true);
            yield break;
        }
        Debug.LogWarning("..");
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
