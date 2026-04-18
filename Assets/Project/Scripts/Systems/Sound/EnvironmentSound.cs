using BigProject.Managers.SoundsMusicManagers;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;

namespace BigProject.Systems.Sound
{
    //Should be on an environment object that plays sounds
    public class EnvironmentSound : MonoBehaviour
    {
        [SerializeField] private bool _shouldPlayLooped;
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private float _soundInterval = DEFAULT_SOUND_INTERVAL;
        [Range(0, 1)][SerializeField] private float _playSoundChance;
        [SerializeField] private float _volume = 1f;

        private Coroutine _playSoundRoutine;
        private SoundsManager _soundsManager;
        private WaitForSeconds _playSoundWait;
        private bool _shouldPlayInterval;

        private const float DEFAULT_SOUND_INTERVAL = -1f;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Sounds manager"));

            if (_soundInterval != DEFAULT_SOUND_INTERVAL)
            {
                _shouldPlayInterval = true;
                _playSoundWait = new(_soundInterval);
            }

            if (_shouldPlayLooped)
            {
                PlaySound();
            }
        }

        private void OnEnable()
        {
            if (_shouldPlayInterval)
            {
                _playSoundRoutine = StartCoroutine(PlaySoundRoutine());
            }
        }

        private void OnDisable()
        {
            if (_shouldPlayInterval)
            {
                StopCoroutine(_playSoundRoutine);
            }
        }

        /// <summary>
        /// May be used in triggers
        /// </summary>
        public void PlaySound()
        {
            _soundsManager.PlaySound(_audioClip, spawnPosition: transform, isLooped: _shouldPlayLooped);
        }

        /// <summary>
        /// Randomly plays a sound every once in a while
        /// </summary>
        private IEnumerator PlaySoundRoutine()
        {
            while (true)
            {
                yield return _playSoundWait;

                if(UnityEngine.Random.Range(0, 1) < _playSoundChance)
                {
                    _soundsManager.PlaySound(_audioClip, spawnPosition: transform);
                }
            }
        }
    }
}