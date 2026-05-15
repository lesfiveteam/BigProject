using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace BigProject.Managers.SoundsMusicManagers
{
    public enum MixerType
    {
        Master,
        SFX,
        UI,
        Voice,
    }

    public class SoundsManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource3D;
        [SerializeField] private AudioSource _audioSource2D;
        [SerializeField] private List<MixerMapping> _mixerMappings = new List<MixerMapping>();

        private List<AudioSource> _activeAudioSources = new List<AudioSource>();
        private Dictionary<Transform, AudioSource> _objectAudioMap = new Dictionary<Transform, AudioSource>();
        private Dictionary<MixerType, AudioMixerGroup> _mixerDictionary = new Dictionary<MixerType, AudioMixerGroup>();

        private void Awake()
        {
            InitializeMixerDictionary();
        }

        private void InitializeMixerDictionary()
        {
            _mixerDictionary.Clear();

            foreach (MixerMapping mapping in _mixerMappings)
            {
                if (mapping.MixerGroup != null)
                {
                    _mixerDictionary[mapping.Type] = mapping.MixerGroup;
                }
            }
        }

        public AudioMixerGroup GetMixer()
        { 
            return _mixerMappings[0].MixerGroup;
        }

        /// <summary>
        /// Spawns a sound object.
        /// <param name = "owner"> If there is an owner, the next sounds spawned with this owner will stop the previous one </param>
        /// <param name = "lowestPitch"> Lowest possible random pitch </param>
        /// <param name = "lowestPitch"> Lowest possible random pitch </param>
        /// <param name = "highestPitch"> Highest possible random pitch </param>
        /// </summary>
        public void PlaySound(
            AudioClip clip, 
            MixerType mixerType = MixerType.Master, 
            float lowestPitch = 1f, 
            float highestPitch = 1f, 
            Transform spawnPosition = null, 
            float volume = 1f, 
            Transform owner = null, 
            bool is2D = false, 
            bool isLooped = false)
        {
            if (owner != null && _objectAudioMap.ContainsKey(owner))
            {
                StopSound(_objectAudioMap[owner]);
            }

            Vector3 spawnPos = spawnPosition != null ? spawnPosition.position : transform.position;
            Transform parent = spawnPosition != null ? spawnPosition : transform;
            AudioSource audioSource = Instantiate(is2D ? _audioSource2D : _audioSource3D, spawnPos, Quaternion.identity, parent);
            audioSource.clip = clip;
            audioSource.volume = volume;
            audioSource.loop = isLooped;

            if (_mixerDictionary.TryGetValue(mixerType, out AudioMixerGroup mixerGroup) && mixerGroup != null)
            {
                audioSource.outputAudioMixerGroup = mixerGroup;
            }

            if (lowestPitch != highestPitch)
            {
                float randomPitch = Random.Range(lowestPitch, highestPitch);
                audioSource.pitch = randomPitch;
            }

            audioSource.Play();

            _activeAudioSources.Add(audioSource);

            if (owner != null)
            {
                _objectAudioMap[owner] = audioSource;
            }

            if (!isLooped)
            {
                StartCoroutine(DestroyAfterPlayback(audioSource, owner));
            }
        }

        /// <summary>
        /// Stops playing sound if it's already playing
        /// </summary>
        public void StopSound(AudioSource audioSourceToStop)
        {
            if (audioSourceToStop != null && _activeAudioSources.Contains(audioSourceToStop))
            {
                audioSourceToStop.Stop();
                _activeAudioSources.Remove(audioSourceToStop);

                foreach (KeyValuePair<Transform, AudioSource> audio in _objectAudioMap)
                {
                    if (audio.Value == audioSourceToStop)
                    {
                        _objectAudioMap.Remove(audio.Key);
                        break;
                    }
                }

                Destroy(audioSourceToStop.gameObject);
            }
        }

        /// <summary>
        /// Destroys spawned sound object
        /// </summary>
        private IEnumerator DestroyAfterPlayback(AudioSource audioSource, Transform owner)
        {
            yield return new WaitForSeconds(audioSource.clip.length);

            if (audioSource == null || !_activeAudioSources.Contains(audioSource))
            {
                yield break;
            }

            _activeAudioSources.Remove(audioSource);

            if (owner != null && _objectAudioMap.ContainsKey(owner) && _objectAudioMap[owner] == audioSource)
            {
                _objectAudioMap.Remove(owner);
            }

            Destroy(audioSource.gameObject);
        }
    }

    [Serializable]
    public struct MixerMapping
    {
        public MixerType Type;
        public AudioMixerGroup MixerGroup;
    }
}