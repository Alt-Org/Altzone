using UnityEngine;
using UnityEngine.UI;

namespace Altzone.Scripts.Apu
{
    [RequireComponent(typeof(Text))]
    public class VersionNumberHere : MonoBehaviour
    {
        private void Start()
        {
            var label = this.GetComponent<Text>();
            label.text = Application.version;
        }
    }
}