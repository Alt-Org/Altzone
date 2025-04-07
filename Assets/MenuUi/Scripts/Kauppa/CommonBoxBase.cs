using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CommonBoxBase : MonoBehaviour
{
    //static Color color = new RarityColor();
    [SerializeField] BoxCollider2D platformCollider;
    //[SerializeField] BoxLighning boxLighning;
    

    //public BoxLighning BoxLighning { get => boxLighning; set => boxLighning = value; }

    // Start is called before the first frame update
    void Start()
    {
       
        GameObject boxBaze = new GameObject("CommonBoxBaze");

        BoxCollider BoxCollider = boxBaze.AddComponent<BoxCollider>();

        BoxCollider.size = new Vector3(5, 1, 5);

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.SetParent(boxBaze.transform);
        cube.transform.localScale = new Vector3(5, 1, 5);
        cube.transform.position = new Vector3(0, 0, 0);

        boxBaze.transform.position = new Vector3(0, 0, 0);

        GameObject lightObject = new GameObject("CommonBoxLight");

        Light lightComponent = lightObject.AddComponent<Light>();
        lightComponent.type = LightType.Point;
        lightComponent.range = 10f;
        lightComponent.intensity = 1f;
        lightComponent.color = Color.white;

        lightObject.transform.position = new Vector3(0, 5, 0);
    }
}
