using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Battle0.Scripts.Ui
{
    public class StartAnimationHandler : MonoBehaviour
    {
        private Animator[] _animators;

        private void Awake()
        {
            _animators = gameObject.GetComponentsInChildren<Animator>();
            for (int i = 0; i < _animators.Length; i++)
            {
                _animators[i].enabled = false;
            }
        }

        private void OnEnable()
        {
            this.Subscribe<UiEvents.StartAnimation>(OnStartBattle);
        }

        void OnStartBattle(UiEvents.StartAnimation data)
        {
            for (int i = 0; i < _animators.Length; i++)
            {
                _animators[i].enabled = true;
            }
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }
    }
}
