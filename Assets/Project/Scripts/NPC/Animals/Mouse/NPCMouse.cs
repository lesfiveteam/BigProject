using BigProject.Systems.Sound;
using BigProject.Utilities;
using DG.Tweening;
using UnityEngine;

namespace Assets.Project.Scripts.NPC.Animals.Mouse
{
    public class NPCMouse : MonoBehaviour, IScared
    {
        private const float ESCAPE_DURATION = 1f;
        private readonly int _runTrigger = Animator.StringToHash("Run");

        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _escapePoint;
        [SerializeField] private EnvironmentSound _environmentSound;

        private bool _isScared = false;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_escapePoint);
            ExceptionUtilities.ThrowIfNullFormat(_environmentSound);
        }

        public void Scare(Transform danger)
        {
            if (_isScared)
                return;

            _isScared = true;

            _environmentSound.PlaySound();
            _animator.SetTrigger(_runTrigger);

            Vector3 escapeDirection = (_escapePoint.position - transform.position).normalized;

            if (escapeDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(escapeDirection);

            transform
                .DOMove(_escapePoint.position, ESCAPE_DURATION)
                .SetEase(Ease.Linear)
                .OnComplete(() => Destroy(gameObject));
        }
    }
}