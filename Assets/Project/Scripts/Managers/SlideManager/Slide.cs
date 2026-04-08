using BigProject.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class Slide : MonoBehaviour
    {
        private const float SKIP_DURATION = 0.1f;

        [Header("Images")]
        [SerializeField] private List<Image> _images;

        [SerializeField] private float _imageShowDuration = 1f;
        [SerializeField] private float _imageHideDuration = 1f;

        [SerializeField] private float _imageShowDelay = 0.5f;
        [SerializeField] private float _imageHideDelay = 0.5f;

        [Header("Text")]
        [SerializeField] private TextMeshProUGUI _text;

        [SerializeField] private bool _isRolling = false;
        [SerializeField] private float _textRollingDelay = 0.5f;
        [SerializeField] private float _rollingDistance = 1000f;
        [SerializeField] private float _rollingDuration = 5f;

        private Coroutine _slideCoroutine;
        private List<Coroutine> _imageCoroutines = new();

        private bool _isHiding = false;
        private Color _zeroAlpha = new Color(1, 1, 1, 0);

        private void Awake()
        {
            if (_images.Count > 0)
                foreach (Image image in _images)
                    image.color = _zeroAlpha;

            if (_text != null)
                _text.color = Color.clear;
        }

        public void Show(Action onEnd)
        {
            ExceptionUtilities.ThrowIfEmptyCollection(_images, nameof(_images));

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(ShowImages(onEnd));
        }

        public void Hide(Action onEnd)
        {
            _isHiding = true;

            Stop();

            _slideCoroutine = StartCoroutine(HideImages(onEnd));
        }

        public void Stop()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }

            foreach (Coroutine coroutine in _imageCoroutines)
            {
                if (coroutine != null)
                    StopCoroutine(coroutine);
            }

            _imageCoroutines.Clear();
        }

        public void SkipShow()
        {
            _imageShowDuration = SKIP_DURATION;
            _imageShowDelay = SKIP_DURATION;
        }

        public void SkipHide()
        {
            _imageHideDuration = SKIP_DURATION;
            _imageHideDelay = SKIP_DURATION;
        }

        private IEnumerator ShowImages(Action onEnd)
        {
            for (int i = 0; i < _images.Count; i++)
            {
                if (_isHiding)
                    continue;

                Coroutine coroutine = StartCoroutine(ShowImage(_images[i]));
                _imageCoroutines.Add(coroutine);

                if (i > 0)
                    yield return new WaitForSeconds(_imageShowDelay);
            }

            Coroutine coroutineText = StartCoroutine(ShowText());
            _imageCoroutines.Add(coroutineText);

            if (_isRolling)
            {
                yield return new WaitForSeconds(_textRollingDelay);

                Coroutine coroutineRollText = StartCoroutine(RollText());
                _imageCoroutines.Add(coroutineRollText);
            }

            onEnd?.Invoke();
        }

        private IEnumerator ShowImage(Image image)
        {
            Color color = image.color;
            float elapsed = 0f;

            while (elapsed < _imageShowDuration)
            {
                elapsed += Time.deltaTime;

                if (_isHiding)
                    continue;

                float alpha = Mathf.Clamp01(elapsed / _imageShowDuration);
                image.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            image.color = new Color(color.r, color.g, color.b, 1f);
        }

        private IEnumerator ShowText()
        {
            Color color = _text.color;

            float elapsed = 0f;

            while (elapsed < _imageShowDuration)
            {
                elapsed += Time.deltaTime;

                if (_isHiding)
                    continue;

                float alpha = Mathf.Clamp01(elapsed / _imageShowDuration);
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

        private IEnumerator HideImages(Action onEnd)
        {
            for (int i = 1; i < _images.Count; i++)
            {
                Coroutine coroutine = StartCoroutine(HideImage(_images[i]));
                _imageCoroutines.Add(coroutine);

                yield return new WaitForSeconds(_imageHideDelay);
            }

            Coroutine coroutineText = StartCoroutine(HideText());
            _imageCoroutines.Add(coroutineText);

            foreach (Coroutine cor in _imageCoroutines)
            {
                yield return cor;
            }

            onEnd?.Invoke();
        }

        private IEnumerator HideImage(Image image)
        {
            Color color = image.color;
            float duration = _imageHideDuration * color.a;
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
            float duration = _imageHideDuration * color.a;
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