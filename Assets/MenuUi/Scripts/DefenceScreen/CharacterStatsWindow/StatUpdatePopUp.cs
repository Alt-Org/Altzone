using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatUpdatePopUp : MonoBehaviour
    {
        [SerializeField] private StatsReference _statsReference;
        [SerializeField] private GameObject _contents;
        [SerializeField] private Image _statIcon;
        [SerializeField] private TMP_Text _statName;
        [SerializeField] private TMP_Text _statNumber;
        [SerializeField] private TMP_Text _diamondCost;


        private void Awake()
        {
            ClosePopUp();
        }


        /// <summary>
        /// Open StatUpdatePopUp
        /// </summary>
        /// <param name="statType">Stat type int which is compared to StatType enum. (is there a better way to do this?)</param>
        public void OpenPopUp(int statType)
        {
            StatInfo statInfo = null;

            // Getting StatInfo from StatsReference sheet
            switch ((StatType)statType)
            {
                case StatType.None:
                    ClosePopUp();
                    break;
                case StatType.Attack:
                    statInfo = _statsReference.GetStatInfo(StatType.Attack);
                    break;
                case StatType.Defence:
                    statInfo = _statsReference.GetStatInfo(StatType.Defence);
                    break;
                case StatType.Resistance:
                    statInfo = _statsReference.GetStatInfo(StatType.Resistance);
                    break;
                case StatType.Hp:
                    statInfo = _statsReference.GetStatInfo(StatType.Hp);
                    break;
                case StatType.Speed:
                    statInfo = _statsReference.GetStatInfo(StatType.Speed);
                    break;
            }

            // Setting stat icon and stat name
            if (statInfo != null)
            {
                _statIcon.sprite = statInfo.Image;
                _statName.text = statInfo.Name;
                _contents.SetActive(true);
            }
            else
            {
                ClosePopUp();
            }
        }


        /// <summary>
        /// Close StatUpdatePopUp
        /// </summary>
        public void ClosePopUp()
        {
            _contents.SetActive(false);
        }
    }
}
