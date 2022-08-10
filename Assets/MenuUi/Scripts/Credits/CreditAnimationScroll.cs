using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Credits
{
    public class CreditAnimationScroll : MonoBehaviour
    {
        private bool _scrollStop;
        private Vector3 _creditPlacement;

        private void Start()
        {
            _creditPlacement = GameObject.Find("CreditContent").transform.position;
        }

        private void Update()
        {
            _scrollStop = false;

            if (Application.isMobilePlatform || AppPlatform.IsSimulator)
            {
                if (Touchscreen.current.press.isPressed)
                {
                    _scrollStop = true;
                }
                if (Touchscreen.current.press.wasPressedThisFrame)
                {
                    _creditPlacement = GameObject.Find("CreditContent").transform.position;
                }
            }
            else
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    _scrollStop = true;
                }
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _creditPlacement = GameObject.Find("CreditContent").transform.position;
                }
            }

            StopScrolling();

        }

        private void StopScrolling()
        {

            if (_scrollStop == true)
            {
                GetComponent<Animator>().enabled = false;
            }

            if (_scrollStop == false)
            {
                GetComponent<Animator>().enabled = true;

                GameObject.Find("CreditContent").transform.position = _creditPlacement;
            }
        }
    }
}

