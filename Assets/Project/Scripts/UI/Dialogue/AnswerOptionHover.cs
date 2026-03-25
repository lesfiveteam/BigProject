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

        private void OnEnable()
        {
            _leftArrow.GetComponent<Image>().enabled = false;
            _rightArrow.GetComponent<Image>().enabled = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _leftArrow.GetComponent<Image>().enabled = true;
            _rightArrow.GetComponent<Image>().enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _leftArrow.GetComponent<Image>().enabled = false;
            _rightArrow.GetComponent<Image>().enabled = false;
        }
    }

}

