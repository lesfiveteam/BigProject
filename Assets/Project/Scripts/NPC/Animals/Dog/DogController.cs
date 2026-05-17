using Assets.Project.Scripts.NPC.Animals;
using BigProject.Managers;
using BigProject.Systems;
using BigProject.Systems.Sound;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class DogController : MonoBehaviour, IScared, IUnscared
{
    private enum DogAction
    {
        Bark = 1,
        Growl,
    }

    [Header("Components")]
    [SerializeField] private Animator _animator;

    [Header("Audio Clips")]
    [SerializeField] private EnvironmentSound _barkSound;
    [SerializeField] private EnvironmentSound _growlSound;
    [SerializeField] private EnvironmentSound _pantSound;

    [Header("Timing")]
    [SerializeField] private float _minDelay = 5f;
    [SerializeField] private float _maxDelay = 15f;

    private Coroutine _randomActionRoutine;
    private bool _isPanting = false;

    private void Start()
    {
        StartRandomActionRoutine();
    }

    public void Scare(Transform danger) => StartPanting();
    public void Unscare() => StopPanting();

    private void StartRandomActionRoutine()
    {
        if (_randomActionRoutine != null)
            StopCoroutine(_randomActionRoutine);

        _randomActionRoutine = StartCoroutine(RandomActionRoutine());
    }

    private IEnumerator RandomActionRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minDelay, _maxDelay));

            if (_isPanting) continue;

            int action = Random.Range(0, Enum.GetNames(typeof(DogAction)).Length);

            _animator.SetBool("isStanding", true);
            _animator.SetTrigger("doAction");

            switch (action)
            {
                case 0:
                    _animator.SetInteger("actionType", (int)DogAction.Bark);
                    break;

                case 1:
                    _animator.SetInteger("actionType", (int)DogAction.Growl);
                    break;

                default:
                    GameLogManager.Error(string.Format(LogStr.CRITICAL_WRONG_ARGUMENT, "DogAction", action));
                    break;
            }
        }
    }

    public void OnBarkAudioEvent()
    {
        _barkSound.PlaySound();
    }

    public void OnGrowlAudioEvent()
    {
        _growlSound.PlaySound();
    }

    public void StartPanting()
    {
        if (_isPanting) return;

        _isPanting = true;

        _animator.SetBool("isPanting", true);
        _pantSound.PlaySound();
    }

    public void StopPanting()
    {
        if (!_isPanting) return;

        _isPanting = false;
        _animator.SetBool("isPanting", false);
        _animator.SetBool("isStanding", false);

        StartRandomActionRoutine();
    }

    private void OnDisable()
    {
        if (_randomActionRoutine != null)
            StopCoroutine(_randomActionRoutine);
    }
}