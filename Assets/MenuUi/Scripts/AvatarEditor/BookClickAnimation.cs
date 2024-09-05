using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BookClickAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnMouseDown()
    {
        if (animator != null)
        {
            animator.Play("PageFlip");
        }
    }
}
