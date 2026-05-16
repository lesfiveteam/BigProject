using DG.Tweening;
using UnityEngine;

namespace BigProject.UI.TutorialSystem
{
    public class Tutorial : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private float _transparencyTime;

        private bool _isActive;

        public bool IsActive => _isActive; 

        public void Activate(bool isActive)
        {
            _isActive = isActive;

            if (_isActive)
            {
                gameObject.SetActive(true);
            }

            _canvasGroup.DOKill();
            _canvasGroup.blocksRaycasts = _isActive;
            float transparencyTime = _isActive ? _transparencyTime - _canvasGroup.alpha * _transparencyTime :
                _canvasGroup.alpha * _transparencyTime;
            _canvasGroup.DOFade(_isActive ? 1f : 0, transparencyTime).OnComplete(() => { if (!_isActive) { gameObject.SetActive(false); } });
        }
    }
}