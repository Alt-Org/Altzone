using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class IntroSwipeUI : SwipeUI
    {
        [SerializeField] private GameObject[] startSlides;
        [SerializeField] private GameObject[] rotatingSlides;

        protected override void Awake()
        {
            GameObject[] slides;
            slides = startSlides.Concat(rotatingSlides).ToArray();
            base.slides = slides;
            base.Awake();
        }

        void Start()
        {
            
        }

        public override void PreviousSlide()
        {
            if (CurrentPage < startSlides.Length)
            {
                return;
            }
            else if (CurrentPage == startSlides.Length)
            {
                if (_willRotate)
                {
                    CurrentPage = maxPage; // Goes to the last slide when swiping left on the first normal slide
                }
            }
            else
            {
                base.PreviousSlide();
            }
        }
        public override void NextSlide()
        {
            if (CurrentPage >= maxPage)
            {
                CurrentPage = startSlides.Length;
            }
            else
            {
                base.NextSlide();
            }
        }
    }

}
