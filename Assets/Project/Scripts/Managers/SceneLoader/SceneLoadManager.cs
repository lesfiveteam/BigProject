using BigProject.Managers;
using BigProject.Managers.SoundsMusicManagers;
using BigProject.Systems;
using BigProject.Utilities;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Project.Scripts.Managers.SceneLoader
{
    public enum Scenes
    {
        MainMenu = 100,
        Intro = 200,
        Village = 300,
        TownHall = 400,
        Watermill = 500,
        Church = 600,
        Outro = 1000,
    }

    public class SceneLoadManager : IDisposable
    {
        private const float PRELOADER_MIN_DURATION = 2f;
        private const string FADER_PREFAB_PATH = "Prefabs/Fader";

        public event Action<Scenes> SceneLoaded;
        public event Action SceneLoadingStarted;
        public event Action SceneLoadingCompleted;
        
        private readonly MonoBehaviour _coroutineStarter;
        private readonly Fader _fader;
        private readonly Preloader _preloader;
        private SoundsManager _soundsManager;

        private bool _isLoading;
        private bool _isInited = false;
        private string _currentSceneName;

        public SceneLoadManager(MonoBehaviour coroutineStarter)
        {
            _coroutineStarter = coroutineStarter;

            if (_fader == null)
            {
                Fader faderPrefab = Resources.Load<Fader>(FADER_PREFAB_PATH);
                _fader = UnityEngine.Object.Instantiate(faderPrefab);
                _preloader = _fader.GetComponent<Preloader>();

                UnityEngine.Object.DontDestroyOnLoad(_fader.gameObject);
            }

            ExceptionUtilities.ThrowIfNullFormat(_fader);
            ExceptionUtilities.ThrowIfNullFormat(_preloader);

            Init();
        }

        private void Init()
        {
            _soundsManager = ServiceLocator.GetService<SoundsManager>();

            if (_soundsManager != null)
                _isInited = true;
        }

        public bool IsLoading => _isLoading;

        public void LoadScene(Scenes scene)
        {
            if (_isLoading)
                return;

            if (!_isInited)
                Init();

            string currentSceneName = SceneManager.GetActiveScene().name;
            string newSceneName = scene.ToString();

            if (IsSceneInBuild(newSceneName))
            {
                GameLogManager.Warning(string.Format(LogStr.WARNING_SCENE_NOT_FOUND, newSceneName));
            }

            if (currentSceneName == newSceneName)
            {
                GameLogManager.Warning(LogStr.WARNING_SAME_SCENE);
            }

            _coroutineStarter.StartCoroutine(LoadSceneRoutine(scene, newSceneName));
        }

        private bool IsSceneInBuild(string sceneName)
        {
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string name = System.IO.Path.GetFileNameWithoutExtension(scenePath);

                if (name == sceneName)
                    return true;
            }

            return false;
        }

        private IEnumerator LoadSceneRoutine(Scenes scene, string sceneName)
        {
            
            _isLoading = true;
            _currentSceneName = sceneName;

            // Fade in
            bool waitFading = true;
            _fader.FadeIn(() => waitFading = false);

            while (waitFading)
                yield return null;

            _soundsManager.StopAllSounds();

            // Show preloader
            bool waitPreloader = true;
            _preloader.Play(PRELOADER_MIN_DURATION, () => waitPreloader = false);

            SceneLoadingStarted?.Invoke();

            // Loading scene
            AsyncOperation async = SceneManager.LoadSceneAsync(_currentSceneName);

            SceneManager.sceneLoaded += NotifyLoadingCompleted;

            // Waiting for fully scene load & preloader min duration
            yield return new WaitUntil(() => SceneManager.GetActiveScene().name == _currentSceneName & !waitPreloader);

            // Notification
            Debug.Log(string.Format(LogStr.INFO_SCENE_LOADING, scene));
            SceneLoaded?.Invoke(scene);

            // Fade out
            waitFading = true;
            _fader.FadeOut(() => waitFading = false);

            while (waitFading)
                yield return null;

            _isLoading = false;
        }

        /// <summary>
        /// Notify when old scene unloaded and new one is loaded.
        /// </summary>
        private void NotifyLoadingCompleted(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == _currentSceneName)
            {
                SceneLoadingCompleted?.Invoke();
                SceneManager.sceneLoaded -= NotifyLoadingCompleted;
            }
        }
        
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_fader.gameObject);
        }
    }
}