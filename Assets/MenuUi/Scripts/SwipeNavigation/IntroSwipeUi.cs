using System.Collections.Generic;
using MenuUi.Scripts.SwipeNavigation;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class IntroSwipeUi : SwipeUI
    {
        void Start()
        {
            //UpdateInput();
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
            else if (CurrentPage == 0)
            {
                CurrentPage = 6;
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
                CurrentPage = 12;
            }

            else
            {
                base.NextSlide();
            }
        }
    }

}