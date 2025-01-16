using System.Linq;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;


namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatsWindowController : MonoBehaviour
    {
        private PlayerData _playerData;
        private CharacterID _characterId;
        private CustomCharacter _currentCharacter;


        private void OnEnable()
        {
            _characterId = (CharacterID)SettingsCarrier.Instance.CharacterGalleryCharacterStatWindowToShow;
            SetPlayerData();
            SetCurrentCharacter();
        }


        private void SetPlayerData() // Get player data from data store and set it to variable
        {
            DataStore dataStore = Storefront.Get();
            dataStore.GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, playerData =>
            {
                if (playerData == null)
                {
                    Debug.Log("GetPlayerData is null");
                    return;
                }
                _playerData = playerData;
            });
        }


        private void SetCurrentCharacter() // Get current custom character from player data and set it to variable
        {
            _currentCharacter = _playerData.CustomCharacters.FirstOrDefault(c => c.Id == _characterId);
        }


        /// <summary>
        /// Get currently displayed character's class id.
        /// </summary>
        /// <returns>Current character's CharacterClassID.</returns>
        public CharacterClassID GetCurrentCharacterClass()
        {
            return CustomCharacter.GetClassID(_characterId);
        }


        /// <summary>
        /// Get currently displayed character's name.
        /// </summary>
        /// <returns>Current character's name as string.</returns>
        public string GetCurrentCharacterName()
        {
            return _currentCharacter.CharacterName;
        }


        /// <summary>
        /// Get currently displayed character's sprite image.
        /// </summary>
        /// <returns>Current character's sprite.</returns>
        public Sprite GetCurrentCharacterSprite()
        {
            return null;
        }


        /// <summary>
        /// Get currently displayed character's description.
        /// </summary>
        /// <returns>Current character's description as string.</returns>
        public string GetCurrentCharacterDescription()
        {
            return "Hahmon kuvaus";
        }


        /// <summary>
        /// Get currently displayed character's special ability description.
        /// </summary>
        /// <returns>Current character's special ability as string.</returns>
        public string GetCurrentCharacterSpecialAbilityDescription()
        {
            return "Hahmon erikoistaidon kuvaus";
        }


        /// <summary>
        /// Get currently displayed character's story.
        /// </summary>
        /// <returns>Current character's story as string.</returns>
        public string GetCurrentCharacterStory()
        {
            return "Hahmon taustatarina";
        }


        /// <summary>
        /// Get currently displayed character's wins.
        /// </summary>
        /// <returns>Current character's wins as int.</returns>
        public int GetCurrentCharacterWins()
        {
            return -1;
        }


        /// <summary>
        /// Get currently displayed character's losses.
        /// </summary>
        /// <returns>Current character's losses as int.</returns>
        public int GetCurrentCharacterLosses()
        {
            return -1;
        }


        /// <summary>
        /// Get player's eraser amount from player data.
        /// </summary>
        /// <returns>Player's eraser amount as int.</returns>
        public int GetEraserAmount()
        {
            return _playerData.Eraser;
        }


        /// <summary>
        /// Try to decrease player's eraser amount by 1.
        /// </summary>
        /// <returns>If decreasing was successful. If true, player's erasers were decreased by 1. If false, player didn't have enough erasers.</returns>
        public bool TryDecreaseEraser()
        {
            if (_playerData.Eraser > 0)
            {
                _playerData.Eraser--;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Get player's diamond amount from player data.
        /// </summary>
        /// <returns>Player's diamond amount as int.</returns>
        public int GetDiamondAmount()
        {
            return _playerData.DiamondSpeed; // using DiamondSpeed as a placeholder
        }


        /// <summary>
        /// Try to decrease player's diamonds by the amount specified.
        /// </summary>
        /// <param name="amount">The amount how many diamonds will be decreased from the player.</param>
        /// <returns>If decreasing was successful. If true, player's diamonds were decreased by 1. If false, player didn't have enough diamonds.</returns>
        public bool TryDecreaseDiamonds(int amount)
        {
            if (_playerData.DiamondSpeed >= amount) // using DiamondSpeed as a placeholder
            {
                _playerData.DiamondSpeed -= amount;
                return true;
            }
            else
            {
                return false;
            }
        }


        /// <summary>
        /// Get how many diamonds it costs to upgrade currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which diamond cost will be checked.</param>
        /// <returns>Stat diamond upgrade cost as int.</returns>
        public int GetDiamondCost(StatType statType)
        {
            return _currentCharacter.GetPriceToNextLevel(statType);
        }


        /// <summary>
        /// Get currently displayed character's stat value according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to get.</param>
        /// <returns>Stat value as int.</returns>
        public int GetStat(StatType statType)
        {
            switch (statType)
            {
                case StatType.None:
                    return -1;
                case StatType.Speed:
                    return _currentCharacter.Speed;
                case StatType.Attack:
                    return _currentCharacter.Attack;
                case StatType.Hp:
                    return _currentCharacter.Hp;
                case StatType.Resistance:
                    return _currentCharacter.Resistance;
                case StatType.Defence:
                    return _currentCharacter.Defence;
            }
            return -1;
        }


        /// <summary>
        /// Get currently displayed character's base stat without upgrades according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to get.</param>
        /// <returns>Base stat value as int.</returns>
        public int GetBaseStat(StatType statType)
        {
            return -1;
        }


        /// <summary>
        /// Try to increase currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to increase.</param>
        /// <returns>If increasing the stat was successful as bool.</returns>
        public bool TryIncreaseStat(StatType statType)
        {
            bool success = false;

            switch (statType)
            {
                case StatType.None:
                    break;
                case StatType.Speed:
                    break;
                case StatType.Attack:
                    break;
                case StatType.Hp:
                    break;
                case StatType.Resistance:
                    break;
                case StatType.Defence:
                    break;
            }

            if (success)
            {
                _playerData.UpdateCustomCharacter(_currentCharacter);
            }

            return success;
        }


        /// <summary>
        /// Try to decrease currently displayed character's stat according to the stat type.
        /// </summary>
        /// <param name="statType">The stat type which to decrease.</param>
        /// <returns>If decreasing the stat was successful as bool.</returns>
        public bool TryDecreaseStat(StatType statType)
        {
            bool success = false;

            switch (statType)
            {
                case StatType.None:
                    break;
                case StatType.Speed:
                    break;
                case StatType.Attack:
                    break;
                case StatType.Hp:
                    break;
                case StatType.Resistance:
                    break;
                case StatType.Defence:
                    break;
            }

            if (success)
            {
                _playerData.UpdateCustomCharacter(_currentCharacter);
            }

            return success;
        }
    }
}
