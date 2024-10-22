using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RandomBG : MonoBehaviour
{

    [SerializeField] public Sprite[] Auras;
    [SerializeField] public Sprite[] Bubbles;
    [SerializeField] public Image Bubble;
    [SerializeField] public Image Aura;
    private int Rnd;
    // Start is called before the first frame update
    void Start()
    {
         Rnd = Random.Range(0, 5);
        //this.GetComponent<Image>().sprite = colors[rnd];
        Bubble.sprite = Bubbles[Rnd];
        Aura.sprite = Auras[Rnd];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
