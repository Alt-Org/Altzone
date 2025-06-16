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
        /// <param name="classId">The class id which name to get.</param>
        /// <returns>Class name as string.</returns>
        public string GetName(CharacterClassID classId)
        {
            switch (classId)
            {
                case CharacterClassID.Desensitizer:
                    return _desensitizerName;
                case CharacterClassID.Trickster:
                    return _tricksterName;
                case CharacterClassID.Obedient:
                    return _obedientName;
                case CharacterClassID.Projector:
                    return _projectorName;
                case CharacterClassID.Retroflector:
                    return _retroflectorName;
                case CharacterClassID.Confluent:
                    return _confluentName;
                case CharacterClassID.Intellectualizer:
                    return _intellectualizerName;
            }

            return "No class name";
        }


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
