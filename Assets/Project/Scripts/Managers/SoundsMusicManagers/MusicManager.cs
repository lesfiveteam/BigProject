using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BigProject.Managers.SoundsMusicManagers
{
    public class MusicManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _musicSource1;
        [SerializeField] private AudioSource _musicSource2;

        private bool _isSource1Active = true;
        private Coroutine _fadeCoroutine;

        public List<AudioSource> GetAudioSources()
        {
            return new List<AudioSource>{_musicSource1, _musicSource2};
        }

        /// <summary>
        /// Plays music with smooth transition.
        /// </summary>
        /// <param name="musicClip">New audio clip</param>
        /// <param name="fadeOutDuration">Duration of current music fade out (at volume 1)</param>
        /// <param name="fadeInDuration">Duration of new music fade in</param>
        /// <param name="isCrossFade">If true – fade out and fade in occur simultaneously, otherwise sequentially</param>
        public void PlayMusic(AudioClip musicClip, float fadeOutDuration = 1f, float fadeInDuration = 1f, bool isCrossFade = false)
        {
            if (musicClip == null)
            {
                GameLogManager.Error("PlayMusic called in MusicManager with null audio clip");
                throw new System.ArgumentNullException(nameof(musicClip), "Audio clip cannot be null");
            }

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
                next.volume = 0f;
            }

            if (current.isPlaying)
            {
                _fadeCoroutine = isCrossFade
                    ? StartCoroutine(CrossFadeMusic(musicClip, fadeOutDuration, fadeInDuration, current, next))
                    : StartCoroutine(SequentialFadeMusic(musicClip, fadeOutDuration, fadeInDuration, current));
            }
            else
            {
                current.clip = musicClip;
                current.volume = 0f;
                current.Play();
                _fadeCoroutine = StartCoroutine(FadeInMusic(current, fadeInDuration));
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

            if (next.isPlaying)
            {
                next.Stop();
                next.volume = 0f;
            }

            _fadeCoroutine = StartCoroutine(FadeOutMusic(current, fadeDuration));
        }

        private IEnumerator SequentialFadeMusic(AudioClip newClip, float fadeOutDuration, float fadeInDuration, AudioSource currentSource)
        {
            float startVol = currentSource.volume;
            float scaledFadeOut = fadeOutDuration * startVol;

            if (scaledFadeOut > 0f)
            {
                for (float t = 0; t < scaledFadeOut; t += Time.deltaTime)
                {
                    currentSource.volume = Mathf.Lerp(startVol, 0f, t / scaledFadeOut);
                    yield return null;
                }
            }

            currentSource.Stop();
            currentSource.clip = newClip;
            currentSource.volume = 0f;
            currentSource.Play();

            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                currentSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }

            currentSource.volume = 1f;
        }

        private IEnumerator CrossFadeMusic(AudioClip newClip, float fadeOutDuration, float fadeInDuration, AudioSource currentSource, AudioSource nextSource)
        {
            if (nextSource.isPlaying)
            {
                nextSource.Stop();
                nextSource.volume = 0f;
            }

            nextSource.clip = newClip;
            nextSource.volume = 0f;
            nextSource.Play();

            float currentStartVol = currentSource.volume;
            float scaledFadeOut = fadeOutDuration * currentStartVol;
            float maxTime = Mathf.Max(scaledFadeOut, fadeInDuration);

            for (float t = 0; t < maxTime; t += Time.deltaTime)
            {
                if (t < scaledFadeOut)
                    currentSource.volume = Mathf.Lerp(currentStartVol, 0f, t / scaledFadeOut);

                if (t < fadeInDuration)
                    nextSource.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);

                yield return null;
            }

            currentSource.volume = 0f;
            nextSource.volume = 1f;
            currentSource.Stop();
            _isSource1Active = !_isSource1Active;
        }

        private IEnumerator FadeInMusic(AudioSource source, float fadeInDuration)
        {
            for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
            {
                source.volume = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }

            source.volume = 1f;
        }

        private IEnumerator FadeOutMusic(AudioSource source, float fadeDuration)
        {
            float startVol = source.volume;
            float scaledFade = fadeDuration * startVol;

            if (scaledFade > 0f)
            {
                for (float t = 0; t < scaledFade; t += Time.deltaTime)
                {
                    source.volume = Mathf.Lerp(startVol, 0f, t / scaledFade);
                    yield return null;
                }
            }

            source.Stop();
            source.volume = 1f;
        }
    }
}