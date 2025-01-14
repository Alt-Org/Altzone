using System.Diagnostics.CodeAnalysis;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.Config.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/CharacterSpec", fileName = nameof(CharacterSpec) + "_ID")]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CharacterSpec : ScriptableObject
    {
        public string Id;
        public string Name;
        public CharacterClassID ClassType;

        public override string ToString()
        {
            return $"{Id}:{ClassType}:{Name}";
        }
    }
}
