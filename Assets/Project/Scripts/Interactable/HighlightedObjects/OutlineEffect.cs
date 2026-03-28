using UnityEngine;

namespace BigProject.Intercatable.HighlightedObjects
{
    public class OutlineEffect : MonoBehaviour, IHighlightEffect
    {
        [SerializeField] private Outline _outlineComponent;

        private void Awake()
        {
            DisableEffect();
        }

        public virtual void EnableEffect()
        {
            _outlineComponent.enabled = true;
        }

        public void DisableEffect()
        {
            _outlineComponent.enabled = false;
        }
    }
}