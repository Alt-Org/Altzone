using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace Altzone.Scripts.ReferenceSheets
{
    //[CreateAssetMenu(menuName = "ALT-Zone/ClassReference", fileName = "ClassReference")]
    public class ClassReference : ScriptableObject
    {
        [SerializeField] private string _desensitizerName;
        [SerializeField] private Color _desensitizerColor;
        [SerializeField] private Color _desensitizerAlternativeColor;

        [Space, SerializeField] private string _tricksterName;
        [SerializeField] private Color _tricksterColor;
        [SerializeField] private Color _tricksterAlternativeColor;

        [Space, SerializeField] private string _obedientName;
        [SerializeField] private Color _obedientColor;
        [SerializeField] private Color _obedientAlternativeColor;

        [Space, SerializeField] private string _projectorName;
        [SerializeField] private Color _projectorColor;
        [SerializeField] private Color _projectorAlternativeColor;

        [Space, SerializeField] private string _retroflectorName;
        [SerializeField] private Color _retroflectorColor;
        [SerializeField] private Color _retroflectorAlternativeColor;

        [Space, SerializeField] private string _confluentName;
        [SerializeField] private Color _confluentColor;
        [SerializeField] private Color _confluentAlternativeColor;

        [Space, SerializeField] private string _intellectualizerName;
        [SerializeField] private Color _intellectualizerColor;
        [SerializeField] private Color _intellectualizerAlternativeColor;


        /// <summary>
        /// Get character class name.
        /// </summary>
        /// <param name="classType">The class id which name to get.</param>
        /// <returns>Class name as string.</returns>
        public string GetName(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerName;
                case CharacterClassType.Trickster:
                    return _tricksterName;
                case CharacterClassType.Obedient:
                    return _obedientName;
                case CharacterClassType.Projector:
                    return _projectorName;
                case CharacterClassType.Retroflector:
                    return _retroflectorName;
                case CharacterClassType.Confluent:
                    return _confluentName;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerName;
            }

            return "No class name";
        }


        /// <summary>
        /// Get character class color.
        /// </summary>
        /// <param name="classType">The class id which color to get.</param>
        /// <returns>Class color.</returns>
        public Color GetColor(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerColor;
                case CharacterClassType.Trickster:
                    return _tricksterColor;
                case CharacterClassType.Obedient:
                    return _obedientColor;
                case CharacterClassType.Projector:
                    return _projectorColor;
                case CharacterClassType.Retroflector:
                    return _retroflectorColor;
                case CharacterClassType.Confluent:
                    return _confluentColor;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerColor;
            }
            return Color.gray;
        }


        /// <summary>
        /// Get character class alternative (lighter) color.
        /// </summary>
        /// <param name="classType">The class id which alternative color to get.</param>
        /// <returns>Class alternative color.</returns>
        public Color GetAlternativeColor(CharacterClassType classType)
        {
            switch (classType)
            {
                case CharacterClassType.Desensitizer:
                    return _desensitizerAlternativeColor;
                case CharacterClassType.Trickster:
                    return _tricksterAlternativeColor;
                case CharacterClassType.Obedient:
                    return _obedientAlternativeColor;
                case CharacterClassType.Projector:
                    return _projectorAlternativeColor;
                case CharacterClassType.Retroflector:
                    return _retroflectorAlternativeColor;
                case CharacterClassType.Confluent:
                    return _confluentAlternativeColor;
                case CharacterClassType.Intellectualizer:
                    return _intellectualizerAlternativeColor;
            }
            return Color.gray;
        }
    }
}
