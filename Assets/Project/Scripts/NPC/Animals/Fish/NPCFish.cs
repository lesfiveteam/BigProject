using DG.Tweening;
using System.Linq;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Fish
{
    public class NPCFish : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _rotateSpeed = 600f;

        private NPCFishPool _pool;
        private Sequence _root;

        private Vector3[] _positions;
        private float[] _distances;

        public void Init(NPCFishPool pool, Vector3[] positions, float[] distances)
        {
            _pool = pool;
            _positions = positions;
            _distances = distances;
        }

        public void SwimToSea()
        {
            _root = DOTween.Sequence();

            for (int i = 0; i < _positions.Length; i++)
            {
                Vector3 targetPoint = _positions[i];
                Vector3 startPoint = i == 0 ?
                    transform.position :
                    _positions[i - 1];
                Vector3 moveDirection = (targetPoint - startPoint).normalized;

                float moveDuration = i == 0 ?
                    Vector3.Distance(transform.position, targetPoint) / _moveSpeed :
                    _distances[i - 1] / _moveSpeed;

                _root.Append(transform
                    .DOMove(targetPoint, moveDuration)
                    .SetEase(Ease.Linear));

                if (moveDirection != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                    float angle = Quaternion.Angle(transform.rotation, targetRotation);
                    float rotateDuration = angle / _rotateSpeed;

                    _root.Join(transform
                        .DORotateQuaternion(targetRotation, rotateDuration));
                }
            }

            _root.OnComplete(() => _pool.Return(this));
            _root.Play();
        }

        private void OnDisable()
        {
            if (_root != null && _root.IsActive())
                _root.Kill();
        }
    }
}