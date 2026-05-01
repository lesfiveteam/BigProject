using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public abstract class SlideManager<T> : MonoBehaviour, ITickable where T : Enum
    {
        private const float VISIBLE = 1f;
        private const float INVISIBLE = 0f;
        private const float SKIP_DURATION = 0.2f;

        public event Action SlideShowEnded;

        [SerializeField] private Image _fader;
        [SerializeField] private CanvasGroup _skip;
        [SerializeField] protected List<CanvasGroup> _slides1;

        protected List<CanvasGroup> _currentSlides;

        private float _longPressDuration = 2f;

        private float _onSlideDuration = 10f;
        private float _currentOnSlideDuration;

        private bool _inFadeOutProcess = false;
        private float _fadeInDuration = 1f;
        private float _onFadeDuration = 0.5f;
        private float _fadeOutDuration = 2f;
        private float _currentFadeDuration;

        private float _skipMessageFadeInOutDuration = 0.5f;
        private float _skipMessageDuration = 2f;
        private float _currentSkipMessageDuration;

        private List<Coroutine> _coroutines = new();
        private Coroutine _slideShowCoroutine;
        private Coroutine _workCoroutine;
        private Coroutine _longPressCoroutine;
        private Coroutine _skipShowCoroutine;
        private Coroutine _dynamicTimerCoroutine;

        private void Start()
        {
            ExceptionUtilities.ThrowIfEmptyCollection(_slides1, nameof(_slides1));
            ExceptionUtilities.ThrowIfNullFormat(_fader);
            ExceptionUtilities.ThrowIfNullFormat(_skip);

            _coroutines.Add(_slideShowCoroutine);
            _coroutines.Add(_workCoroutine);
            _coroutines.Add(_longPressCoroutine);
            _coroutines.Add(_skipShowCoroutine);
            _coroutines.Add(_dynamicTimerCoroutine);
        }

        protected virtual void AdditionInit() { }

        public void Tick() => InputHandler();

        public void StopSlideShow()
        {
            if (_coroutines == null || _coroutines.Count == 0)
                return;

            for (int i = 0; i < _coroutines.Count; i++)
            {
                if (_coroutines[i] != null)
                {
                    StopCoroutine(_coroutines[i]);
                    _coroutines[i] = null;
                }
            }

            _slideShowCoroutine = null;
            _workCoroutine = null;
            _longPressCoroutine = null;
            _skipShowCoroutine = null;
            _dynamicTimerCoroutine = null;

            _coroutines.Clear();
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

                _longPressCoroutine = StartCoroutine(LongPressTimerRoutine());

                if (_skipShowCoroutine != null)
                    _currentSkipMessageDuration = _skipMessageDuration;
                else
                    _skipShowCoroutine = StartCoroutine(SkipMessageRoutine());
            }

            if (!isPressed && _longPressCoroutine != null)
            {
                StopCoroutine(_longPressCoroutine);
                _longPressCoroutine = null;
            }
        }

        private void OnShortPress()
        {
            if (_inFadeOutProcess)
            {
                _currentFadeDuration = SKIP_DURATION;
            }
            else
            {
                _currentOnSlideDuration = SKIP_DURATION;
                _currentFadeDuration = SKIP_DURATION;
            }
        }

        private IEnumerator LongPressTimerRoutine()
        {
            yield return new WaitForSeconds(_longPressDuration);
            OnLongPress();
            _longPressCoroutine = null;
        }

        private void OnLongPress()
        {
            StopSlideShow();
            SlideShowEnded?.Invoke();
        }

        public void StartSlideShow(T slidesVariant = default)
        {
            _currentSlides = GetActualSlides(slidesVariant);

            if (_slideShowCoroutine != null)
                StopSlideShow();

            _slideShowCoroutine = StartCoroutine(SlideShowRoutine());
        }

        protected abstract List<CanvasGroup> GetActualSlides(T slidesVariant);

        protected IEnumerator SlideShowRoutine()
        {
            for (int i = 0; i < _currentSlides.Count; i++)
            {
                _inFadeOutProcess = true;

                _currentSlides[i].alpha = VISIBLE;

                // waiting on fade
                yield return _workCoroutine = StartCoroutine(DynamicWaitRoutine(() => _onFadeDuration));
                _workCoroutine = null;

                // go out fade
                _currentFadeDuration = _fadeOutDuration;
                yield return _workCoroutine = StartCoroutine(FadeRoutine(1f, 0f));
                _workCoroutine = null;

                _inFadeOutProcess = false;

                // waiting on slide
                _currentOnSlideDuration = _onSlideDuration;
                yield return _workCoroutine = StartCoroutine(DynamicWaitRoutine(() => _currentOnSlideDuration));
                _workCoroutine = null;

                // go in fade
                if (_currentFadeDuration != SKIP_DURATION)
                    _currentFadeDuration = _fadeInDuration;

                yield return _workCoroutine = StartCoroutine(FadeRoutine(0f, 1f));
                _workCoroutine = null;

                _currentSlides[i].alpha = INVISIBLE;
            }

            SlideShowEnded?.Invoke();
        }

        private IEnumerator FadeRoutine(float from, float to)
        {
            Color color = _fader.color;
            float elapsed = 0f;

            while (elapsed < _currentFadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _currentFadeDuration);
                color.a = Mathf.Lerp(from, to, t);

                _fader.color = color;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }

            color.a = to;
            _fader.color = color;
        }

        private IEnumerator SkipMessageRoutine()
        {
            _currentSkipMessageDuration = _skipMessageDuration;

            float elapsed = 0f;

            while (elapsed < _skipMessageFadeInOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _skipMessageFadeInOutDuration);
                float alpha = Mathf.Lerp(0f, 1f, t);

                _skip.alpha = alpha;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }

            _skip.alpha = 1f;

            yield return _dynamicTimerCoroutine = StartCoroutine(DynamicTimerRoutine());

            elapsed = 0f;

            while (elapsed < _skipMessageFadeInOutDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / _skipMessageFadeInOutDuration);
                float alpha = Mathf.Lerp(1f, 0f, t);

                _skip.alpha = alpha;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }

            _skip.alpha = 0f;

            _skipShowCoroutine = null;
        }

        private IEnumerator DynamicWaitRoutine(Func<float> getDuration)
        {
            float elapsed = 0f;

            while (elapsed < getDuration())
            {
                elapsed += Time.deltaTime;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }
        }

        private IEnumerator DynamicTimerRoutine()
        {
            while (_currentSkipMessageDuration > 0)
            {
                _currentSkipMessageDuration -= Time.deltaTime;

                yield return new WaitWhile(() => Time.timeScale == 0);
            }
        }

        private void OnDisable()
        {
            if (_coroutines != null)
                StopSlideShow();
        }
    }
}