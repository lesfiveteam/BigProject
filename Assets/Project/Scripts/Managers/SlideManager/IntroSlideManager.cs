using BigProject.Systems;
using System;
using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class IntroSlideManager : SlideManager<IntroSlideManager.IntroVariant>
    {
        public enum IntroVariant
        {
            First,
        }

        protected override SlidesGroup GetActualSlidesGroup(IntroVariant slidesVariant)
        {
            if (_slidesGroups.Count < Enum.GetValues(typeof(IntroVariant)).Length)
                Debug.LogAssertion(string.Format(LogStr.CRITICAL_REQUIRED_VALUES_MISSING, name, nameof(_slidesGroups)));

            return slidesVariant switch
            {
                IntroVariant.First => _slidesGroups[0],
                _ => throw new ArgumentException(slidesVariant.ToString()),
            };
        }
    }
}
