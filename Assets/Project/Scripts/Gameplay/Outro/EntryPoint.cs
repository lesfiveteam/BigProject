using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
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

            _slideManager.OutroEnded += OnOpeningEnded;
            _slideManager.StartSlideShow(OutroVariant.First); // !Hardcode! Need to set valid outro enum from ServiceLocator
        }

        private void OnOpeningEnded()
        {
            _slideManager.OutroEnded -= OnOpeningEnded;

            Bootstrapper.SetStage(GameExecutionStage.Launch);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.MainMenu);
        }
    }
}

