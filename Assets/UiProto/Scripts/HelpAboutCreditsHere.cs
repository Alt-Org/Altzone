using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Apu
{
    [RequireComponent(typeof(Text))]
    public class HelpAboutCreditsHere : MonoBehaviour
    {
        public TextAsset textAsset;

        private void Start()
        {
            var label = GetComponent<Text>();
            label.text = textAsset.text;
        }
    }
}