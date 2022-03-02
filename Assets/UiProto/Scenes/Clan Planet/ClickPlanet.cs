using Prg.Scripts.Common.Unity.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Scenes.Clan_Planet
{
    public class ClickPlanet : MonoBehaviour
    {
        public UnityEventComponent onMouseDown;
        public UnityEvent onMouseDrag;

        private void OnMouseDown()
        {
            onMouseDown.Invoke(this);
        }

        private void OnMouseDrag()
        {
            onMouseDrag.Invoke();
        }
    }
}
