using BigProject.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class OutroSlideManager : SlideManager<OutroSlideManager.OutroVariant>
    {
        public enum OutroVariant
        {
            First,
            Second,
        }

        [SerializeField] private List<CanvasGroup> _slides2;

        private void Start()
        {
            ExceptionUtilities.ThrowIfEmptyCollection(_slides2, nameof(_slides2));
        }

        protected override List<CanvasGroup> GetActualSlides(OutroVariant slidesVariant)
        {
            switch (slidesVariant)
            {
                case OutroVariant.First:
                    return _slides1;

                case OutroVariant.Second:
                    return _slides2;

                default:
                    throw new ArgumentException(slidesVariant.ToString());
            }
        }
    }
}