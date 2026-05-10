using BigProject.Managers;
using BigProject.Managers.CutsceneManager;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.UI;
using BigProject.Utilities;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
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

        private RunePanelUI _runePanel;
        private IQuestActionHandler _runesAssembleHandler;
        private CutsceneManager _cutsceneManager;

        private const float FRESCO_BLINK_TIME = 2f;

        public void Init(ProgressManager progressManager, RunePanelUI runePanel, CutsceneManager cutsceneManager)
        {
            _runePanel = runePanel;
            _cutsceneManager = cutsceneManager;
            ExceptionUtilities.ThrowIfNull(progressManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "ProgressManager"));
            ExceptionUtilities.ThrowIfNull(_runePanel, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "RunePanelUI"));
            ExceptionUtilities.ThrowIfNull(_cutsceneManager, string.Format(LogStr.CRITICAL_NULL_REFERENCE, gameObject.name, "CutsceneManager"));
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
            yield return new WaitUntil(() => _cutsceneManager.IsPlaying);
        }

        private void OnEnable()
        {
            _runePanel.Completed += OnRunesAssembled;
        }

        private void OnDisable()
        {
            _runePanel.Completed -= OnRunesAssembled;
        }
    }
}