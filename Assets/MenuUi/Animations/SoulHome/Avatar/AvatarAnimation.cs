using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2.Internal;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarAnimation", fileName = nameof(AvatarAnimation) + "_name")]
public class AvatarAnimation : ScriptableObject
{
    [SerializeField] private CharacterClassType _validClassType;
    [SerializeField] private AnimationClip _clip;

    public CharacterClassType ValidClass { get => _validClassType;}
    public AnimationClip Clip { get => _clip;}
}
