using BigProject.Managers.SoundsMusicManagers;
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
        private const float INVISIBLE = 0f;
        private const float VISIBLE = 1f;

        private const float DEFAULT_SLIDE_DURATION = 22f;
        private const float SKIPED_DURATION = 0.1f;

        private const float FADE_IN_DURATION = 1f;
        private const float ON_FADE_DURATION = 0.5f;
        private const float FADE_OUT_DURATION = 2f;

        private const float SKIP_HINT_DURATION = 2f;
        private const float SKIP_HINT_FADE_IN_OUT_DURATION = 0.2f;

        private const float HOLD_BUTTON_FOR_SKIP_DURATION = 2f;

        public event Action SlideShowEnded;

        [SerializeField] private Image _fader;
        [SerializeField] private CanvasGroup _skipHint;
        [SerializeField] private Image _filler;
        [SerializeField] protected List<SlidesGroup> _slidesGroups;

        protected SlidesGroup _currentSlides;
        private MusicManager _musicManager;

        private bool _inFadeOutProcess = false;
        private bool _isButtonPressed = false;

        private float _currentSlideDuration = DEFAULT_SLIDE_DURATION;
        private float _currentFadeDuration;
        private float _currentSkipHintDuration;

        private List<Coroutine> _coroutines = new();
        private Coroutine _slideShowCoroutine;
        private Coroutine _workCoroutine;
        private Coroutine _skipSlideShowCoroutine;
        private Coroutine _skipHintCoroutine;
        private Coroutine _dynamicTimerCoroutine;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_fader);
            ExceptionUtilities.ThrowIfNullFormat(_skipHint);
            ExceptionUtilities.ThrowIfNullFormat(_filler);
            ExceptionUtilities.ThrowIfEmptyCollection(_slidesGroups, nameof(_slidesGroups));

            _coroutines.Add(_slideShowCoroutine);
            _coroutines.Add(_workCoroutine);
            _coroutines.Add(_skipSlideShowCoroutine);
            _coroutines.Add(_skipHintCoroutine);
            _coroutines.Add(_dynamicTimerCoroutine);

            _currentSkipHintDuration = SKIP_HINT_DURATION;
        }

        public void Init(MusicManager musicManager)
        {
            _musicManager = musicManager;
        }

        public void Tick() => InputHandler();

        public void StartSlideShow(T slidesVariant = default)
        {
            ExceptionUtilities.ThrowIfNullFormat(_musicManager);

            _currentSlides = GetActualSlidesGroup(slidesVariant);

            if (_slideShowCoroutine != null)
                StopSlideShow();

            _slideShowCoroutine = StartCoroutine(SlideShowRoutine());
        }

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
            _skipSlideShowCoroutine = null;
            _skipHintCoroutine = null;
            _dynamicTimerCoroutine = null;
        }

        protected abstract SlidesGroup GetActualSlidesGroup(T slidesVariant);

        protected IEnumerator SlideShowRoutine()
        {
            List<Slide> slides = _currentSlides.Slides;

            _musicManager.PlayMusic(_currentSlides.Music, maxVolume: _currentSlides.MusicVolume);

            for (int i = 0; i < slides.Count; i++)
            {
                _inFadeOutProcess = true;

                slides[i].Group.alpha = VISIBLE;

                // waiting on fade
                yield return new WaitForSeconds(ON_FADE_DURATION);

                // go out fade
                _currentFadeDuration = FADE_OUT_DURATION;
                yield return _workCoroutine = StartCoroutine(FadeRoutine(VISIBLE, INVISIBLE));
                _workCoroutine = null;

                _inFadeOutProcess = false;

                // waiting on slide
                _currentSlideDuration = slides[i].SlideTime;
                yield return _workCoroutine = StartCoroutine(DynamicWaitRoutine(() => _currentSlideDuration));
                _workCoroutine = null;

                // go in fade
                if (_currentFadeDuration != SKIPED_DURATION)
                    _currentFadeDuration = FADE_IN_DURATION;

                yield return _workCoroutine = StartCoroutine(FadeRoutine(INVISIBLE, VISIBLE));
                _workCoroutine = null;

                slides[i].Group.alpha = INVISIBLE;
            }

            _musicManager.StopMusic();

            SlideShowEnded?.Invoke();
        }

        private void InputHandler()
        {
            bool wasPressed = Mouse.current.leftButton.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
            bool wasEscPressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            bool isEscPressed = Keyboard.current.escapeKey.isPressed;

            if (wasPressed)
            {
                if (!_isButtonPressed)
                {
                    _isButtonPressed = true;
                    _skipHintCoroutine = StartCoroutine(SkipHintRoutine());
                }

                OnShortPress();
            }

            if (wasEscPressed)
            {
                if (!_isButtonPressed)
                    _isButtonPressed = true;

                if (_skipHintCoroutine != null)
                    _currentSkipHintDuration = SKIP_HINT_DURATION;
                else
                    _skipHintCoroutine = StartCoroutine(SkipHintRoutine());

                if (_skipSlideShowCoroutine != null)
                    StopCoroutine(_skipSlideShowCoroutine);

                _skipSlideShowCoroutine = StartCoroutine(SkipSlideShowRoutine());
            }

            if (isEscPressed)
            {
                _currentSkipHintDuration = SKIP_HINT_DURATION;
            }

            if (!isEscPressed && _skipSlideShowCoroutine != null)
            {
                _filler.fillAmount = 0f;

                StopCoroutine(_skipSlideShowCoroutine);
                _skipSlideShowCoroutine = null;
            }
        }

        private void OnShortPress()
        {
            if (_inFadeOutProcess)
            {
                _currentFadeDuration = SKIPED_DURATION;
            }
            else
            {
                _currentSlideDuration = SKIPED_DURATION;
                _currentFadeDuration = SKIPED_DURATION;
            }
        }

        private IEnumerator SkipSlideShowRoutine()
        {
            float elapsed = 0f;
            _filler.fillAmount = 0f;

            while (elapsed < HOLD_BUTTON_FOR_SKIP_DURATION)
            {
                elapsed += Time.deltaTime;
                _filler.fillAmount = Mathf.Clamp01(elapsed / HOLD_BUTTON_FOR_SKIP_DURATION);
                yield return null;
            }

            _filler.fillAmount = 1f;

            SkipSlideShow();
            _skipSlideShowCoroutine = null;
        }

        private void SkipSlideShow()
        {
            StopSlideShow();
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

                yield return null;
            }

            color.a = to;
            _fader.color = color;
        }

        private IEnumerator SkipHintRoutine()
        {
            _currentSkipHintDuration = SKIP_HINT_DURATION;

            float elapsed = 0f;

            while (elapsed < SKIP_HINT_FADE_IN_OUT_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SKIP_HINT_FADE_IN_OUT_DURATION);
                float alpha = Mathf.Lerp(0f, 1f, t);

                _skipHint.alpha = alpha;

                yield return null;
            }

            _skipHint.alpha = 1f;

            yield return _dynamicTimerCoroutine = StartCoroutine(DynamicTimerRoutine());

            elapsed = 0f;

            while (elapsed < SKIP_HINT_FADE_IN_OUT_DURATION)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / SKIP_HINT_FADE_IN_OUT_DURATION);
                float alpha = Mathf.Lerp(1f, 0f, t);

                _skipHint.alpha = alpha;

                yield return null;
            }

            _skipHint.alpha = 0f;

            _skipHintCoroutine = null;
        }

        private IEnumerator DynamicWaitRoutine(Func<float> getDuration)
        {
            float elapsed = 0f;

            while (elapsed < getDuration())
            {
                elapsed += Time.deltaTime;

                yield return null;
            }
        }

        private IEnumerator DynamicTimerRoutine()
        {
            while (_currentSkipHintDuration > 0)
            {
                _currentSkipHintDuration -= Time.deltaTime;

                yield return null;
            }
        }

        private void OnDisable()
        {
            if (_coroutines.Count > 0)
                StopSlideShow();
        }
    }
}