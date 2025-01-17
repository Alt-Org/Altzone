using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ModelV2.Internal;
using UnityEngine;

namespace Altzone.Scripts.ModelV2
{
    /// <summary>
    /// General contract for player character prototype.
    /// </summary>
    public class PlayerCharacterPrototype
    {
        public string Id => _characterSpec.Id;
        public CharacterClassID ClassType => _characterSpec.ClassType;
        public string Name => _characterSpec.Name;
        public Sprite GalleryImage => _characterSpec.GalleryImage;

        private readonly CharacterSpec _characterSpec;

        internal PlayerCharacterPrototype(CharacterSpec characterSpec)
        {
            _characterSpec = characterSpec;
        }

        public override string ToString() => $"{_characterSpec}";
    }
}
