using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum InfoType
{
    LogIn,
    CheckSettingsData,
    FetchPlayerData,
    FetchClanData,
    Finished
}

namespace MenuUi.Scripts.Loader
{
    public class LoadingInfoController : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _loadingText;
        [SerializeField]
        private TextMeshProUGUI _infoText;

        // Start is called before the first frame update
        void Start()
        {

        }

        private void OnEnable()
        {
            StartCoroutine(LoadingProgress());
        }

        public void SetInfoText(InfoType type)
        {
            switch (type)
            {
                case InfoType.LogIn:
                    _infoText.text = "Kirjaudutaan sisään";
                    break;
                case InfoType.FetchPlayerData:
                    _infoText.text = "Haetaan pelaajan tietoja";
                    break;
                case InfoType.FetchClanData:
                    _infoText.text = "Haetaan klaanin tietoja";
                    break;
                case InfoType.Finished:
                    _infoText.text = "Siirrytään pääikkunaan";
                    break;
                case InfoType.CheckSettingsData:
                    _infoText.text = "Tarkistetaan asetuksia";
                    break;
                default:
                    break;
            }

            if (!gameObject.activeSelf) gameObject.SetActive(true);

        }

        public void DisableInfo()
        {
            gameObject.SetActive(false);
        }

        private IEnumerator LoadingProgress()
        {
            int i=0;
            while (true)
            {
                yield return new WaitForSeconds(1);
                i++;
                if (i == 4) i = 1;
                _loadingText.text = "Ladataan"+ new string('.',i);
            }
        }
    }
}
