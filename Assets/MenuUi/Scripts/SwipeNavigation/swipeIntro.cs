using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class IntroSwipeUI : SwipeUI
    {
        void Start()
        {
            
        }

        public override void PreviousSlide()
        {
            if (CurrentPage == 7)
            {
                return;
            }
            else if (CurrentPage == 12)
            {
                CurrentPage  = maxPage;
            }
            else
            {
                base.PreviousSlide();
            }
        }
        public override void NextSlide()
        {
            if (CurrentPage == maxPage)
            {
                CurrentPage = 7;
            }
            else
            {
                base.NextSlide();
            }
        }
    }

}