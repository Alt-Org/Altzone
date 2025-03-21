using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Storage
{
    public class SetFilterHandler : MonoBehaviour
    {
        [SerializeField] private StorageFurnitureReference _furnitureReference;
        [SerializeField] private GameObject _togglePrefab;

        public List<Toggle> toggleList;

        public void CreateSetFilterButtons()
        {
            if (transform.childCount > 0) return;

            for (int i = 0; i < _furnitureReference.Info.Count; i++)
            {
                GameObject toggleButton = Instantiate(_togglePrefab, transform);
                toggleButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = _furnitureReference.Info[i].SetName;

                toggleList.Add(toggleButton.GetComponent<Toggle>());
            }
        }
    }

}
