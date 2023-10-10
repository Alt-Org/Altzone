// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Exit Games GmbH"/>
// <summary>Demo code for Photon Chat in Unity.</summary>
// <author>developer@exitgames.com</author>
// --------------------------------------------------------------------------------------------------------------------


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;


namespace Photon.Chat.Demo
{
    [RequireComponent(typeof(ChatGui))]
    public class NamePickGui : MonoBehaviour
    {
        private const string UserNamePlayerPref = "NamePickUserName";

        public ChatGui chatNewComponent;

        public InputField idInput;

        public void Start()
        {
            this.chatNewComponent = FindObjectOfType<ChatGui>();


            string prefsName = PlayerPrefs.GetString(UserNamePlayerPref);
            if (!string.IsNullOrEmpty(prefsName))
            {
                this.idInput.text = prefsName;
            }
        }


        // new UI will fire "EndEdit" event also when loosing focus. So check "enter" key and only then StartChat.
        public void EndEditOnEnter()
        {
            if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame || Touchscreen.current.press.wasPressedThisFrame)
            {
                this.StartChat();
            }
        }

        public void StartChat()
        {
            ChatGui chatNewComponent = FindObjectOfType<ChatGui>();
            chatNewComponent.UserName = this.idInput.text.Trim();
            chatNewComponent.Connect();
            this.enabled = false;

            PlayerPrefs.SetString(UserNamePlayerPref, chatNewComponent.UserName);
        }

    }
}