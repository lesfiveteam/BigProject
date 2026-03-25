using Assets.Project.Scripts.Managers.SceneLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace BigProject.Managers
{
    public class SceneLoadManager_test : MonoBehaviour
    {
        //private const Scenes Scene1 = Scenes.SceneLoaderManager_test_1;
        //private const Scenes Scene2 = Scenes.SceneLoaderManager_test_2;

        private SceneLoadManager _sceneLoaderManager;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            _sceneLoaderManager = ServiceLocator.GetService<SceneLoadManager>();
        }

        void Update()
        {
            SwitchScene();
        }

        private void SwitchScene()
        {
            if (!Keyboard.current.lKey.wasPressedThisFrame)
                return;

            //Scenes nextScene = SceneManager.GetActiveScene().name == Scene1.ToString() ? Scene2 : Scene1;
            //_sceneLoaderManager.LoadScene(nextScene);
        }
    }
}