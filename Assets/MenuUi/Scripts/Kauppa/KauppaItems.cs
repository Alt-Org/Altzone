using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Uusi esine", menuName = "Esine")]
public class KauppaItems : ScriptableObject
{
    public string hinta;
    public float value;

    public Sprite esine;
}
