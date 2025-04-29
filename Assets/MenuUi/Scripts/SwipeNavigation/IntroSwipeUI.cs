using System.Collections;
using System.Collections.Generic;
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
            if (CurrentPage == 1)
            {
                return;
            }
            else if (CurrentPage == 6)
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
                CurrentPage = 6;
            }
            else
            {
                base.NextSlide();
            }
        }
    }

}