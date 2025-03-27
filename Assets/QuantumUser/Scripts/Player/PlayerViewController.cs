using UnityEngine;

namespace Quantum
{
    public class PlayerViewController : QuantumCallbacks
    {

        [SerializeField] private GameObject _playerEntityGameObject;

        private void Awake()
        {
            QuantumEvent.Subscribe<EventPlayerViewInit>(this, OnViewInit);
        }

        private void OnViewInit(EventPlayerViewInit e)
        {
            EntityRef entityRef = _playerEntityGameObject.GetComponent<QuantumEntityView>().EntityRef;
            if (entityRef != e.Entity) return;

            float scale = (float)e.ModelScale;
            transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
