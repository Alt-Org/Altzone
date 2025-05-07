using UnityEngine;

namespace Altzone.Scripts.Settings
{
    public class PlayerSettings
    {
        public string PlayerGuid
        {
            get => PlayerPrefs.GetString("PlayerGuid", "12345");
            set => PlayerPrefs.SetString("PlayerGuid", value);
        }

        public string PhotonRegion
        {
            get => PlayerPrefs.GetString("PhotonRegion", "eu");
            set => PlayerPrefs.SetString("PhotonRegion", value);
        }
        
        public int SelectedCharacter
        {
            get => PlayerPrefs.GetInt("SelectedCharacter", -1);
            set => PlayerPrefs.SetInt("SelectedCharacter", value);
        }
    }
}
