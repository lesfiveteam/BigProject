using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.Gameplay.Outro
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private OutroSlideManager _slideManager;
        [SerializeField] private int _finalQuestId = 4;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_slideManager);

            ServiceLocator.GetService<ManualLoop>().AddTickable(_slideManager);
            _slideManager.SlideShowEnded += OnOpeningEnded;

            _slideManager.Init(ServiceLocator.GetService<MusicManager>());

            ProgressManager progressManager = ServiceLocator.GetService<ProgressManager>();
            _slideManager.StartSlideShow(progressManager.GetQuestState(_finalQuestId) == QuestState.Completed ? 
                OutroSlideManager.OutroVariant.First : OutroSlideManager.OutroVariant.Second);

        }

        private void OnOpeningEnded()
        {
            _slideManager.SlideShowEnded -= OnOpeningEnded;

            Bootstrapper.SetStage(GameExecutionStage.Launch);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.MainMenu);
        }

        private void OnDestroy()
        {
            ServiceLocator.GetService<ManualLoop>().RemoveTickable(_slideManager);
        }
    }
}

