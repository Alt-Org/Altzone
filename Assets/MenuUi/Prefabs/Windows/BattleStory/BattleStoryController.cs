using System;
using System.Collections;
using System.Collections.Generic;
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

    [Header("Ball Emotion Sprites"),SerializeField]
    private Sprite _ballAngrySprite;
    [SerializeField]
    private Sprite _ballJoySprite;
    [SerializeField]
    private Sprite _ballLoveSprite;
    [SerializeField]
    private Sprite _ballPlaySprite;
    [SerializeField]
    private Sprite _ballSorrowSprite;

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
        var clips1 = _characterAnimator1.runtimeAnimatorController.animationClips;
        int clips1Count = clips1.Length;
        List<int> randomClipOrder1 = new();
        List<int> randomBallOrder1 = new();
        var clips2 = _characterAnimator2.runtimeAnimatorController.animationClips;
        int clips2Count = clips2.Length;
        List<int> randomClipOrder2 = new();
        List<int> randomBallOrder2 = new();
        int prevSelectedValue1 = -1;
        int selectedvalue1 = -1;
        int prevSelectedValue2 = -1;
        int selectedvalue2 = -1;
        for (int i= 0; i<5; i++)
        {
            do
            {
                selectedvalue1 = Random.Range(0, clips1Count);
            } while(selectedvalue1.Equals(prevSelectedValue1));
            randomClipOrder1.Add(selectedvalue1);
            prevSelectedValue1 = selectedvalue1;
            int ballAnimation1 = Random.Range(0, 3);
            randomBallOrder1.Add(ballAnimation1);

            do
            {
                selectedvalue2 = Random.Range(0, clips2Count);
            } while (selectedvalue2.Equals(prevSelectedValue2));
            randomClipOrder2.Add(selectedvalue2);
            prevSelectedValue2 = selectedvalue2;
            int ballAnimation2 = Random.Range(0, 3);
            randomBallOrder2.Add(ballAnimation2);
        }
        yield return new WaitForSeconds(1f);
        for (int i = 0; i < randomClipOrder1.Count; i++)
        {
            //Debug.LogWarning($"Character 1: {randomClipOrder1[i]}:{clips1[randomClipOrder1[i]].name}, Ball 1: {randomBallOrder1[i]}");
            _characterAnimator1.Play(clips1[randomClipOrder1[i]].name);
            GameObject ball = Instantiate(_emotionBall, _endStartPositionLeft);
            switch (randomClipOrder1[i])
            {
                case 0:
                    ball.GetComponent<Image>().sprite = _ballJoySprite;
                    break;
                case 1:
                    ball.GetComponent<Image>().sprite = _ballPlaySprite;
                    break;
                case 2:
                    ball.GetComponent<Image>().sprite = _ballLoveSprite;
                    break;
                case 3:
                    ball.GetComponent<Image>().sprite = _ballSorrowSprite;
                    break;
                case 4:
                    ball.GetComponent<Image>().sprite = _ballAngrySprite;
                    break;
                default:
                    break;
            }
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
            //Debug.LogWarning($"Character 2: {randomClipOrder2[i]}:{clips2[randomClipOrder2[i]].name}, Ball 2: {randomBallOrder2[i]}");
            _characterAnimator2.Play(clips2[randomClipOrder2[i]].name);
            GameObject ball2 = Instantiate(_emotionBall, _endStartPositionLeft);
            switch (randomClipOrder2[i])
            {
                case 0:
                    ball2.GetComponent<Image>().sprite = _ballJoySprite;
                    break;
                case 1:
                    ball2.GetComponent<Image>().sprite = _ballAngrySprite;
                    break;
                case 2:
                    ball2.GetComponent<Image>().sprite = _ballLoveSprite;
                    break;
                case 3:
                    ball2.GetComponent<Image>().sprite = _ballPlaySprite;
                    break;
                case 5:
                    ball2.GetComponent<Image>().sprite = _ballSorrowSprite;
                    break;
                default:
                    break;
            }
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
