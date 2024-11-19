using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NUnit.Framework;
using System.Collections.Generic;

public class Chat : MonoBehaviour
{
    [Header("Chat")]
    public TMP_InputField inputField;  // Поле ввода для отправки сообщений
    public GameObject languageChat;  // Контейнер, в котором будут отображаться сообщения
    public GameObject globalChat;
    public GameObject clanChat;
    private GameObject currentContent;

    [Header("Prefab")]
    public GameObject messagePrefabBlue;
    public GameObject messagePrefabRed;
    public GameObject messagePrefabYellow;
    public GameObject messagePrefabOrange;
    public GameObject messagePrefabPink;

    private GameObject currentPrefab;

    [Header("Commands")]
    [SerializeField]
    private string delete = "/deleteMessage";
    [SerializeField]
    private string deleteAllMessages = "/delete";

    private  Dictionary<GameObject, List<GameObject>> messagesByChat = new Dictionary<GameObject, List<GameObject>>();

    private void Start()
    {
        currentContent = languageChat;
        Debug.Log("Активен язык чат");

        messagesByChat[languageChat] = new List<GameObject>();
        messagesByChat[globalChat] = new List<GameObject>();
        messagesByChat[clanChat] = new List<GameObject>();
    }
    public void SendChatMessage()
    {
        if (currentContent == null)
        {
            Debug.LogWarning("Не выбран активный чат");
        }
        
        if (inputField != null && !string.IsNullOrEmpty(inputField.text) && inputField.text.Trim().Length >= 2)
        {
            if(inputField.text == delete)
            {
                DeleteMessage();
                inputField.text = "";
                return; // Выходим из метода, чтобы не отправлять команду в чат
            }

            if(inputField.text == deleteAllMessages)
            {
                DeleteAllMessages();
                inputField.text = "";
                return;
            }

            Debug.Log("Current Prefab: " + currentPrefab.name);

            DisplayMessage(inputField.text);

            inputField.text = "";
        }
        else
        {
            Debug.LogWarning("Нельзя отправить пустое сообщение.");
        }
    }

    public void SendQuickMessage(Button button)
    {
        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            string textFromButton = buttonText.text;
            inputField.text = textFromButton;
        }
        else
        {
            Debug.LogError("Ошибка: компонент TMP_Text не найден на кнопке.");
        }
    }

    public void DeleteMessage()
    {
       if(messagesByChat.Count > 0)
       {
            Debug.Log("удаление последнео сообщения");
            GameObject lastMessage = messagesByChat[currentContent][messagesByChat[currentContent].Count - 1];
            Destroy(lastMessage);
            messagesByChat[currentContent].RemoveAt(messagesByChat[currentContent].Count - 1);
       }
       else
        {
            Debug.LogWarning("нет сообщения для удаления");
        }
    }

    public void DeleteAllMessages()
    {
        Debug.Log("удаление всех сообщений");
        foreach (GameObject message in messagesByChat[currentContent]) //Проходит по каждому объекту message в списке messages
        {
            Destroy(message);
        }

        messagesByChat[currentContent].Clear(); // Очищаем список после удаления всех сообщений
    }

    public void DisplayMessage(string messageText)
    {
        if (currentPrefab != null)
        {
            // Создаём новый префаб
            GameObject newMessage = Instantiate(currentPrefab, currentContent.transform);

            // Ищем компонент TMP_Text внутри дочерних объектов префаба
            TMP_Text messageUI = newMessage.GetComponentInChildren<TMP_Text>();
            if (messageUI != null)
            {
                messageUI.text = messageText;  // Устанавливаем текст сообщения
            }
            else
            {
                Debug.LogError("Компонент TMP_Text не найден в префабе!");
            }

            messagesByChat[currentContent].Add(newMessage);

            // Прокручиваем окно чата вниз, если используется ScrollView
            Canvas.ForceUpdateCanvases();
            ScrollRect scrollRect = currentContent.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.verticalNormalizedPosition = 0;
            }
        }
        else
        {
            Debug.LogError("Префаб не назначен.");
        }
    }

    public void GlobalCahtActive()
    {
        currentContent = globalChat;
        Debug.Log("Активирован глобальный чат");
    }
    public void ClanCahtActive()
    {
        currentContent = clanChat;
        Debug.Log("Активирован клановый чат");
    }
    public void LanguageCahtActive()
    {
        currentContent = languageChat;
        Debug.Log("Активирован языковой чат");
    }

    // Методы для установки нужного префаба и отправки сообщения
    public void SetBluePrefab() { currentPrefab = messagePrefabBlue; SendChatMessage(); }
    public void SetRedPrefab() { currentPrefab = messagePrefabRed; SendChatMessage(); }
    public void SetYellowPrefab() { currentPrefab = messagePrefabYellow; SendChatMessage(); }
    public void SetOrangePrefab() { currentPrefab = messagePrefabOrange; SendChatMessage(); }
    public void SetPinkPrefab() { currentPrefab = messagePrefabPink; SendChatMessage(); }
}
