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
    private Transform _path1Position1;
    [SerializeField]
    private AnimationCurve _path1Curve;
    [SerializeField]
    private Transform _path2LeftPosition1;
    [SerializeField]
    private Transform _path2LeftPosition2;
    [SerializeField]
    private Transform _path2RightPosition1;
    [SerializeField]
    private Transform _path2RightPosition2;
    [SerializeField]
    private Transform _path3Position1;

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
            _characterAnimator1.Play(validatedList.First(x => x.Emotion == randomClipOrder1[i]).Character1Animation.name);
            GameObject ball = Instantiate(_emotionBall, _endStartPositionLeft);

            ball.GetComponent<Image>().sprite = validatedList.First(x => x.Emotion == randomClipOrder1[i]).BallSprite;
            
            ball.GetComponent<RectTransform>().rotation = Quaternion.Euler(new(0, 180, 0));
            bool ballDone = false;
            switch (randomBallOrder1[i])
            {
                case 0:
                    StartCoroutine(BallAnimationLeft1(ball, done => ballDone = done));
                    break;
                case 1:
                    StartCoroutine(BallAnimationLeft2(ball, done => ballDone = done));
                    break;
                case 2:
                    StartCoroutine(BallAnimationLeft3(ball, done => ballDone = done));
                    break;
                default:
                    ballDone = true;
                    break;
            }
            yield return new WaitUntil(() => ballDone is true);
            Destroy(ball);

            yield return new WaitForSeconds(0.5f);
            //Debug.LogWarning($"Character 2: {randomClipOrder2[i]}:{validatedList.First(x => x.Emotion == randomClipOrder2[i]).Character2Animation.name}, Ball 2: {randomBallOrder2[i]}");
            _characterAnimator2.Play(validatedList.First(x => x.Emotion == randomClipOrder2[i]).Character2Animation.name);
            GameObject ball2 = Instantiate(_emotionBall, _endStartPositionRight);

            ball2.GetComponent<Image>().sprite = validatedList.First(x => x.Emotion == randomClipOrder2[i]).BallSprite;

            ballDone = false;
            switch (randomBallOrder2[i])
            {
                case 0:
                    StartCoroutine(BallAnimationRight1(ball2, done => ballDone = done));
                    break;
                case 1:
                    StartCoroutine(BallAnimationRight2(ball2, done => ballDone = done));
                    break;
                case 2:
                    StartCoroutine(BallAnimationRight3(ball2, done => ballDone = done));
                    break;
                default:
                    ballDone = true;
                    break;
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

    private IEnumerator BallAnimationRight1(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path1Position1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path1Position1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionRight.position, _path1Position1.position, currentTime / duration);
            float yPos = _endStartPositionRight.position.y + (_path1Position1.position.y - _endStartPositionRight.position.y) * _path1Curve.Evaluate(currentTime / duration);
            ball.transform.position = new(pos.x, yPos);
        }
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path1Position1.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionLeft.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path1Position1.position, _endStartPositionLeft.position, currentTime / duration);
            float yPos = _endStartPositionLeft.position.y + (_path1Position1.position.y - _endStartPositionLeft.position.y) * _path1Curve.Evaluate(1 - currentTime / duration);
            ball.transform.position = new(pos.x, yPos);
        }
        callback(true);
    }

    private IEnumerator BallAnimationLeft1(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path1Position1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while(Mathf.Abs(Vector2.Distance(ball.transform.position, _path1Position1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionLeft.position, _path1Position1.position, currentTime / duration);
            float yPos = _endStartPositionLeft.position.y + (_path1Position1.position.y - _endStartPositionLeft.position.y) * _path1Curve.Evaluate(currentTime / duration);
            ball.transform.position = new(pos.x,yPos);
        }
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path1Position1.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionRight.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path1Position1.position, _endStartPositionRight.position, currentTime / duration);
            float yPos = _endStartPositionRight.position.y + (_path1Position1.position.y - _endStartPositionRight.position.y) * _path1Curve.Evaluate(1 - currentTime / duration);
            ball.transform.position = new(pos.x, yPos);
        }
        callback(true);
    }

    private IEnumerator BallAnimationLeft2(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path2LeftPosition1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path2LeftPosition1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionLeft.position, _path2LeftPosition1.position, currentTime / duration);
            ball.transform.position = pos;
        }

        speed = 60f;
        distance = Mathf.Abs(Vector2.Distance(_path2LeftPosition1.position, _path2LeftPosition2.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path2LeftPosition2.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path2LeftPosition1.position, _path2LeftPosition2.position, currentTime / duration);
            ball.transform.position = pos;
        }

        speed = 300f;
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path2LeftPosition2.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionRight.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path2LeftPosition2.position, _endStartPositionRight.position, currentTime / duration);
            ball.transform.position = pos;
        }
        callback(true);
    }

    private IEnumerator BallAnimationRight2(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path2RightPosition1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path2RightPosition1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionRight.position, _path2RightPosition1.position, currentTime / duration);
            ball.transform.position = pos;
        }

        speed = 60f;
        distance = Mathf.Abs(Vector2.Distance(_path2RightPosition1.position, _path2RightPosition2.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path2RightPosition2.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path2RightPosition1.position, _path2RightPosition2.position, currentTime / duration);
            ball.transform.position = pos;
        }

        speed = 300f;
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path2RightPosition2.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionLeft.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path2RightPosition2.position, _endStartPositionLeft.position, currentTime / duration);
            ball.transform.position = pos;
        }
        callback(true);
    }

    private IEnumerator BallAnimationLeft3(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path3Position1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path3Position1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionLeft.position, _path3Position1.position, currentTime / duration);
            ball.transform.position = pos;
        }
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path3Position1.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionRight.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path3Position1.position, _endStartPositionRight.position, currentTime / duration);
            ball.transform.position = pos;
        }
        callback(true);
    }

    private IEnumerator BallAnimationRight3(GameObject ball, Action<bool> callback)
    {
        float speed = 150f;
        float distance = Mathf.Abs(Vector2.Distance(_endStartPositionRight.position, _path3Position1.position));
        float duration = distance / speed;
        float currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _path3Position1.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_endStartPositionRight.position, _path3Position1.position, currentTime / duration);
            ball.transform.position = pos;
        }
        distance = Mathf.Abs(Vector2.Distance(_endStartPositionLeft.position, _path3Position1.position));
        duration = distance / speed;
        currentTime = 0;
        while (Mathf.Abs(Vector2.Distance(ball.transform.position, _endStartPositionLeft.position)) > Mathf.Epsilon)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Vector2 pos = Vector2.Lerp(_path3Position1.position, _endStartPositionLeft.position, currentTime / duration);
            ball.transform.position = pos;
        }
        callback(true);
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
