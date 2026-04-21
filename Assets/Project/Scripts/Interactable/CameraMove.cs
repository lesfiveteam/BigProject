using BigProject.Player;
using BigProject.Utilities;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Scripts.Interactable
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private float _moveDistance = 10f;
        [SerializeField] private float _duration = 1f;

        private float epsilon = 0.1f;

        private PlayerController _player;
        private Tween _currentTween;

        private float _downY;
        private float _upY;

        private void Start()
        {
            _downY = transform.position.y;
            _upY = _downY + _moveDistance;
        }

        public void Init(PlayerController player)
        {
            ExceptionUtilities.ThrowIfNullFormat(player);

            Subscribe(player);
        }

        private void MoveUp()
        {
            if (Mathf.Abs(transform.position.y - _upY) < epsilon)
                return;

            _currentTween?.Kill();
            _currentTween = transform.DOMoveY(_upY, _duration);
        }

        private void MoveDown()
        {
            if (Mathf.Abs(transform.position.y - _downY) < epsilon)
                return;

            _currentTween?.Kill();
            _currentTween = transform.DOMoveY(_downY, _duration);
        }

        public void Subscribe(PlayerController player)
        {
            _player = player;
            _player.OnUp += MoveUp;
            _player.OnDown += MoveDown;
        }

        private void OnDestroy()
        {
            _currentTween?.Kill();

            _player.OnUp -= MoveUp;
            _player.OnDown -= MoveDown;
            _player = null;
        }
    }
}