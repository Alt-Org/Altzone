using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.AvatarEditor
{
public class SaveButtonScript : MonoBehaviour
    {
        public Button saveButton; 
        public TMP_InputField nameInput;

        private PlayerAvatar _playerAvatar;


        private void OnEnable()
        {
            LoadAvatarData();
            saveButton.onClick.AddListener(SaveAvatarData); 
        }
        private void LoadAvatarData(){
            _playerAvatar = new PlayerAvatar(PlayerPrefs.GetString("CharacterName"), LoadFeatures(), LoadColors());
            nameInput.text = _playerAvatar.Name;
            
        }
        private List<FeatureID> LoadFeatures(){
            List<FeatureID> features = new();
            for(int i = 0; i < 8; i++){
                features.Add((FeatureID)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()));
            }
            return features;
        }
        private List<FeatureColor> LoadColors(){
            List<FeatureColor> colors = new();
            for (int i = 0; i < 8; i++){
                colors.Add((FeatureColor)PlayerPrefs.GetInt(((FeatureSlot)i).ToString()));
            }
            return colors;
        }

        private void SaveAvatarData(){
            SaveName();
        }
        private void SaveName()
        {
            string characterName = nameInput.text;
            Debug.Log("Character name saved: " + characterName);

            _playerAvatar.Name = characterName;
            PlayerPrefs.SetString("CharacterName", characterName);
            PlayerPrefs.Save();
        }
        private void SaveFeatures(){
            
        }
    }
}
