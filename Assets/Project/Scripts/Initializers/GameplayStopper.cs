using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Systems;
using BigProject.Utilities;
using UnityEngine;

namespace BigProject.Initializers
{
    public class GameplayStopper
    {
        private SceneLoadManager _sceneLoader;

        public GameplayStopper(SceneLoadManager sceneLoader)
        {
            _sceneLoader = sceneLoader;
            ExceptionUtilities.ThrowIfNull(_sceneLoader, string.Format(LogStr.CRITICAL_NULL_REFERENCE, "GameplayStopper", "SceneLoadManager"));
        }

        public void Run(Scenes nonGameplayScene)
        {
            _sceneLoader.SceneLoadingStarted += OnNonGameplaySceneLoadingStarted;
            _sceneLoader.LoadScene(nonGameplayScene);
        }

        private void OnNonGameplaySceneLoadingStarted()
        {
            _sceneLoader.SceneLoadingStarted -= OnNonGameplaySceneLoadingStarted;
            GameplaySceneEntryPoint sceneEntryPoint = GameObject.FindFirstObjectByType<GameplaySceneEntryPoint>();

            if (sceneEntryPoint != null)
            {
                GameObject.Destroy(sceneEntryPoint);
            }

            Bootstrapper.SetStage(GameExecutionStage.Launch);
        }
    }
}