using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen
{
    //[CreateAssetMenu(menuName = "ALT-Zone/PiechartReference", fileName = "PiechartReference")]
    public class PiechartReference : ScriptableObject
    {
        [SerializeField] private Color _impactForceColor;
        [SerializeField] private Color _impactForceAlternativeColor;

        [SerializeField] private Color _healthPointsColor;
        [SerializeField] private Color _healthPointsAlternativeColor;

        [SerializeField] private Color _defenceColor;
        [SerializeField] private Color _defenceAlternativeColor;

        [SerializeField] private Color _characterSizeColor;
        [SerializeField] private Color _characterSizeAlternativeColor;

        [SerializeField] private Color _speedColor;
        [SerializeField] private Color _speedAlternativeColor;

        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _defaultAlternativeColor;

        [SerializeField] private Sprite _circleSprite;
        [SerializeField] private Sprite _circlePatternedSprite;


        /// <summary>
        /// Get color from the reference sheet which matches the stat type. None is for default color.
        /// </summary>
        /// <param name="statType">Stat type which color to get.</param>
        /// <returns>Color for the stat type.</returns>
        public Color GetColor(StatType statType = StatType.None)
        {
            switch (statType)
            {
                case StatType.None:
                    return _defaultColor;
                case StatType.Attack:
                    return _impactForceColor;
                case StatType.Defence:
                    return _defenceColor;
                case StatType.Hp:
                    return _healthPointsColor;
                case StatType.CharacterSize:
                    return _characterSizeColor;
                case StatType.Speed:
                    return _speedColor;
            }
            return _defaultColor;
        }


        /// <summary>
        /// Get alternative color from the reference sheet which matches the stat type. None is for default alternative color.
        /// </summary>
        /// <param name="statType">Stat type which alternative color to get.</param>
        /// <returns>Alternative Color for the stat type.</returns>
        public Color GetAlternativeColor(StatType statType = StatType.None)
        {
            switch (statType)
            {
                case StatType.None:
                    return _defaultAlternativeColor;
                case StatType.Attack:
                    return _impactForceAlternativeColor;
                case StatType.Defence:
                    return _defenceAlternativeColor;
                case StatType.Hp:
                    return _healthPointsAlternativeColor;
                case StatType.CharacterSize:
                    return _characterSizeAlternativeColor;
                case StatType.Speed:
                    return _speedAlternativeColor;
            }
            return _defaultAlternativeColor;
        }


        /// <summary>
        /// Get circle sprite used for the piechart.
        /// </summary>
        /// <returns>Circle sprite.</returns>
        public Sprite GetCircleSprite()
        {
            return _circleSprite;
        }


        /// <summary>
        /// Get patterned circle sprite used for the piechart.
        /// </summary>
        /// <returns>Patterned circle sprite.</returns>
        public Sprite GetPatternedSprite()
        {
            return _circlePatternedSprite;
        }
    }
}
