using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace Assets.Project.Scripts.Gameplay.Outro
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private OutroSlideManager _slideManager;

        private void Start()
        {
            ExceptionUtilities.ThrowIfNullFormat(_slideManager);

            ServiceLocator.GetService<ManualLoop>().AddTickable(_slideManager);
            _slideManager.SlideShowEnded += OnOpeningEnded;

            _slideManager.Init(ServiceLocator.GetService<MusicManager>());
            _slideManager.StartSlideShow(OutroSlideManager.OutroVariant.First); // !Hardcode! Need to set valid outro enum from ServiceLocator
        }

        private void OnOpeningEnded()
        {
            _slideManager.SlideShowEnded -= OnOpeningEnded;

            Bootstrapper.SetStage(GameExecutionStage.Launch);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.MainMenu);
        }
    }
}

