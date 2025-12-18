using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SosiaalinenAhdistusFX : MonoBehaviour

{
    public Sprite sofaSprite; // image
    // public float fadeSpeed = 0.1f; // fading speed

    [Range(0.0f, 1.0f)]
    public float alphaChange; // alpha change


    // Start is called before the first frame update
    void Start()
    {
        sofaSprite = GetComponent<SpriteRenderer> ().sprite; // points to the sprite of the renderer
        
    }

    // Update is called once per frame
    void Update()
    {

        // SetAlpha(fadeSpeed ); // setting the alpha according to the speed
    
    }

    void SetAlpha(float alpha) // 
    {

    

    }
  


}
