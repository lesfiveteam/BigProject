using BigProject.Intercatable.HighlightedObjects;
using BigProject.Managers;
using BigProject.Managers.CursorManager;
using BigProject.Managers.CutsceneManager;
using BigProject.NPC;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace BigProject.Gameplay.Final
{
    public class QuestActions : MonoBehaviour
    {
        [SerializeField]
        private int _questId;
        [SerializeField]
        private int _runesAssembleActionId;
        [SerializeField]
        private Renderer _frescoRenderer;
        [SerializeField]
        private AssetReferenceT<TimelineAsset> _ñutscene;
        [SerializeField]
        private DialogNPC _cutsceneDialogue;
        [SerializeField]
        private Transform _priest;
        [SerializeField]
        private Transform _priestFinalPosition;
        [SerializeField]
        private Transform _elderFinalPosition;
        [SerializeField]
        private Transform _blacksmithFinalPosition;

        private RunePanelUI _runePanel;
        private IQuestActionHandler _runesAssembleHandler;
        private CutsceneManager _cutsceneManager;
        private GameplayManager _gameplayManager;
        private CursorManager _cursorManager;
        private SkinnedMeshRenderer _playerRenderer;

        private const float FRESCO_BLINK_TIME = 2f;
       // private const float 

        private void Awake()
        {
            Assert.IsNotNull(_frescoRenderer, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Fresco Renderer"));
            Assert.IsNotNull(_ñutscene, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Cutscene"));
            Assert.IsNotNull(_cutsceneDialogue, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Cutscene Dialogue"));
            Assert.IsNotNull(_priest, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Priest"));
            Assert.IsNotNull(_priestFinalPosition, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Priest Final Position"));
            Assert.IsNotNull(_elderFinalPosition, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Elder Final Position"));
            Assert.IsNotNull(_blacksmithFinalPosition, string.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, gameObject.name, "Blacksmith Final Position"));
        }

        public void Init(ProgressManager progressManager, RunePanelUI runePanel, CutsceneManager cutsceneManager, 
            GameplayManager gameplayManager, CursorManager cursorManager, SkinnedMeshRenderer playerRenderer)
        {
            _runePanel = runePanel;
            _cutsceneManager = cutsceneManager;
            _gameplayManager = gameplayManager;
            _cursorManager = cursorManager;
            _playerRenderer = playerRenderer;
            ExceptionUtilities.ThrowIfNull(progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "ProgressManager"));
            ExceptionUtilities.ThrowIfNull(_runePanel, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "RunePanelUI"));
            ExceptionUtilities.ThrowIfNull(_cutsceneManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "CutsceneManager"));
            ExceptionUtilities.ThrowIfNull(_gameplayManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "GameplayManager"));
            ExceptionUtilities.ThrowIfNull(_cursorManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "CursorManager"));
            ExceptionUtilities.ThrowIfNull(_playerRenderer, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "Player SkinnedMeshRenderer"));
            progressManager.TryGetQuestActionHandler(_questId, _runesAssembleActionId, out _runesAssembleHandler);
        }

        public void OnRunesAssembled()
        {
            if (_runePanel.IsCompleted)
            {
                _runesAssembleHandler.MakeTransition(0);
            }
        }

        public void PlayFinalCutscene()
        {
            StartCoroutine(FinalRoutine());
        }

        public void FrescoBlink()
        {
            StartCoroutine(FrescoBlinkRoutine());
        }

        private IEnumerator FrescoBlinkRoutine()
        {
            MaterialPropertyBlock propBlock = new();
            _frescoRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_StartTime", Time.time);
            propBlock.SetFloat("_GlintFactor", 0.5f);
            _frescoRenderer.SetPropertyBlock(propBlock);
            yield return new WaitForSeconds(FRESCO_BLINK_TIME);
            propBlock.SetFloat("_GlintFactor", 0.0f);
            _frescoRenderer.SetPropertyBlock(propBlock);
        }

        private IEnumerator FinalRoutine()
        {
            _cutsceneManager.Play(_ñutscene, GameplayState.Cutscene, GameplayState.Cutscene, false);
            yield return new WaitUntil(() => _cutsceneManager.IsReadyToPlay);
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime * 0.8f);
            _playerRenderer.enabled = false;
            yield return new WaitUntil(() => !_cutsceneManager.IsPlaying);

            bool isGetActorsSuccess = _cutsceneManager.TryGetActor("Elder", out CutsceneActor elder) &
                _cutsceneManager.TryGetActor("Blacksmith", out CutsceneActor blacksmith);

            if (isGetActorsSuccess)
            {
                SetActor(_priest, _priestFinalPosition, true, false);
                SetActor(elder.transform, _elderFinalPosition, true, true);
                SetActor(blacksmith.transform, _blacksmithFinalPosition, true, true);
            }
            else
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, gameObject.name, "unable to get cutscene actors"));
            }

            _cutsceneDialogue.Interact();
            yield return new WaitUntil(() => _gameplayManager.State == GameplayState.Play);
            _cutsceneManager.DeactivatePrefabs();
            yield return new WaitForFixedUpdate();
            yield return new WaitForSeconds(GameplayUtilities.CurrentCameraTransitionTime * 0.2f);
            _playerRenderer.enabled = true;
        }

        private void SetActor(Transform actor, Transform target, bool hasTalk, bool talkActive = false)
        {
            actor.transform.position = target.position;
            actor.transform.rotation = target.rotation;

            if (hasTalk && actor.TryGetComponent(out Collider collider))
            {
                collider.enabled = talkActive;

                if (talkActive && actor.TryGetComponent(out CursorChangingEffect cursorEffect))
                {
                    cursorEffect.Init(_cursorManager);
                }
            }
            else
            {
                Debug.LogError(string.Format(LogStr.ERROR_SYSTEM, gameObject.name, "unable to get Elder collider"));
            }
        }

        private void OnEnable()
        {
            _runePanel.Completed += OnRunesAssembled;
        }

        private void OnDisable()
        {
            _runePanel.Completed -= OnRunesAssembled;

            if (_playerRenderer != null && !_playerRenderer.enabled)
            {
                _playerRenderer.enabled = true;
            }
        }
    }
}