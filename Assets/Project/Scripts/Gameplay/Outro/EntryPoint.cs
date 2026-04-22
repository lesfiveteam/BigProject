using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
using UnityEngine;

namespace Assets.Project.Scripts.Gameplay.Outro
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private OutroSlideManager _slideManager;

        private void Start()
        {
            _slideManager.StartSlideShow(Managers.SlideManager.Outro.First); // !Hardcode! Need to set valid outro enum from ServiceLocator
            _slideManager.OutroEnded += OnOpeningEnded;
        }

        private void OnOpeningEnded()
        {
            _slideManager.OutroEnded -= OnOpeningEnded;

            Bootstrapper.SetStage(GameExecutionStage.Launch);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.MainMenu);
        }
    }
}

