using BigProject.Utilities;
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

        public event Action IntroEnded;

        [SerializeField] private List<Slide> _slides;
        [SerializeField] private Image _fader;
        [SerializeField] private CanvasGroup _skip;

        private float _slideShowDuration = 25f;
        private float _currentSlideShowDuration;

        private float _fadeDuration = 1f;
        private float _currentFadeDuration;

        private bool _inShowProcess = false;

        private Coroutine _slideCoroutine;
        private Coroutine _longPressCoroutine;
        private Coroutine _skipShowCoroutine;

        private int _currentSlideIndex = 0;

        private float _longPressDuration = 2f;
        private float _skipFadeDuration = 0.5f;
        private float _skipShowDuration = 2f;
        private float _skipShowTimer;

        private void Start()
        {
            ExceptionUtilities.ThrowIfEmptyCollection(_slides, nameof(_slides));
            ExceptionUtilities.ThrowIfNullFormat(_fader);
            ExceptionUtilities.ThrowIfNullFormat(_skip);
        }

        private void Update()
        {
            InputHandler();
        }

        private void InputHandler()
        {
            bool isPressed = Mouse.current.leftButton.isPressed || Keyboard.current.spaceKey.isPressed;
            bool wasPressed = Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;

            if (wasPressed)
            {
                OnShortPress();
                
                if (_longPressCoroutine != null)
                    StopCoroutine(_longPressCoroutine);

                _longPressCoroutine = StartCoroutine(LongPressTimer());

                if (_skipShowCoroutine != null)
                    _skipShowTimer = _skipShowDuration;
                else
                    _skipShowCoroutine = StartCoroutine(FadeSkip());
            }

            if (!isPressed && _longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }
        }

        private void OnShortPress()
        {
            if (_inShowProcess)
            {
                _slides[_currentSlideIndex].SkipShow();
                _inShowProcess = false;
                _currentFadeDuration = SKIP_DURATION;
            }
            else
            {
                _slides[_currentSlideIndex].SkipHide();
                _currentSlideShowDuration = SKIP_DURATION;
                _currentFadeDuration = SKIP_DURATION;
            }
        }

        private IEnumerator LongPressTimer()
        {
            yield return new WaitForSeconds(_longPressDuration);
            OnLongPress();
            _longPressCoroutine = null;
        }

        private void OnLongPress()
        {
            StopSlideShow();
            IntroEnded?.Invoke();
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

            if (_longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }

            if (_skipShowCoroutine  != null)
            {
                StopCoroutine(_skipShowCoroutine);
                _skipShowCoroutine = null;
            }

        }

        private IEnumerator PlaySlider()
        {
            for (_currentSlideIndex = 0; _currentSlideIndex < _slides.Count; _currentSlideIndex++)
            {
                _currentSlideShowDuration = _slideShowDuration;
                _currentFadeDuration = _fadeDuration;

                _inShowProcess = true;

                _slides[_currentSlideIndex].Show(() => _inShowProcess = false);

                yield return Fade(1f, 0f);

                yield return DynamicWait(() => _currentSlideShowDuration);

                bool isHideProccesEnded = false;

                _slides[_currentSlideIndex].Hide(() => isHideProccesEnded = true);

                yield return Fade(0f, 1f);

                yield return new WaitUntil(() => isHideProccesEnded);

                _slides[_currentSlideIndex].Stop();
            }

            IntroEnded?.Invoke();
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

        private IEnumerator FadeSkip()
        {
            _skipShowTimer = _skipShowDuration;

            float elapsed = 0f;

            while (elapsed < _skipFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _skipFadeDuration);
                float alpha = Mathf.Lerp(0f, 1f, t);

                _skip.alpha = alpha;

                yield return null;
            }

            _skip.alpha = 1f;

            yield return DynamicTimer();

            elapsed = 0f;

            while (elapsed < _skipFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _skipFadeDuration);
                float alpha = Mathf.Lerp(1f, 0f, t);

                _skip.alpha = alpha;

                yield return null;
            }

            _skip.alpha = 0f;

            _skipShowCoroutine = null;
        }

        private IEnumerator DynamicWait(Func<float> getDuration)
        {
            float elapsed = 0f;

            while (elapsed < getDuration())
            {
                elapsed += Time.deltaTime;

                yield return null;
            }
        }

        private IEnumerator DynamicTimer()
        {
            while (_skipShowTimer > 0)
            {
                _skipShowTimer -= Time.deltaTime;

                yield return null;
            }
        }

        private void OnDestroy() => StopSlideShow();
    }
}