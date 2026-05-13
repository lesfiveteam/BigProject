using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.Gameplay.Intro
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private IntroSlideManager _slideManager;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_slideManager);

            ServiceLocator.GetService<ManualLoop>().AddTickable(_slideManager);
            _slideManager.SlideShowEnded += OnIntroEnded;

            _slideManager.Init(ServiceLocator.GetService<MusicManager>());
            _slideManager.StartSlideShow();
        }

        private void OnIntroEnded()
        {
            _slideManager.SlideShowEnded -= OnIntroEnded;

            Bootstrapper.SetStage(GameExecutionStage.Gameplay);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.Village);
        }
    }
}

