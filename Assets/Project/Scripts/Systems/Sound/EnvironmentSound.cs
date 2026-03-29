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
        [SerializeField] private AudioClip _audioClip;
        [SerializeField] private float _soundInterval;
        [Range(0, 1)][SerializeField] private float _playSoundChance;

        private Coroutine _playSoundRoutine;
        private SoundsManager _soundsManager;
        private WaitForSeconds _playSoundWait;

        public void Init(SoundsManager soundsManager)
        {
            _soundsManager = soundsManager;
            _playSoundWait = new(_soundInterval);
            ExceptionUtilities.ThrowIfNull(_soundsManager, String.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Sounds manager"));
        }

        private void OnEnable()
        {
            _playSoundRoutine = StartCoroutine(PlaySoundRoutine());
        }

        private void OnDisable()
        {
            StopCoroutine(_playSoundRoutine);
        }

        private IEnumerator PlaySoundRoutine()
        {
            while (true)
            {
                yield return _playSoundWait;

                if(UnityEngine.Random.Range(0, 1) < _playSoundChance)
                {
                    _soundsManager.PlaySound(_audioClip);
                }
            }
        }
    }
}