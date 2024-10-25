using UnityEngine.UI;
using UnityEngine;

public class Chat : MonoBehaviour
{
    public InputField inputField;  // Поле ввода для отправки сообщений
    public GameObject content;  // Контейнер, в котором будут отображаться сообщени


    [Header("Prefab")]
    public GameObject messagePrefabBlue;
    public GameObject messagePrefabRed;
    public GameObject messagePrefabYellow;
    public GameObject messagePrefabOrange;
    public GameObject messagePrefabPink;

    private GameObject currentPrefab;

    


    public void SendChatMessage()
    {
        if (!string.IsNullOrEmpty(inputField.text))
        {
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
        Text buttonText = button.GetComponentInChildren<Text>();
        if(buttonText != null)
        {
            string textFromButton = buttonText.text;
            inputField.text = textFromButton;
        }
        else
        {
            Debug.Log("Error");
        }
    }


    public void DisplayMessage(string messageText)
    {
        if (currentPrefab != null)
        {
            // Создаём новый префаб
            GameObject newMessage = Instantiate(currentPrefab, content.transform);

            // Ищем компонент Text внутри дочерних объектов префаба
            Text messageUI = newMessage.GetComponentInChildren<Text>();
            if (messageUI != null)
            {
                messageUI.text = messageText;  // Устанавливаем текст сообщения
            }
            else
            {
                Debug.LogError("Компонент Text не найден в префабе!");
            }

            // Прокручиваем окно чата вниз, если используется ScrollView
            Canvas.ForceUpdateCanvases();
            ScrollRect scrollRect = content.GetComponentInParent<ScrollRect>();
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

 

    // Методы для установки нужного префаба и отправки сообщения
    public void SetBluePerfab()
    {
        currentPrefab = messagePrefabBlue;
        SendChatMessage();
    }

    public void SetRedPerfab()
    {
        currentPrefab = messagePrefabRed;
        SendChatMessage();
    }

    public void SetYellowPerfab()
    {
        currentPrefab = messagePrefabYellow;
        SendChatMessage();
    }

    public void SetOrangePerfab()
    {
        currentPrefab = messagePrefabOrange;
        SendChatMessage();
    }

    public void SetPinkPerfab()
    {
        currentPrefab = messagePrefabPink;
        SendChatMessage();
    }
}
