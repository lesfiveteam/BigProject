using Assets.Project.Scripts.Managers.SceneLoader;
using Assets.Project.Scripts.Managers.SlideManager;
using BigProject.Initializers;
using BigProject.Managers;
using UnityEngine;

namespace Assets.Project.Scripts.Gameplay.Intro
{
    public class EntryPoint : MonoBehaviour
    {
        [SerializeField] private SlideManager _slideManager;

        private void Start()
        {
            _slideManager.StartSlideShow();
            _slideManager.OpeningEnded += OnOpeningEnded;
        }

        private void OnOpeningEnded()
        {
            _slideManager.OpeningEnded -= OnOpeningEnded;

            Bootstrapper.SetStage(GameExecutionStage.Gameplay);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.Village);
        }
    }
}

