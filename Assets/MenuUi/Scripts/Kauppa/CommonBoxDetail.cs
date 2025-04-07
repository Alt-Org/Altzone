using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonBoxDetail : MonoBehaviour
{
    
    public Canvas canvas;
    public Sprite productImage;
    public string productName = "T_ItemName";
    public string price = "Price";

    public Text titleText;
    public Image itemImage;
    public Image priceImage;
    public string T_ItemName;
    public Sprite Content;
    public float T_Price;
    

    void Start() {
        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        }
        UpdateUI();
        CreateCommonBoxDetail();
    }

    void CreateCommonBoxDetail()
    {
        GameObject panel = new GameObject("CommonBoxDetail");
        panel.transform.SetParent(canvas.transform);

        Image panelImage = panel.AddComponent<Image>();

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(400, 400);
        rectTransform.anchoredPosition = new Vector2(0, 0);

        GameObject productNameObject = new GameObject("T_ItemName");
        productNameObject.transform.SetParent(panel.transform);

        Text productNameText = productNameObject.AddComponent<Text>();
        productNameText.text = T_ItemName;
        productNameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        productNameText.fontSize = 20;
        productNameText.alignment = TextAnchor.MiddleCenter;

        RectTransform nameRectTransform = productNameText.GetComponent<RectTransform>();
        nameRectTransform.sizeDelta = new Vector2(380, 50);
        nameRectTransform.anchoredPosition = new Vector2(0, 150);

        GameObject productImageObject = new GameObject("ProductImage");
        productImageObject.transform.SetParent(panel.transform);

        Image productImageComp = productImageObject.AddComponent<Image>();
        productImageObject.transform.SetParent(panel.transform);

        RectTransform imageRectTransform = productImageObject.GetComponent<RectTransform>();
        imageRectTransform.sizeDelta = new Vector2(150, 150);
        imageRectTransform.anchoredPosition = new Vector2(0, 0);

        GameObject productPriceObject = new GameObject("ProductPrice");
        productPriceObject.transform.SetParent(panel.transform);

        Text productPriceText = productPriceObject.AddComponent<Text>();
        productPriceText.text = price;
        productPriceText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        productPriceText.fontSize = 20;
        productPriceText.alignment = TextAnchor.MiddleCenter;

        RectTransform priceRectTransform = productPriceText.GetComponent<RectTransform>();
        priceRectTransform.sizeDelta = new Vector2(380, 50);
        priceRectTransform.anchoredPosition = new Vector2(0, -150);
    }
       
    void UpdateUI()
    {
        
        titleText.text = T_ItemName;
        itemImage.sprite = Content;
        string myString = T_Price.ToString();
        priceImage.GetComponentInChildren<Text>().text = myString;
    }

}
