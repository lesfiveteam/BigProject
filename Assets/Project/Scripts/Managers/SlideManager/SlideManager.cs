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
        public event Action OpeningEnded;

        [SerializeField] private List<Slide> _slides;
        [SerializeField] private Image _fader;

        [SerializeField] private float _slideDuration = 7f;
        [SerializeField] private float _hideDuration = 5f;

        [SerializeField] private float _fadeDuration = 0.3f;
        [SerializeField] private float _fadeInterval = 0.3f;

        private Coroutine _slideCoroutine;

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
            for (int i = 0; i < _slides.Count; i++)
            {
                _slides[i].Play();

                yield return Fade(1f, 0f, _fadeDuration);

                yield return WaitForSlide();

                _slides[i].Hide();

                yield return new WaitForSeconds(_hideDuration);

                yield return Fade(0f, 1f, _fadeDuration);

                _slides[i].Stop();

                yield return new WaitForSeconds(_fadeInterval);
            }

            OpeningEnded?.Invoke();
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            Color color = _fader.color;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float alpha = Mathf.Lerp(from, to, t);
                _fader.color = new Color(color.r, color.g, color.b, alpha);

                yield return null;
            }

            _fader.color = new Color(color.r, color.g, color.b, to);
        }

        private IEnumerator WaitForSlide()
        {
            float elapsed = 0f;
            bool clicked = false;

            while (elapsed < _slideDuration && !clicked)
            {
                elapsed += Time.deltaTime;

                if (Mouse.current != null & Mouse.current.leftButton.wasPressedThisFrame)
                    clicked = true;

                yield return null;
            }


        }

        private void OnDestroy() => StopSlideShow();
    }
}