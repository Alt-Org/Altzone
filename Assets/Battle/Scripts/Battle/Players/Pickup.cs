using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    //[SerializeField] AudioClip collect;
    //[SerializeField] private AudioSource collectionSoundEffect;
    public int DiamondCount = 0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Diamond"))
        {
            DiamondCount = DiamondCount + 1;
            //collectionSoundEffect.PlayOneShot(collect);
            Destroy(collision.gameObject);
        }
    }
}
