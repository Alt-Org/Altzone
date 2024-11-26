using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ALT-Zone/AvatarVisualData")]
public class AvatarVisualDataScriptableObject : ScriptableObject
{
     
    public List<Sprite> sprites= new();
    public List<Color> colors= new();

}
