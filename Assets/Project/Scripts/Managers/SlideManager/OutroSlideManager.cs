using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public enum Outro
    {
        First = 1,
        Second = 2,
    }

    public class OutroSlideManager : MonoBehaviour
    {
        private const float SKIP_DURATION = 0.2f;

        public event Action OutroEnded;

        [SerializeField] private List<Slide> _slidesOutro1;
        [SerializeField] private List<Slide> _slidesOutro2;
        [SerializeField] private Image _fader;
        [SerializeField] private CanvasGroup _skip;

        private List<Slide> _currentSlides;

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
            ExceptionUtilities.ThrowIfEmptyCollection(_slidesOutro1, nameof(_slidesOutro1));
            ExceptionUtilities.ThrowIfEmptyCollection(_slidesOutro2, nameof(_slidesOutro2));
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
                _currentSlides[_currentSlideIndex].SkipShow();
                _inShowProcess = false;
                _currentFadeDuration = SKIP_DURATION;
            }
            else
            {
                _currentSlides[_currentSlideIndex].SkipHide();
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
            OutroEnded?.Invoke();
        }

        public void StartSlideShow(Outro outro)
        {
            _currentSlides = GetActualSlides(outro);

            if (_slideCoroutine != null)
                StopSlideShow();

            _slideCoroutine = StartCoroutine(PlaySlider());
        }

        private List<Slide> GetActualSlides(Outro outro)
        {
            switch (outro)
            {
                case Outro.First:
                    return _slidesOutro1;

                case Outro.Second:
                    return _slidesOutro2;

                default:
                    throw new ArgumentException(outro.ToString());
            }
        }

        public void StopSlideShow()
        {
            if (_slideCoroutine != null)
            {
                foreach (Slide slide in _currentSlides)
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
            for (_currentSlideIndex = 0; _currentSlideIndex < _currentSlides.Count; _currentSlideIndex++)
            {
                _currentSlideShowDuration = _slideShowDuration;
                _currentFadeDuration = _fadeDuration;

                _inShowProcess = true;

                _currentSlides[_currentSlideIndex].Show(() => _inShowProcess = false);

                yield return Fade(1f, 0f);

                yield return DynamicWait(() => _currentSlideShowDuration);

                bool isHideProccesEnded = false;

                _currentSlides[_currentSlideIndex].Hide(() => isHideProccesEnded = true);

                yield return Fade(0f, 1f);

                yield return new WaitUntil(() => isHideProccesEnded);

                _currentSlides[_currentSlideIndex].Stop();
            }

            OutroEnded?.Invoke();
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