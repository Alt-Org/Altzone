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

    [SerializeField]
    private Transform _path1Position1;
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
    [SerializeField]
    private AnimationCurve _path3Curve;

    [SerializeField]
    private Animator _characterAnimator1;
    [SerializeField]
    private Animator _characterAnimator2;

    // Start is called before the first frame update
    void Start()
    {
        _exitButton.onClick.AddListener(ExitStory);
    }


    public IEnumerator PlayAnimation()
    {
        var clips1 = _characterAnimator1.runtimeAnimatorController.animationClips;
        int clips1Count = clips1.Length;
        List<int> randomClipOrder1 = new();
        List<int> randomBallOrder1 = new();
        var clips2 = _characterAnimator1.runtimeAnimatorController.animationClips;
        int clips2Count = clips2.Length;
        List<int> randomClipOrder2 = new();
        List<int> randomBallOrder2 = new();
        int prevSelectedValue1 = -1;
        int selectedvalue1 = -1;
        int prevSelectedValue2 = -1;
        int selectedvalue2 = -1;
        for (int i= 0; i>5; i++)
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

        for (int i = 0; i > randomClipOrder1.Count; i++)
        {
            _characterAnimator1.Play(clips1[randomClipOrder1[i]].name);
            GameObject ball = Instantiate(_emotionBall, _endStartPositionLeft);
            bool ballDone = false;
            switch (randomBallOrder1[i])
            {
                case 0:
                    StartCoroutine(BallAnimationLeft1(ball, done => ballDone = done));
                    break;
                case 1:
                    break;
                case 2:
                    break;
                default:
                    break;
            }
            yield return new WaitUntil(() => ballDone is true);
            _characterAnimator2.Play(clips2[randomClipOrder2[i]].name);
        }
    }

    private void BallAnimationRight1(GameObject ball)
    {

    }

    private IEnumerator BallAnimationLeft1(GameObject ball, Action<bool> callback)
    {
        yield return null;
    }

    private void ExitStory()
    {
        LobbyManager.ExitBattleStory();
    }

}
