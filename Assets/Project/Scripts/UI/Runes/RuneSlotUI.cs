using UnityEngine;

namespace BigProject.UI
{
    public class RuneSlotUI : MonoBehaviour
    {
        [SerializeField] private GameObject _runeImage;
        public void ShowRune()
        {
            _runeImage.SetActive(true);
        }

        public void HideRune()
        {
            _runeImage.SetActive(false);
        }
    }
}