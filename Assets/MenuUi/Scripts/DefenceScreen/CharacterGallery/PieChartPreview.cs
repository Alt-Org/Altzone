using System.Collections.Generic;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using Altzone.Scripts.Config;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Player;
using System.Linq;


namespace MenuUi.Scripts.DefenceScreen.CharacterGallery
{
    public class PieChartPreview : MonoBehaviour
    {
        [SerializeField] private PiechartReference _referenceSheet;

        private Color _impactForceColor;
        private Color _healthPointsColor;
        private Color _defenceColor;
        private Color _characterSizeColor;
        private Color _speedColor;
        private Color _defaultColor;
        private Sprite _circleSprite;


        private void Awake() 
        {
            // caching colors and circle sprite from the reference sheet to avoid unneccessary function calls
            _impactForceColor = _referenceSheet.GetColor(StatType.Attack);
            _healthPointsColor = _referenceSheet.GetColor(StatType.Hp);
            _defenceColor = _referenceSheet.GetColor(StatType.Defence);
            _characterSizeColor = _referenceSheet.GetColor(StatType.CharacterSize);
            _speedColor = _referenceSheet.GetColor(StatType.Speed);
            _defaultColor = _referenceSheet.GetColor(StatType.None);
            _circleSprite = _referenceSheet.GetCircleSprite();
        }


        /// <summary>
        /// Update/Create piechart preview based on characterId.
        /// </summary>
        /// <param name="characterId">Character Id whose piechart preview to show.</param>
        public void UpdateChart(CharacterID characterId)
        {
            // Get CustomCharacter
            PlayerData playerData = null;
            CustomCharacter customCharacter = null;

            DataStore dataStore = Storefront.Get();
            dataStore.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData_ =>
            {
                if (playerData_ == null)
                {
                    Debug.Log("GetPlayerData is null");
                    return;
                }
                playerData = playerData_;
            });

            if (playerData != null)
            {
                customCharacter = playerData.CustomCharacters.FirstOrDefault(c => c.Id == characterId);
            }

            if (customCharacter == null)
            {
                return;
            }

            // Destroy old pie slices
            for (int i = 0; i < transform.childCount; i++)
            {
                Destroy(transform.GetChild(i).gameObject);
            }

            // Get stats
            int impactForce = customCharacter.Attack;
            int healthPoints = customCharacter.Hp;
            int defence = customCharacter.Defence;
            int characterSize = customCharacter.CharacterSize;
            int speed = customCharacter.Speed;

            UpdateChart(impactForce,healthPoints, defence, characterSize, speed);
        }


        /// <summary>
        /// Update/create piechart preview based on individual stat numbers.
        /// </summary>
        /// <param name="impactForce">Impact force stat level as integer.</param>
        /// <param name="healthPoints">Health points stat level as integer.</param>
        /// <param name="defence">Defence stat level as integer.</param>
        /// <param name="characterSize">Character size stat level as integer.</param>
        /// <param name="speed">Speed stat level as integer.</param>
        public void UpdateChart(int impactForce, int healthPoints, int defence, int characterSize, int speed)
        {
            // Arrange stats
            var stats = new List<(int level, Color color)>
            {
                (defence, _defenceColor),
                (characterSize, _characterSizeColor),
                (speed, _speedColor),
                (healthPoints, _healthPointsColor),
                (impactForce, _impactForceColor),
            };

            // Create slices
            int maxCombinedStatLevel = CustomCharacter.STATMAXCOMBINED;
            float oneLevelFillAmount = 1.0f / maxCombinedStatLevel;
            float currentSliceFill = 1.0f;

            foreach (var stat in stats) // Colored slices
            {
                CreateSlice(currentSliceFill, stat.color);
                currentSliceFill -= stat.level * oneLevelFillAmount;

                if (!(currentSliceFill > 0.0f))
                {
                    return;
                }
            }

            if (currentSliceFill > 0.0f) // White slice
            {
                CreateSlice(currentSliceFill, _defaultColor);
            }
        }


        private void CreateSlice(float fillAmount, Color color)
        {
            // Create gameobject and add components
            GameObject slice = new GameObject();
            slice.AddComponent<RectTransform>();
            slice.AddComponent<Image>();

            // Modify image properties
            Image sliceImage = slice.GetComponent<Image>();
            
            sliceImage.sprite = _circleSprite;

            sliceImage.color = color;
            sliceImage.type = Image.Type.Filled;
            sliceImage.fillClockwise = false;
            sliceImage.fillOrigin = (int)Image.Origin360.Top;
            sliceImage.preserveAspect = true;
            sliceImage.raycastTarget = false;
            sliceImage.fillAmount = fillAmount;

            // Reparent to this node
            slice.transform.SetParent(transform);

            // Set scale
            slice.transform.localScale = Vector3.one;

            // Set anchors
            RectTransform sliceRect = slice.GetComponent<RectTransform>();
            sliceRect.offsetMax = Vector2.zero;
            sliceRect.offsetMin = Vector2.zero;
            sliceRect.anchorMin = Vector3.zero;
            sliceRect.anchorMax = Vector3.one;
        }
    }
}
