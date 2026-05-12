using BigProject.Systems;
using System;
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

        protected override SlidesGroup GetActualSlidesGroup(OutroVariant slidesVariant)
        {
            if (_slidesGroups.Count < Enum.GetValues(typeof(OutroVariant)).Length)
                Debug.LogAssertion(string.Format(LogStr.CRITICAL_REQUIRED_VALUES_MISSING, name, nameof(_slidesGroups)));


            return slidesVariant switch
            {
                OutroVariant.First => _slidesGroups[0],
                OutroVariant.Second => _slidesGroups[1],
                _ => throw new ArgumentException(slidesVariant.ToString()),
            };
        }
    }
}