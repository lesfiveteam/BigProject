using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class SlideManager : MonoBehaviour
    {
        private const float SKIP_DURATION = 0.2f;

        public event Action OpeningEnded;

        [SerializeField] private List<Slide> _slides;
        [SerializeField] private Image _fader;

        private float _slideShowDuration = 25f;
        private float _currentSlideShowDuration;

        private float _fadeDuration = 1f;
        private float _currentFadeDuration;

        private bool _inShowProcces = false;

        private Coroutine _slideCoroutine;

        private int _currentSlideIndex = 0;

        private void Update()
        {
            InputHandler();
        }

        private void InputHandler()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (_inShowProcces)
                {
                    _slides[_currentSlideIndex].SkipShow();
                    _inShowProcces = false;
                    _currentFadeDuration = SKIP_DURATION;
                }
                else
                {
                    _slides[_currentSlideIndex].SkipHide();
                    _currentSlideShowDuration = SKIP_DURATION;
                    _currentFadeDuration = SKIP_DURATION;
                }
            }
        }

        public void StartSlideShow()
        {
            if (_slideCoroutine != null)
                StopSlideShow();

            _slideCoroutine = StartCoroutine(PlaySlider());
        }

        public void StopSlideShow()
        {
            if (_slideCoroutine != null)
            {
                foreach (Slide slide in _slides)
                    slide.Stop();

                StopCoroutine(_slideCoroutine);

                _slideCoroutine = null;
            }
        }

        private IEnumerator PlaySlider()
        {
            for (_currentSlideIndex = 0; _currentSlideIndex < _slides.Count; _currentSlideIndex++)
            {
                _currentSlideShowDuration = _slideShowDuration;
                _currentFadeDuration = _fadeDuration;

                _inShowProcces = true;

                _slides[_currentSlideIndex].Show(() => _inShowProcces = false);

                yield return Fade(1f, 0f);

                yield return DynamicWait();

                bool isHideProccesEnded = false;

                _slides[_currentSlideIndex].Hide(() => isHideProccesEnded = true);

                yield return Fade(0f, 1f);

                yield return new WaitUntil(() => isHideProccesEnded);

                _slides[_currentSlideIndex].Stop();
            }

            OpeningEnded?.Invoke();
        }

        private IEnumerator Fade(float from, float to)
        {
            Color color = _fader.color;
            float elapsed = 0f;

            while (elapsed < _currentFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _currentFadeDuration);
                float alpha = Mathf.Lerp(from, to, t);

                _fader.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            _fader.color = new Color(color.r, color.g, color.b, to);
        }

        private IEnumerator DynamicWait()
        {
            float elapsed = 0f;

            while (elapsed < _currentSlideShowDuration)
            {
                elapsed += Time.deltaTime;
                Debug.Log(_currentSlideShowDuration + " " + elapsed);
                yield return null;
            }
        }

        private void OnDestroy() => StopSlideShow();
    }
}