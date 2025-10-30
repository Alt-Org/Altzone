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

        public string Name { get => _name; set => _name = value; }
        public string Id { get => _id; set => _id = value; }
        public string VisibleName { get => _visibleName; set => _visibleName = value; }
        public Sprite AvatarImage { get => _avatarImage; set => _avatarImage = value; }
        public Sprite IconImage { get => _iconImage; set => _iconImage = value; }

        public bool IsValid => !string.IsNullOrEmpty(Name) && !string.IsNullOrEmpty(Id);
    }

}




