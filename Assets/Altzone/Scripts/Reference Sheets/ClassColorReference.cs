using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ClassColorReference", fileName = "ClassColorReference")]
    public class ClassColorReference : ScriptableObject
    {
        [SerializeField] private Color _desensitizerColor;
        [SerializeField] private Color _desensitizerAlternativeColor;

        [SerializeField] private Color _tricksterColor;
        [SerializeField] private Color _tricksterAlternativeColor;

        [SerializeField] private Color _obedientColor;
        [SerializeField] private Color _obedientAlternativeColor;

        [SerializeField] private Color _projectorColor;
        [SerializeField] private Color _projectorAlternativeColor;

        [SerializeField] private Color _retroflectorColor;
        [SerializeField] private Color _retroflectorAlternativeColor;

        [SerializeField] private Color _confluentColor;
        [SerializeField] private Color _confluentAlternativeColor;

        [SerializeField] private Color _intellectualizerColor;
        [SerializeField] private Color _intellectualizerAlternativeColor;


        /// <summary>
        /// Get character class color.
        /// </summary>
        /// <param name="classId">The class id which color to get.</param>
        /// <returns>Class color.</returns>
        public Color GetColor(CharacterClassID classId)
        {
            switch (classId)
            {
                case CharacterClassID.Desensitizer:
                    return _desensitizerColor;
                case CharacterClassID.Trickster:
                    return _tricksterColor;
                case CharacterClassID.Obedient:
                    return _obedientColor;
                case CharacterClassID.Projector:
                    return _projectorColor;
                case CharacterClassID.Retroflector:
                    return _retroflectorColor;
                case CharacterClassID.Confluent:
                    return _confluentColor;
                case CharacterClassID.Intellectualizer:
                    return _intellectualizerColor;
            }
            return Color.gray;
        }


        /// <summary>
        /// Get character class alternative (lighter) color.
        /// </summary>
        /// <param name="classId">The class id which alternative color to get.</param>
        /// <returns>Class alternative color.</returns>
        public Color GetAlternativeColor(CharacterClassID classId)
        {
            switch (classId)
            {
                case CharacterClassID.Desensitizer:
                    return _desensitizerAlternativeColor;
                case CharacterClassID.Trickster:
                    return _tricksterAlternativeColor;
                case CharacterClassID.Obedient:
                    return _obedientAlternativeColor;
                case CharacterClassID.Projector:
                    return _projectorAlternativeColor;
                case CharacterClassID.Retroflector:
                    return _retroflectorAlternativeColor;
                case CharacterClassID.Confluent:
                    return _confluentAlternativeColor;
                case CharacterClassID.Intellectualizer:
                    return _intellectualizerAlternativeColor;
            }
            return Color.gray;
        }
    }
}
