using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUi.Scripts.DefenceScreen.CharacterStatsWindow
{
    public class StatUpdatePopUp : MonoBehaviour
    {
        [SerializeField] private GameObject _contents;


        private void Awake()
        {
            ClosePopUp();
        }


        /// <summary>
        /// Open StatUpdatePopUp
        /// </summary>
        public void OpenPopUp()
        {
            _contents.SetActive(true);
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
