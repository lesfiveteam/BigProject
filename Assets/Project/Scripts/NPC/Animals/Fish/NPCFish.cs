using Assets.Project.Scripts.NPC.NPCWalkSystem;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Fish
{
    public class NPCFish : MonoBehaviour
    {
        [SerializeField] private float _moveSpeed = 3f;
        [SerializeField] private float _rotateSpeed = 600f;

        private Queue<NPCRootPoint> _currentRoute;
        private Sequence _currentSequence;

        public void Init(Queue<NPCRootPoint> currentRoute)
        {
            _currentSequence = DOTween.Sequence();
            _currentRoute = currentRoute;

            SwimToSea();
        }

        private void SwimToSea()
        {
            List<NPCRootPoint> points = _currentRoute.ToList();

            for (int i = 0; i < points.Count; i++)
            {
                Vector3 targetPos = points[i].Position;
                Vector3 startPos = i == 0 ? transform.position : points[i - 1].Position;
                Vector3 direction = (targetPos - startPos).normalized;

                float moveDuration = Vector3.Distance(startPos, targetPos) / _moveSpeed;
                _currentSequence.Append(transform.DOMove(targetPos, moveDuration).SetEase(Ease.Linear));

                if (direction != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    float angle = Quaternion.Angle(transform.rotation, targetRotation);
                    float rotateDuration = angle / _rotateSpeed;

                    _currentSequence.Join(transform.DORotateQuaternion(targetRotation, rotateDuration));
                }
            }

            _currentSequence.OnComplete(() => Destroy(gameObject));
            _currentSequence.Play();
        }

        private void OnDestroy()
        {
            if (_currentSequence != null && _currentSequence.IsActive())
                _currentSequence.Kill();
        }
    }
}