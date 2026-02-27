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
        [SerializeField] private Sprite _maskImage;
        [SerializeField] private Sprite _iconImage;

        public string Name { get => _name; }
        public string Id { get => _id; }
        public string VisibleName { get => _visibleName; }
        public Sprite AvatarImage { get => _avatarImage; }
        public Sprite MaskImage { get => _maskImage; }
        public Sprite IconImage { get => _iconImage; }

        public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Id);
    }

}




