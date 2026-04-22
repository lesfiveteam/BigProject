using BigProject.Intercatable;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Systems.QuestSystem
{
    public class QuestInteractableHandler : MonoBehaviour, IInteractable
    {
        [SerializeField]
        private int _questId;
        [SerializeField]
        private int _actionId;
        [SerializeField]
        private int _transitionId;
        [SerializeField]
        private bool _destroyAfterInteract = false;
        [SerializeField]
        private bool _destroyWithGameObject = false;
        [SerializeField]
        private Transform _targetPosition;
        [SerializeField]
        private AudioClip _interactSound;
        [SerializeField]
        private float _interactSoundVolume = 1f;

        private SoundsManager _soundsManager;
        private ProgressManager _progressManager;
        private IQuestActionHandler _actionHandler;

        private void Start()
        {
            SetTransition(_actionId, _transitionId);
        }

        public void Init(ProgressManager progressManager, SoundsManager soundsManager)
        {
            _progressManager = progressManager;
            _soundsManager = soundsManager;
            ExceptionUtilities.ThrowIfNull(_progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Progress manager"));
            ExceptionUtilities.ThrowIfNull(_soundsManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Sounds manager"));
        }

        public void SetTransition(int actionId, int transitionId)
        {
            _actionId = actionId;
            _transitionId = transitionId;
            
            if (!_progressManager.TryGetQuestActionHandler(_questId, _actionId, out _actionHandler))
            {
                Debug.LogWarning(string.Format(LogStr.WARNING_QUEST, $"{gameObject.name} unable to get quest {_questId} action handler {_actionId}"));
                Destroy(this);
            }
        }

        public void Interact()
        {
            _actionHandler.MakeTransition(_transitionId);

            if (_interactSound != null)
            {
                _soundsManager.PlaySound(_interactSound, volume: _interactSoundVolume);
            }

            if (_destroyAfterInteract)
            {
                Destroy(_destroyWithGameObject ? gameObject : this);
            }
        }

        public Vector3 TargetPosition => _targetPosition == null ? transform.position : _targetPosition.position;
    }
}