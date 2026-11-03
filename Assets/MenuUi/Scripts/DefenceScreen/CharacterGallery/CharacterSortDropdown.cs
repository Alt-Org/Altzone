using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Lobby;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.CharacterGallery
{
    /// <summary>
    /// The CharacterFilterPanel script's custom dropdown component, serving two purposes:
    /// 1. Improves UI performance by using an event-driven approach (DropdownStateChanged) 
    ///    to handle the dropdown arrow state, replacing the need for an Update loop in the CharacterFilterPanel.cs.
    /// 2. Visually locks specific dropdown filtering options by overlaying the NotInUsePanel prefab
    ///    over not yet available options (currently RarestFirst and RarestLast) and disabling interaction.
    ///    Can be used to lock/unlock other filtering options as needed.
    /// </summary>
    public class CharacterSortDropdown : TMP_Dropdown
    {
        public event Action<bool> DropdownStateChanged;

        /// <summary>
        /// Creates the dropdown list and starts a coroutine to update the instantiated item visuals.
        /// </summary>
        /// <param name="template">The template game object for the dropdown list.</param>
        /// <returns>The created dropdown list game object.</returns>
        protected override GameObject CreateDropdownList(GameObject template)
        {
            BuildOptions();

            GameObject dropdownList = base.CreateDropdownList(template);

            StartCoroutine(UpdateDropdownItems(dropdownList));

            DropdownStateChanged?.Invoke(true);
            return dropdownList;
        }

        /// <summary>
        /// Updates the visuals of the instantiated dropdown items after a short delay.
        /// Visually locks specific sorting options that are currently unavailable in the game.
        /// </summary>
        /// <param name="dropdownList">The instantiated dropdown list containing the items.</param>
        private IEnumerator UpdateDropdownItems(GameObject dropdownList)
        {
            yield return new WaitForEndOfFrame();

            //This still need a lot of cleaning.
            for (int i = 1; i < dropdownList.transform.GetChild(0).GetChild(0).childCount; i++)
            {
                CharacterSortHandler handler = dropdownList.transform.GetChild(0).GetChild(0).GetChild(i).GetComponent<CharacterSortHandler>();
                var sortOrder = (CharacterSortOrder)i-1;

                bool isNotAvailable = sortOrder == CharacterSortOrder.RarestFirst ||
                                      sortOrder == CharacterSortOrder.RarestLast;

                if (isNotAvailable)
                {
                    GameObject notInUsePanel = handler.NotInUsePanel;

                    if (notInUsePanel)
                    {
                        notInUsePanel.SetActive(true);
                    }

                    handler.Toggle.interactable = false;
                }
            }
        }

        private void BuildOptions()
        {
            options.Clear();

            List<CharacterSortOrder> enumList = Enum.GetValues(typeof(CharacterSortOrder)).Cast<CharacterSortOrder>().ToList();

            foreach (CharacterSortOrder enumValue in enumList)
            {
                options.Add(new(enumValue.GetString()));
            }
        }

        /// <summary>
        /// Destroys the dropdown list and invokes the state changed event.
        /// </summary>
        /// <param name="dropdownList">The dropdown list to be destroyed.</param>
        protected override void DestroyDropdownList(GameObject dropdownList)
        {
            base.DestroyDropdownList(dropdownList);
            DropdownStateChanged?.Invoke(false);
        }
    }
}
