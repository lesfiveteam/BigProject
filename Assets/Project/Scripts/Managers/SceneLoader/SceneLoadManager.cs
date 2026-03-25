using BigProject.Managers;
using BigProject.Systems;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Project.Scripts.Managers.SceneLoader
{
    public enum Scenes
    {
        MainMenu,
        Intro,
        Village,
        TownHall,
        Watermill
    }

    public class SceneLoadManager : IDisposable
    {
        public event Action<Scenes> SceneLoaded;
        public event Action SceneLoadingStarted;
        public event Action SceneLoadingCompleted;

        private const string FADER_PREFAB_PATH = "Prefabs/Fader";

        private readonly MonoBehaviour _coroutineStarter;
        private readonly Fader _fader;

        private bool _isLoading;

        public SceneLoadManager(MonoBehaviour coroutineStarter)
        {
            _coroutineStarter = coroutineStarter;

            if (_fader == null)
            {
                Fader faderPrefab = Resources.Load<Fader>(FADER_PREFAB_PATH);
                _fader = UnityEngine.Object.Instantiate(faderPrefab);

                UnityEngine.Object.DontDestroyOnLoad(_fader.gameObject);
            }
        }

        public void LoadScene(Scenes scene)
        {
            if (_isLoading)
            {
                return;
            }

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

            // 1. Затемнение
            bool waitFading = true;
            _fader.FadeIn(() => waitFading = false);

            while (waitFading)
            {
                yield return null;
            }

            SceneLoadingStarted?.Invoke();

            // 2. Загрузка сцены
            AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
            async.allowSceneActivation = false;
            async.completed += NotifyLoadingCompleted;

            while (async.progress < 0.9f)
            {
                yield return null;
            }

            async.allowSceneActivation = true;

            // 3. Оповещение
            Debug.Log(string.Format(LogStr.INFO_SCENE_LOADING, scene));
            SceneLoaded?.Invoke(scene);

            // 4. Появление
            waitFading = true;
            _fader.FadeOut(() => waitFading = false);

            while (waitFading)
            {
                yield return null;
            }

            _isLoading = false;
        }

        /// <summary>
        /// Notify when old scene unloaded and new one is loaded.
        /// </summary>
        private void NotifyLoadingCompleted(AsyncOperation loading)
        {
            SceneLoadingCompleted?.Invoke();
            loading.completed -= NotifyLoadingCompleted;
        }
        
        public void Dispose()
        {
            UnityEngine.Object.Destroy(_fader.gameObject);
        }
    }
}