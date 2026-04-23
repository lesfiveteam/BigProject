using BigProject.Utilities;
using System.Collections;
using UnityEngine;

namespace Assets.Project.Scripts.NPC
{
    public class NPCWorker : MonoBehaviour
    {
        private readonly int StartTrigger = Animator.StringToHash("Start");

        private float _timeToAlive = 2f;

        [SerializeField] private Animator _animator;

        private Coroutine _animationCoroutine;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);

            _animationCoroutine = StartCoroutine(StartAnimations());
        }

        private IEnumerator StartAnimations()
        {
            yield return new WaitForSeconds(Random.Range(0, _timeToAlive));

            _animator.SetTrigger(StartTrigger);
        }

        private void OnDestroy()
        {
            StopCoroutine(_animationCoroutine);
        }
    }
}
