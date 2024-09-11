using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookClickAnimation : MonoBehaviour
{
    private Animator animator;
    private bool hasBeenClicked = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMouseDown()
    {
        if (!hasBeenClicked && animator != null)
        {
            animator.Play("PageFlip");
            hasBeenClicked = true;
        }
    }
}
