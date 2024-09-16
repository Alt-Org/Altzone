using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Loader
{
    public class AgeVerificationHandler : MonoBehaviour
    {
        [SerializeField] ToggleGroup _toggleGroup;
        [SerializeField] Toggle _ageToggle;
        [SerializeField] Toggle _parentalAuthToggle;
        [SerializeField] Button _button;
        private bool _finished = false;

        public bool Finished { get => _finished;}

        public void CheckToggle()
        {
            if (_toggleGroup.AnyTogglesOn())
            {
                _button.interactable = true;
            }
            else
            {
                _button.interactable = false;
            }
        }

        public void UpdateProfile()
        {
            ServerPlayer player = ServerManager.Instance.Player;

            if (_ageToggle.isOn) player.above13 = true;
            else player.above13 = false;
            if (_parentalAuthToggle.isOn) player.parentalAuth = true;
            else player.parentalAuth = false;

            string body = "{\"_id\":\""+ ServerManager.Instance.Player._id + "\",\"above13\":"+ player.above13.ToString().ToLower() + ",\"parentalAuth\":" + player.parentalAuth.ToString().ToLower() + "}";

            StartCoroutine(ServerManager.Instance.UpdatePlayerToServer(body, callback =>
            {
                if (callback != null)
                {
                    Debug.Log("Profile info updated.");
                    _finished = true;
                    gameObject.SetActive(false);
                    return;
                }
                else
                {
                    Debug.Log("Profile info update failed.");
                }
            }));
        }

    }
}
