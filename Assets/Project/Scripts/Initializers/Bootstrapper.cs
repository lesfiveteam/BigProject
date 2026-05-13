using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Utilities;
using System;
using UnityEngine;

namespace BigProject.Initializers
{
    /// <summary>
    /// Stages for loading dependencies.
    /// </summary>
    public enum GameExecutionStage
    {
        Launch,
        Gameplay,
    }

    /// <summary>
    /// Game dependencies loading stages controller.
    /// </summary>
    public static class Bootstrapper
    {
        public static event Action<GameExecutionStage> OnStageChanged;

        private const string INITIALIZERS_DIR = "Prefabs/Initializers/";
        private const string GLOBAL_EP_PREFAB_NAME = "GlobalEntryPoint";
        private const string GAMEPLAY_EP_PREFAB_NAME = "GameplayEntryPoint";

        // Hashing gameplay dependencies for removal.
        private static GameplayEntryPoint _gameplayEntryPoint;

        public static GameExecutionStage Stage { get; private set; }

        [RuntimeInitializeOnLoadMethodAttribute(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            // Reset in case of uncleaned domain.
            ServiceLocator.Init();
            GlobalEntryPoint.Init();

            Stage = GameExecutionStage.Launch;
            MoveToStage(GameExecutionStage.Launch);
        }

        public static void SetStage(GameExecutionStage stage)
        {
            if (Stage == stage)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_GAME_EXECUTION_REWRITE_STAGE, Stage));
                return;
            }

            MoveToStage(stage);
        }

        private static void MoveToStage(GameExecutionStage stage)
        {
            OnStageChanged?.Invoke(stage);

            switch (stage)
            {
                case GameExecutionStage.Launch:
                    if (Stage == GameExecutionStage.Gameplay)
                    {
                        Debug.Log(LogStr.INFO_REMOVING_GAMEPLAY_SERVICES);
                        Remover.SafeRelease(_gameplayEntryPoint);

                        // Temporary solution.
                        ServiceLocator.GetService<ProgressManager>().Dispose();
                        ServiceLocator.ReleaseService<ProgressManager>();
                        ServiceLocator.AddService(new ProgressManager("Player", new QuestJsonLoader("Data/Quests"), ServiceLocator.GetService<SavesManager>()));
                    }
                    else
                    {
                        GameObject.Instantiate(Resources.Load<GlobalEntryPoint>($"{INITIALIZERS_DIR}{GLOBAL_EP_PREFAB_NAME}"));
                    }

                    break;
                case GameExecutionStage.Gameplay:
                    GameplayEntryPoint.Init();
                    _gameplayEntryPoint =  GameObject.Instantiate(Resources.Load<GameplayEntryPoint>($"{INITIALIZERS_DIR}{GAMEPLAY_EP_PREFAB_NAME}"));
                    break;
                default:
                    Debug.LogWarning(String.Format(LogStr.WARNING_GAME_EXECUTION_INCORRECT_STAGE, stage));
                    return;
            }

            Stage = stage;
            Debug.Log(string.Format(LogStr.INFO_GAME_EXECUTION_MOVE, Stage));
        }
    }
}