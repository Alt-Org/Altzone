using UnityEngine;

namespace Quantum
{
    public class SoulWallViewController : QuantumCallbacks
    {
        [SerializeField] private GameObject _soulWallEntityGameObject;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventSoulWallViewInit>(this, OnViewInit);
        }
        private void OnViewInit(EventSoulWallViewInit e)
        {
            //get parent object with entityview
            EntityRef entityRef = _soulWallEntityGameObject.GetComponent<QuantumEntityView>().EntityRef;
            if (entityRef != e.Entity) return;

            //scale gameobject
            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
