using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SceneLoader
{
    public class Preloader : MonoBehaviour
    {
        private const float VISIBLE = 1f;
        private const float INVISIBLE = 0f;

        private const float FRAME_RATE = 12f;
        private const float FADE_DURATION = 2f;

        [SerializeField] private Image _targetImage;
        [SerializeField] private List<Sprite> _sprites;

        private Coroutine _animationCoroutine;
        private Coroutine _fadeCoroutine;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_targetImage);
            ExceptionUtilities.ThrowIfEmptyCollection(_sprites, "_sprites");
        }

        public void Play(float duration, Action preloaderCallback)
        {
            Stop();

            _animationCoroutine = StartCoroutine(AnimateRoutine(duration, preloaderCallback));
        }

        public void Stop()
        {
            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            SetImageVisible(false);
        }

        private IEnumerator AnimateRoutine(float duration, Action preloaderCallback)
        {
            _targetImage.sprite = _sprites[0];

            _fadeCoroutine = StartCoroutine(FadeRoutine(INVISIBLE, VISIBLE, FADE_DURATION));

            float frameDuration = 1f / FRAME_RATE;
            float spriteTimer = 0f;
            int currentIndex = 0;

            while (spriteTimer < duration)
            {
                _targetImage.sprite = _sprites[currentIndex];

                currentIndex = (currentIndex + 1) % _sprites.Count;

                float waitTime = frameDuration;
                float remaining = duration - spriteTimer;

                if (remaining < waitTime)
                    waitTime = remaining;

                yield return new WaitForSeconds(waitTime);

                spriteTimer += frameDuration;
            }

            preloaderCallback?.Invoke();

            Stop();
        }

        private IEnumerator FadeRoutine(float from, float to, float duration)
        {
            Color color = _targetImage.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                color.a = Mathf.Lerp(from, to, t);
                _targetImage.color = color;

                yield return new WaitForEndOfFrame();
            }

            color.a = to;
            _targetImage.color = color;
        }

        private void SetImageVisible(bool isVisible)
        {
            Color color = _targetImage.color;
            color.a = isVisible ? VISIBLE : INVISIBLE;
            _targetImage.color = color;
        }

        private void OnDisable()
        {
            Stop();
        }
    }
}