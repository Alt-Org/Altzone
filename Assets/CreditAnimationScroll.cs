using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Credits
{
    public class CreditAnimationScroll : MonoBehaviour
    {
        public GameObject canvas;
        bool _ScrollStop;
        Vector3 _creditPlacement;

        private void Start()
        {
            _creditPlacement = GameObject.Find("CreditContent").transform.position;
        }

        private void Update()
        {
            _ScrollStop = false;

            if (Application.isMobilePlatform || AppPlatform.IsSimulator)
            {
                if (Touchscreen.current.press.isPressed)
                {
                    _ScrollStop = true;
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
                    _ScrollStop = true;
                }
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _creditPlacement = GameObject.Find("CreditContent").transform.position;
                }
            }

            StopScrolling();

        }

        void StopScrolling()
        {

            if (_ScrollStop == true)
            {
                GetComponent<Animator>().enabled = false;
            }

            if (_ScrollStop == false)
            {
                GetComponent<Animator>().enabled = true;

                GameObject.Find("CreditContent").transform.position = _creditPlacement;
            }
        }
    }
}

