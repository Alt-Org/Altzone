using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaikkiOnHyvinShelfScript : MonoBehaviour
{
    private float transitionDuration = 9.0F;
    private float newPositionY;
    private float newPositionX;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(shakeAnimation());
    }

    private IEnumerator shakeAnimation()
    {
        // Since furniture can be anywhere in soulhome, get its position values
        Vector3 startPosition = transform.position;

        float elapsedTime = 0f;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = startPosition + ((Random.insideUnitSphere / 20) * 3); // Gives a random vector

            yield return null;
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
