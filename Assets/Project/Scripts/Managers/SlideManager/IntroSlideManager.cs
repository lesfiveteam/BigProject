using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class IntroSlideManager : SlideManager<IntroSlideManager.IntroVariant>
    {
        public enum IntroVariant
        {
            First,
        }

        protected override List<CanvasGroup> GetActualSlides(IntroVariant slidesVariant)
        {
            switch (slidesVariant)
            {
                case IntroVariant.First:
                    return _slides1;

                default:
                    throw new ArgumentException(slidesVariant.ToString());
            }
        }
    }
}
