using BigProject.Utilities;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Mouse
{
    public class NPCMouse : MonoBehaviour, IScared
    {
        private readonly int RunTrigger = Animator.StringToHash("Run");

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _escapePoint;

        public float _duration = 1f;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_escapePoint);
        }

        public void Scare(Transform danger)
        {
            _animator.SetTrigger(RunTrigger);

            Vector3 direction = (_escapePoint.position - transform.position).normalized;

            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = targetRotation;
            }

            transform.DOMove(_escapePoint.position, _duration)
                .SetEase(Ease.Linear)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}