using UnityEngine;
using System.Collections.Generic;

namespace BigProject.Intercatable.HighlightedObjects
{
    //Is required on any highlighted object. Has a list of effects that are applied on mouse over or click
    
    public class HighlightedObject : MonoBehaviour
    {
        [SerializeField] private List<MonoBehaviour> _highlightEffects;

        public void Highlight()
        {
            foreach (MonoBehaviour effect in _highlightEffects)
            {
                if (effect is IHighlightEffect highlightEffect)
                {
                    highlightEffect.EnableEffect();
                }
            }
        }

        public void Unhighlight()
        {
            foreach (MonoBehaviour effect in _highlightEffects)
            {
                if (effect is IHighlightEffect highlightEffect)
                {
                    highlightEffect.DisableEffect();
                }
            }
        }

        public void PressHighlight()
        {
            foreach (MonoBehaviour effect in _highlightEffects)
            {
                if (effect is IPressableEffect pressableEffect)
                {
                    pressableEffect.SetPressableEffect();
                }
            }
        }
    }
}