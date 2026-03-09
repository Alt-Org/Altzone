using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KaikkiOnHyvinPopupScript : MonoBehaviour
{
    public GameObject popup;
    public TextMeshProUGUI popupText;
    public Button button;
    private float transitionDuration = 0.4F;
    public static KaikkiOnHyvinPopupScript Instance; // static variable

    private string[] messages = 
    {
        "1a","2a",
        "3a","4a",
        "5a","6a"
    };
    string message = "";

    public void popupController() 
    { 
        int randomNumber = Random.Range(0, messages.Length);
        message = messages[randomNumber];
        popupText.text = $"'{message}'";
        popup.SetActive(true);
        StartCoroutine(popupAnimation());

        button.onClick.AddListener(() =>{
            hidePopup();
        });
    }

    void Start()
    {
        popup.SetActive(false);
    }

    private IEnumerator popupAnimation()
    {    
        float elapsedTime = 0f;
        float scale = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;

            scale = Mathf.Lerp(0f, 1f, elapsedTime / transitionDuration);
            this.transform.localScale = new Vector3(scale, scale, 1);

            yield return null;
        }
    }

    public void hidePopup()
    {
        popup.SetActive(false);
    }
    void Awake()
    {
        Instance = this; // sets Instance to this script: can be found without a reference set in inspector
    }

}
