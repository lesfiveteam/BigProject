using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BigProject.UI.Dialogue
{
    public class AnswerOptionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField]
        private GameObject _leftArrow;
        [SerializeField]
        private GameObject _rightArrow;

        private Image _leftArrowImage = null;
        private Image _rightArrowImage = null;

        private void OnEnable()
        {
            _leftArrowImage = _leftArrow.GetComponent<Image>();
            _rightArrowImage = _rightArrow.GetComponent<Image>();
            _leftArrowImage.enabled = false;
            _rightArrowImage.enabled = false;

            Canvas.ForceUpdateCanvases();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _leftArrowImage.enabled = true;
            _rightArrowImage.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _leftArrowImage.enabled = false;
            _rightArrowImage.enabled = false;
        }
    }

}

