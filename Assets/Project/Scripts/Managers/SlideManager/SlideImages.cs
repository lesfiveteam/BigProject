using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class SlideImages : Slide
    {
        [SerializeField] private float _fadeDuration = 1f;
        [SerializeField] private float _delayImages = 0.5f;
        [SerializeField] private float _delayText = 0.5f;
        [SerializeField] private List<Image> _slides;
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private bool _isRolling = false;
        [SerializeField] private float _rollingDistance = 1000f;
        [SerializeField] private float _rollingDuration = 5f;

        private Coroutine _slideCoroutine;
        private List<Coroutine> _slideCoroutines = new();

        private bool _isHiding = false;
        private Color _zeroAlpha = new Color(1, 1, 1, 0);

        private void Awake()
        {
            if (_slides.Count > 0)
                foreach (Image slide in _slides)
                    slide.color = _zeroAlpha;

            if (_text != null)
                _text.color = Color.clear;
        }

        public override void Play()
        {
            if (_slides.Count == 0)
                throw new InvalidOperationException("SlideImages collection is empty");

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(PlaySlides());
        }

        public override void Hide()
        {
            _isHiding = true;

            Stop();

            _slideCoroutine = StartCoroutine(HideSlides());
        }

        public override void Stop()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }

            foreach (Coroutine coroutine in _slideCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            _slideCoroutines.Clear();
        }

        private IEnumerator PlaySlides()
        {
            for (int i = 0; i < _slides.Count; i++)
            {
                if (_isHiding)
                    continue;

                Coroutine coroutine = StartCoroutine(ShowSlide(_slides[i]));
                _slideCoroutines.Add(coroutine);

                yield return new WaitForSeconds(_delayImages);
            }

            yield return new WaitForSeconds(_delayText);

            Coroutine coroutineText = StartCoroutine(ShowText());
            _slideCoroutines.Add(coroutineText);

            if (_isRolling)
            {
                yield return new WaitForSeconds(_delayText);

                Coroutine coroutineRollText = StartCoroutine(RollText());
                _slideCoroutines.Add(coroutineRollText);
            }
        }

        private IEnumerator HideSlides()
        {
            for (int i = 1; i < _slides.Count; i++)
            {
                Coroutine coroutine = StartCoroutine(HideSlide(_slides[i]));

                _slideCoroutines.Add(coroutine);
                yield return new WaitForSeconds(_delayImages);
            }

            Coroutine coroutineText = StartCoroutine(HideText());
            _slideCoroutines.Add(coroutineText);
        }

        private IEnumerator ShowSlide(Image image)
        {
            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;

                if (_isHiding)
                    continue;

                float alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                image.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            image.color = new Color(color.r, color.g, color.b, 1f);
        }

        private IEnumerator ShowText()
        {
            Color color = _text.color;
            Vector3 textStartPosition = _text.transform.position;
            Vector3 textEndPosition = textStartPosition + Vector3.up * _rollingDistance;

            float elapsed = 0f;

            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;

                if (_isHiding)
                    continue;

                if (_isRolling)
                {
                    float t = Mathf.Clamp01(elapsed / _rollingDuration);
                    _text.transform.position = Vector3.Lerp(textStartPosition, textEndPosition, t);
                }

                float alpha = Mathf.Clamp01(elapsed / _fadeDuration);
                _text.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            _text.color = new Color(color.r, color.g, color.b, 1f);
        }

        private IEnumerator RollText()
        {
            Vector3 textStartPosition = _text.transform.position;
            Vector3 textEndPosition = textStartPosition + Vector3.up * _rollingDistance;

            float elapsed = 0f;

            while (elapsed < _rollingDuration)
            {
                elapsed += Time.deltaTime;

                if (_isHiding)
                    continue;

                float t = Mathf.Clamp01(elapsed / _rollingDuration);
                _text.transform.position = Vector3.Lerp(textStartPosition, textEndPosition, t);

                yield return null;
            }
        }

        private IEnumerator HideSlide(Image image)
        {
            Color color = image.color;
            float duration = _fadeDuration * color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Lerp(color.a, 0f, t);

                image.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            image.color = new Color(color.r, color.g, color.b, 0f);
        }

        private IEnumerator HideText()
        {
            Color color = _text.color;
            float duration = _fadeDuration * color.a;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Lerp(color.a, 0f, t);

                _text.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            _text.color = new Color(color.r, color.g, color.b, 0f);
        }
    }
}