using MenuUI.Scripts.SoulHome;
using UnityEngine;

public class AnimationEventBridge : MonoBehaviour
{
    [SerializeField] private SoulHomeAvatarController _controller;

    public void StartWaving() => _controller.StartWaving();
    public void EndWaving() => _controller.EndWaving();
}
