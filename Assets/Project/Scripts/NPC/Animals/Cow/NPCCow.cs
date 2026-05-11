using BigProject.Systems;
using BigProject.Systems.Sound;
using BigProject.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

namespace Assets.Project.Scripts.NPC.Animals.Cow
{
    public class NPCCow : MonoBehaviour
    {
        private const float TIME_TO_ALIVE = 4f;
        private const float MIN_BORING_TIME = 20f;
        private const float MAX_BORING_TIME = 40f;

        private readonly int _startTrigger = Animator.StringToHash("Start");
        private readonly int _boredTrigger = Animator.StringToHash("Bored");

        [SerializeField] private Animator _animator;
        [SerializeField] private EnvironmentSound _environmentSound;

        private Coroutine _animationCoroutine;
        
        private bool _isAlive = false;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_animator);
            ExceptionUtilities.ThrowIfNullFormat(_environmentSound);

            _animationCoroutine = StartCoroutine(StartAnimationsRoutine());
        }

        private IEnumerator StartAnimationsRoutine()
        {
            yield return new WaitForSeconds(Random.Range(0, TIME_TO_ALIVE));

            _animator.SetTrigger(_startTrigger);
            _isAlive = true;

            float boredTime;

            while (_isAlive)
            {
                boredTime = Random.Range(MIN_BORING_TIME, MAX_BORING_TIME);

                yield return new WaitForSeconds(boredTime);

                _environmentSound.PlaySound();
                _animator.SetTrigger(_boredTrigger);
            }
        }

        private void OnDisable()
        {
            _isAlive = false;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }
        }
    }
}