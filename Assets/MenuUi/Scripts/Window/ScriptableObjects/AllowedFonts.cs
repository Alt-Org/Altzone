using TMPro;
using UnityEngine;

namespace MenuUi.Scripts.Window.ScriptableObjects
{
    [CreateAssetMenu(menuName = "ALT-Zone/AllowedFonts", fileName = nameof(AllowedFonts))]
    public class AllowedFonts : ScriptableObject
    {
        [Header("TextMesh Pro")] public TMP_FontAsset[] _tmpFonts;
        [Header("Legacy")] public Font[] _fonts;
    }
}