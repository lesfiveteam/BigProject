using BigProject.Systems;
using BigProject.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Project.Scripts.NPC.Animals.Cow
{
    public class NPCCow : MonoBehaviour
    {
        private readonly int StartTrigger = Animator.StringToHash("Start");
        private readonly int BoredTrigger = Animator.StringToHash("Bored");

        [SerializeField] private Animator _animator;

        [SerializeField] private float _minBoringTime = 20f;
        [SerializeField] private float _maxBoringTime = 40f;
        [SerializeField] private AudioSource _audioSource;

        private Coroutine _animationCoroutine;
        private float _timeToAlive = 4f;

        private bool _isAlive = false;

        private void Start()
        {
            Assert.IsNotNull(_animator, string.Format(LogStr.CRITICAL_NULL_REFERENCE, $"{name}", "Animator"));
            ExceptionUtilities.ThrowIfNullFormat(_audioSource);

            _animationCoroutine = StartCoroutine(StartAnimationsRoutine());
        }

        private IEnumerator StartAnimationsRoutine()
        {
            yield return new WaitForSeconds(Random.Range(0, _timeToAlive));

            _animator.SetTrigger(StartTrigger);
            _isAlive = true;

            float boredTime;

            while (_isAlive)
            {
                boredTime = Random.Range(_minBoringTime, _maxBoringTime);

                yield return new WaitForSeconds(boredTime);

                _audioSource.Play();
                _animator.SetTrigger(BoredTrigger);
            }
        }

        private void OnDestroy()
        {
            _isAlive = false;

            StopCoroutine(_animationCoroutine);
        }
    }
}