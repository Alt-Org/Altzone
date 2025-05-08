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
            if (CurrentPage == 8)
            {
                return;
            }
            else if (CurrentPage == 13)
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
                CurrentPage = 8;
            }
            else
            {
                base.NextSlide();
            }
        }
    }

}
