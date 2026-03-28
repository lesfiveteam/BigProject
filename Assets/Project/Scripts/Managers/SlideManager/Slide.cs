using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Project.Scripts.Managers.SlideManager
{
    public class Slide : MonoBehaviour
    {
        private const float DEFAULT_SPRITE_DURATION = 0.3f;

        [SerializeField] private Image _targetImage;
        [SerializeField] private float _spriteDuration;
        [SerializeField] private List<Sprite> _slides;

        private Coroutine _slideCoroutine;
        private int _currentIndex;

        public void Play(float spriteDuration = DEFAULT_SPRITE_DURATION)
        {
            if (_slides.Count == 0)
                throw new InvalidOperationException("SlideImages collection is empty");

            if (_slides.Count == 1)
            {
                _targetImage.sprite = _slides[0];
                return;
            }

            if (spriteDuration != 0)
                _spriteDuration = spriteDuration;

            _targetImage.color = new Color(1f, 1f, 1f, 1f);

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _currentIndex = 0;

            _slideCoroutine = StartCoroutine(PlaySlides());
        }

        public void Stop()
        {
            _targetImage.color = new Color(1f, 1f, 1f, 0f);

            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }
        }

        private IEnumerator PlaySlides()
        {
            while(true)
            {
                if (_slides[_currentIndex] != null)
                    _targetImage.sprite = _slides[_currentIndex];

                yield return new WaitForSeconds(_spriteDuration);

                _currentIndex++;

                if (_currentIndex >= _slides.Count)
                    _currentIndex = 0;
            }
        }

        private void OnDestroy() => Stop();
    }
}