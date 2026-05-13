using BigProject.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class SlidesGroup : MonoBehaviour
    {
        [field: SerializeField] public AudioClip Music { get; private set; }
        [field: SerializeField] public List<Slide> Slides { get; private set; }

        private void Start()
        {
            ExceptionUtilities.ThrowIfNull(Music);
            ExceptionUtilities.ThrowIfEmptyCollection(Slides, $"{name}.{nameof(Slides)}");
        }
    }
}