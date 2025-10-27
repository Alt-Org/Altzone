using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Altzone.Scripts.AvatarPartsInfo
{
    [CreateAssetMenu(fileName = "AvatarPartInfo", menuName = "Avatar/Avatar Parts Info")]
    public class AvatarPartInfo : ScriptableObject
    {
        [Header("Basic Info")]
        [SerializeField] private string _name = "";
        [SerializeField] private string _id = "";
        [SerializeField] private string _visibleName = "";

        [Header("Visual Assets")]
        [SerializeField] private Sprite _avatarImage;
        [SerializeField] private Sprite _iconImage;

        public string Name { get; }
        public string Id { get; }
        public string VisibleName { get; }
        public Sprite AvatarImage { get; }
        public Sprite IconImage { get; }

        public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Id);
    }

}




