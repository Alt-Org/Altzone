using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;


/// <summary>
/// Sends random messages to chat for testing purposes.
/// </summary>
public class ChatTester : MonoBehaviour
{

    [SerializeField]
    Chat _chat;
    [SerializeField]
    InputAction _Inputs;

    bool testing = false;
    int codePos = 0;


    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            if (testing)
            {
                if (Input.GetKeyDown(KeyCode.S))
                {

                }
            }
            codeCheck();
        }
        else if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            codePos = 0;
        }
    }

    void codeCheck()
    {
        if (!testing)
        {
            switch(codePos)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        codePos++;
                    }
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        codePos++;
                    }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.S))
                    {
                        codePos++;
                    }
                    break;
                case 3:
                    if (Input.GetKeyDown(KeyCode.T))
                    {
                        codePos++;
                        testing = true;
                        print("chat test is on");
                    }
                    break;
            }
        }
        else
        {
            switch (codePos)
            {
                case 0:
                    if (Input.GetKeyDown(KeyCode.O))
                    {
                        codePos++;
                    }
                    break;
                case 1:
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        codePos++;
                    }
                    break;
                case 2:
                    if (Input.GetKeyDown(KeyCode.F))
                    {
                        codePos++;
                        testing = false;
                        print("chat test is off");
                    }
                    break;
            }
        }
    }
}
