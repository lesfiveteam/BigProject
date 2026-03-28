using BigProject.Systems;
using BigProject.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BigProject.UI.Chat
{
    public class ChatPanelController : MonoBehaviour
    {
        [SerializeField]
        private RectTransform _rect;
        [SerializeField]
        private TMP_Text _text;
        [SerializeField]
        private RectTransform _textRect;
        [SerializeField]
        private LayoutElement _textLayoutElement;
        [SerializeField]
        private float _minWidth = 100f;
        [SerializeField]
        private float _maxWidth = 700f;
        [SerializeField]
        private bool _isWorldPanel = true;
        [SerializeField]
        private bool _isMovable;

        private void Awake()
        {
            ExceptionUtilities.ThrowIfNull(_rect, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "RectTransform"));
            ExceptionUtilities.ThrowIfNull(_text, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "TMP_Text"));
            ExceptionUtilities.ThrowIfNull(_textRect, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Text RectTransform"));
            ExceptionUtilities.ThrowIfNull(_textLayoutElement, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, $"{name}", "Text LayoutElement"));
        }

        private void LateUpdate()
        {
            if (_isWorldPanel && _isMovable)
            {
                transform.LookAt(transform.position + Camera.main.transform.forward);
            }
        }

        private void Resize()
        {
            float prefWidth = _text.GetPreferredValues(float.PositiveInfinity, float.PositiveInfinity).x;

            if (prefWidth > _maxWidth)
            {
                _text.textWrappingMode = TextWrappingModes.Normal;
                _textLayoutElement.preferredWidth = _maxWidth;
            }
            else
            {
                _text.textWrappingMode = TextWrappingModes.NoWrap;
                _textLayoutElement.preferredWidth = Mathf.Max(prefWidth, _minWidth);
            }

            _textLayoutElement.preferredHeight = _text.preferredHeight;
        }

        private void OnEnable()
        {
            if (_isWorldPanel)
            {
                transform.LookAt(transform.position + Camera.main.transform.forward);
            }

            _text.RegisterDirtyVerticesCallback(Resize);
            Resize();
        }

        private void OnDisable()
        {
            _text.UnregisterDirtyVerticesCallback(Resize);
        }
    }
}