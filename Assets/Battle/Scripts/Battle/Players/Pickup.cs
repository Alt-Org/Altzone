using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Pickup : MonoBehaviour
{
    //[SerializeField] AudioClip collect;
    //[SerializeField] private AudioSource collectionSoundEffect;
    public int DiamondCount = 0;
    public GameObject TeamDiamonds;
    //public TMP_Text TeamDiamonds;

    private TMP_Text DiamondText;

    private void Start()
    {
        TeamDiamonds = GameObject.FindGameObjectWithTag("AlphaDiamonds");
        DiamondText = TeamDiamonds.GetComponent<TMP_Text>();     ///TeamDiamonds.TMP_Text;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Diamond"))
        {
            DiamondCount = DiamondCount + 1;
            //TeamDiamonds = DiamondCount;
            DiamondText.SetText(DiamondCount.ToString());
            //collectionSoundEffect.PlayOneShot(collect);
            Destroy(collision.gameObject);
        }
    }
}
