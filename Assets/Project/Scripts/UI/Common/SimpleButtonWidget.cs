using BigProject.Systems.HUD;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BigProject.UI.Common
{
    public class SimpleButtonWidget : MonoBehaviour, IHUDWidget, IPointerUpHandler
    {
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}