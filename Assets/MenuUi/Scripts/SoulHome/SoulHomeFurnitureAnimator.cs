using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Lobby;
using MenuUI.Scripts.SoulHome;
using UnityEngine;
using static MenuUI.Scripts.SoulHome.FurnitureHandling;

[RequireComponent(typeof(FurnitureHandling)), RequireComponent(typeof(Animator))]
public class SoulHomeFurnitureAnimator : MonoBehaviour, ISoulHomeObjectClick
{
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip _frontAnimation;
    [SerializeField] private AnimationClip _leftAnimation;
    [SerializeField] private AnimationClip _rightAnimation;

    [SerializeField] private AnimationClip _frontResetAnimation;
    [SerializeField] private AnimationClip _leftResetAnimation;
    [SerializeField] private AnimationClip _rightResetAnimation;

    private FurnitureHandling _furnitureHandling;
    private bool _isPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        _furnitureHandling = GetComponent<FurnitureHandling>();
        _furnitureHandling.OnFurnitureRotated += RotateAnimation;
    }

    private void OnDestroy()
    {
        _furnitureHandling.OnFurnitureRotated -= RotateAnimation;
    }

    public void HandleClick() => StartCoroutine(Animation());

    private IEnumerator Animation()
    {
        if (_isPlaying) yield break;
        float animationLength = PlayBattleStart();

        yield return new WaitForSeconds(animationLength);
        _furnitureHandling.ResetDirection();
        _isPlaying = false;
    }

    private float PlayBattleStart()
    {
        AnimationClip clip = null;
        if (_furnitureHandling.TempSpriteDirection is FurnitureHandling.Direction.Front)
            clip = _frontAnimation;
        else if (_furnitureHandling.TempSpriteDirection is FurnitureHandling.Direction.Left)
            clip = _leftAnimation;
        else if (_furnitureHandling.TempSpriteDirection is FurnitureHandling.Direction.Right)
            clip = _rightAnimation;

        if(clip == null) return 0;

        _animator.Play(clip.name);
        _isPlaying = true;
        return clip.length;
    }

    private void RotateAnimation(Direction direction)
    {
        AnimationClip clip = null;
        if (direction is FurnitureHandling.Direction.Front)
            clip = _frontResetAnimation;
        else if (direction is FurnitureHandling.Direction.Left)
            clip = _leftResetAnimation;
        else if (direction is FurnitureHandling.Direction.Right)
            clip = _rightResetAnimation;

        if (clip == null) return;

        _animator.Play(clip.name);
    }
}
