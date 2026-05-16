using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BigProject.Utilities;

namespace BigProject.Managers.SoundsMusicManagers
{
    public class MusicManager : MonoBehaviour
    {
        private const float SILENT_VOLUME = 0f;

        [SerializeField] private AudioSource _musicSource1;
        [SerializeField] private AudioSource _musicSource2;

        private bool _isSource1Active = true;
        private Coroutine _fadeCoroutine;
        private Coroutine _crossCoroutine;
        private float _maxVolume = 1f;

        //public List<AudioSource> GetAudioSources() => new() { _musicSource1, _musicSource2 };

        public void SetVolume(float volume)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }

            if (_crossCoroutine != null)
            {
                StopCoroutine(_crossCoroutine);
            }

            _musicSource2.Stop();
            _musicSource2.volume = SILENT_VOLUME;
            _musicSource1.volume = volume;
            _isSource1Active = true;
            _maxVolume = volume;

            if (!_musicSource1.isPlaying)
            {
                _musicSource1.Play();
            }
        }

        /// <summary>
        /// Plays music with smooth transition.
        /// </summary>
        /// <param name="musicClip">New audio clip</param>
        /// <param name="fadeOutDuration">Duration of current music fade out (at volume 1)</param>
        /// <param name="fadeInDuration">Duration of new music fade in</param>
        /// <param name="maxVolume">Max volume for music</param>
        /// <param name="isCrossFade">If true – fade out and fade in occur simultaneously, otherwise sequentially</param>
        /// <param name="isCrossFadeForOne">If true – fade out and fade in occur simultaneously for one audio clip infinitely. isCrossFade must be true</param>
        public void PlayMusic(
            AudioClip musicClip, 
            float fadeOutDuration = 1f,
            float fadeInDuration = 1f,
            float maxVolume = 1f,
            bool isCrossFade = false,
            bool isCrossFadeForOne = false)
        {
            ExceptionUtilities.ThrowIfNullFormat(musicClip);

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            AudioSource current = _isSource1Active ? _musicSource1 : _musicSource2;
            AudioSource next = _isSource1Active ? _musicSource2 : _musicSource1;

            if (next.isPlaying)
            {
                next.Stop();
                next.volume = SILENT_VOLUME;
            }

            maxVolume = Mathf.Min(maxVolume, _maxVolume);

            if (current.isPlaying)
            {
                _fadeCoroutine = isCrossFade
                    ? StartCoroutine(CrossFadeMusic(musicClip, fadeOutDuration, fadeInDuration, maxVolume, current, next, isCrossFadeForOne))
                    : StartCoroutine(SequentialFadeMusic(musicClip, fadeOutDuration, fadeInDuration, maxVolume, current));
            }
            else
            {
                current.clip = musicClip;
                current.volume = SILENT_VOLUME;
                current.Play();
                _fadeCoroutine = StartCoroutine(FadeInMusic(current, maxVolume, fadeInDuration));
            }
        }

        /// <summary>
        /// Stops current music with fade out.
        /// </summary>
        /// <param name="fadeDuration">Duration of fade out (at volume 1)</param>
        public void StopMusic(float fadeDuration = 1f)
        {
            AudioSource current = _isSource1Active ? _musicSource1 : _musicSource2;
            AudioSource next = _isSource1Active ? _musicSource2 : _musicSource1;

            if (!current.isPlaying && !next.isPlaying)
                return;

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_crossCoroutine != null)
            {
                StopCoroutine(_crossCoroutine);
                _crossCoroutine = null;
            }

            if (next.isPlaying)
            {
                next.Stop();
                next.volume = SILENT_VOLUME;
            }

            _fadeCoroutine = StartCoroutine(FadeOutMusic(current, fadeDuration));
        }

        private IEnumerator SequentialFadeMusic(
            AudioClip newClip, 
            float fadeOutDuration, 
            float fadeInDuration,
            float maxVolume,
            AudioSource currentSource)
        {
            float startVol = currentSource.volume;
            float scaledFadeOut = fadeOutDuration * startVol;

            if (scaledFadeOut > 0f)
            {
                for (float t = 0; t < scaledFadeOut; t += Time.deltaTime)
                {
                    currentSource.volume = Mathf.Lerp(startVol, SILENT_VOLUME, t / scaledFadeOut);
                    yield return null;
                }
            }

            currentSource.Stop();
            currentSource.clip = newClip;
            currentSource.volume = SILENT_VOLUME;
            currentSource.Play();

            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                currentSource.volume = Mathf.Lerp(SILENT_VOLUME, maxVolume, t / fadeInDuration);
                yield return null;
            }

            currentSource.volume = maxVolume;
        }

        private IEnumerator CrossFadeMusic(
            AudioClip newClip, 
            float fadeOutDuration, 
            float fadeInDuration,
            float maxVolume,
            AudioSource currentSource, 
            AudioSource nextSource,
            bool isCrossFadeForOne)
        {
            if (nextSource.isPlaying)
            {
                nextSource.Stop();
                nextSource.volume = SILENT_VOLUME;
            }

            nextSource.clip = newClip;
            nextSource.volume = SILENT_VOLUME;
            nextSource.Play();

            float currentStartVol = currentSource.volume;
            float scaledFadeOut = fadeOutDuration * currentStartVol;
            float maxTime = Mathf.Max(scaledFadeOut, fadeInDuration);

            for (float t = 0; t < maxTime; t += Time.deltaTime)
            {
                if (t < scaledFadeOut)
                    currentSource.volume = Mathf.Lerp(currentStartVol, SILENT_VOLUME, t / scaledFadeOut);

                if (t < fadeInDuration)
                    nextSource.volume = Mathf.Lerp(SILENT_VOLUME, maxVolume, t / fadeInDuration);

                yield return null;
            }

            currentSource.volume = SILENT_VOLUME;
            currentSource.Stop();
            nextSource.volume = maxVolume;

            _isSource1Active = !_isSource1Active;

            if (isCrossFadeForOne)
            {
                if (_crossCoroutine != null)
                    StopCoroutine(_crossCoroutine);

                _crossCoroutine = StartCoroutine(
                    ReplayCrossFadeMusic(newClip, fadeOutDuration, fadeInDuration, maxVolume, nextSource, currentSource));
            }
        }

        private IEnumerator ReplayCrossFadeMusic(
            AudioClip clip,
            float fadeOutDuration,
            float fadeInDuration,
            float maxVolume,
            AudioSource currentSource,
            AudioSource nextSource)
        {
            float timeToStartSwitch = Mathf.Max(0f, currentSource.clip.length - currentSource.time - fadeOutDuration);

            yield return new WaitForSeconds(timeToStartSwitch);

            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(
                CrossFadeMusic(clip, fadeOutDuration, fadeInDuration, maxVolume, currentSource, nextSource, true));

            _crossCoroutine = null;
        }

        private IEnumerator FadeInMusic(AudioSource source, float maxVolume, float fadeInDuration)
        {
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(SILENT_VOLUME, maxVolume, t / fadeInDuration);
                yield return null;
            }

            source.volume = maxVolume;
        }

        private IEnumerator FadeOutMusic(AudioSource source, float fadeDuration)
        {
            float startVol = source.volume;
            float scaledFade = fadeDuration * startVol;

            if (scaledFade > 0f)
            {
                for (float t = 0; t < scaledFade; t += Time.deltaTime)
                {
                    source.volume = Mathf.Lerp(startVol, SILENT_VOLUME, t / scaledFade);
                    yield return null;
                }
            }

            source.Stop();
            source.volume = SILENT_VOLUME;
        }

        private void OnDisable()
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            if (_crossCoroutine != null)
            {
                StopCoroutine(_crossCoroutine);
                _crossCoroutine = null;
            }
        }
    }
}