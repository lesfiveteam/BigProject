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
            _slideManager.IntroEnded += OnIntroEnded;
        }

        private void OnIntroEnded()
        {
            _slideManager.IntroEnded -= OnIntroEnded;

            Bootstrapper.SetStage(GameExecutionStage.Gameplay);
            ServiceLocator.GetService<SceneLoadManager>().LoadScene(Scenes.Village);
        }
    }
}

