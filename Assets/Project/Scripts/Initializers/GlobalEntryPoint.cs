using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using BigProject.Systems.HUD;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;

namespace BigProject.Initializers
{
    /// <summary>
    /// Точка входа для регистрации базовых служб и настроек.
    /// </summary>
    public class GlobalEntryPoint : MonoBehaviour
    {
        [SerializeField]
        private MusicManager _musicManager;
        [SerializeField]
        private GlobalConfig _config;
        [SerializeField, Tooltip("Actions to execute for startup initialize.")]
        private UnityEvent _initActions;
        [SerializeField]
        private LogLevel _currentLogLevel = LogLevel.None;

        private static bool _isInstantiated;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            _isInstantiated = false;
        }

        private void Awake()
        {
            if (_isInstantiated)
            {
                Debug.LogWarning("Global point should exist in one copy.");
                Destroy(gameObject);
                return;
            }
            
            Assert.IsNotNull(_config, "Global entry point config is null.");           
            _isInstantiated = true;

            ManualLoop manualLoop = new GameObject("ManualLoop").AddComponent<ManualLoop>();
            DontDestroyOnLoad(manualLoop);
            ServiceLocator.AddService(manualLoop);
            ServiceLocator.AddService(new GameplayManager(manualLoop));
            ServiceLocator.AddService(new GameLogManagerTicker(manualLoop));
            ServiceLocator.AddService(new SceneLoadManager(manualLoop));
            ServiceLocator.AddService(new ProgressManager(_config.PlayerProfileName, new QuestJsonLoader(_config.QuestsFolder), new SavesManager()));
            ServiceLocator.AddService(new HUD());
            //ServiceLocator.AddServiceResolver(() => InventorySystem.Instance);

            MusicManager musicManager = Instantiate(_musicManager);
            DontDestroyOnLoad(musicManager);
            ServiceLocator.AddService(musicManager);

            GameLogManager.Init(_currentLogLevel);

            _initActions?.Invoke();
        }
    }
}