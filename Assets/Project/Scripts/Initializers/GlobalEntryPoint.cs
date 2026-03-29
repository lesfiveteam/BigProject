using Assets.Project.Scripts.Managers.SceneLoader;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Managers;
using BigProject.Settings;
using BigProject.Systems;
using BigProject.Systems.QuestSystem;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace BigProject.Initializers
{
    /// <summary>
    /// Global services and settings.
    /// </summary>
    public class GlobalEntryPoint : MonoBehaviour
    {
        [SerializeField]
        private GlobalConfig _config;
        [SerializeField]
        private MusicManager _musicManagerPrefab;
        [SerializeField]
        private SoundsManager _soundsManagerPrefab;
        [SerializeField]
        private LogLevel _currentLogLevel = LogLevel.None;

        private static bool _isInstantiated;

        public static void Init()
        {
            _isInstantiated = false;
        }

        private void Awake()
        {
            if (_isInstantiated)
            {
                Debug.LogWarning(String.Format(LogStr.WARNING_DUPLICATE_UNIQUE_ENTITY, "Global Entry Point"));
                Destroy(gameObject);
                return;
            }
            
            Assert.IsNotNull(_config, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Global Entry Point", "Global Config"));
            Assert.IsNotNull(_musicManagerPrefab, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Global Entry Point", "Music Manager Prefab"));
            Assert.IsNotNull(_soundsManagerPrefab, String.Format(LogStr.CRITICAL_NOT_SERIALIZED_FIELD, "Global Entry Point", "Sounds Manager Prefab"));
            _isInstantiated = true;

            GameObject globalServices = new GameObject("GlobalServices");
            DontDestroyOnLoad(globalServices);

            ManualLoop manualLoop = new GameObject("ManualLoop").AddComponent<ManualLoop>();
            manualLoop.transform.parent = globalServices.transform;
            ServiceLocator.AddService(manualLoop);

            ServiceLocator.AddService(new GameLogManagerTicker(manualLoop));
            GameLogManager.Init(_currentLogLevel);

            ServiceLocator.AddService(new SceneLoadManager(manualLoop));

            SavesManager savesManager = new();
            ServiceLocator.AddService(savesManager);
            ServiceLocator.AddService(new ProgressManager(_config.PlayerProfileName, new QuestJsonLoader(_config.QuestsFolder), savesManager));

            MusicManager musicManager = Instantiate(_musicManagerPrefab);
            musicManager.transform.parent = globalServices.transform;
            ServiceLocator.AddService(musicManager);

            SoundsManager soundsManager = Instantiate(_soundsManagerPrefab);
            soundsManager.transform.parent = globalServices.transform;
            ServiceLocator.AddService(soundsManager);
        }
    }
}