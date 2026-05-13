using UnityEngine;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    [RequireComponent(typeof(CanvasGroup))]
    public class Slide : MonoBehaviour
    {
        private float INVISIBLE = 0f;

        [field: SerializeField] public float SlideTime { get; private set; } = 20f;

        public CanvasGroup Group { get; private set; }

        private void Start()
        {
            Group = GetComponent<CanvasGroup>();
            Group.alpha = INVISIBLE;
        }
    }
}