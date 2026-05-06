using System;
using System.Collections;
using System.Collections.Generic;
using ProjectDaydream.DataPersistence;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectDaydream.Core
{
    public class SceneManager : MonoBehaviour
    {
        public static SceneManager Instance { get; private set; }
        [SerializeField]
        private List<GameObject> gameplayObjects;
        [SerializeField]
        private List<GameObject> mainMenuObjects;
        
        public CanvasGroup loadingScreen;
        private Scene? _activeWorldScene;
        private const float FadeDuration = 0.5f;

        public bool IsInMainMenu => _gameState == GameState.MainMenu;
        private GameState _gameState = GameState.MainMenu;
        public GameState GameState => _gameState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            LoadMainMenu();
            gameplayObjects.ForEach(m => m.SetActive(false));
        }

        private IEnumerator FadeLoadingScreen(float targetAlpha)
        {
            loadingScreen.gameObject.SetActive(true);
            float startAlpha = loadingScreen.alpha;
            float elapsedTime = 0;

            while (elapsedTime < FadeDuration)
            {
                elapsedTime += Time.deltaTime;
                loadingScreen.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / FadeDuration);
                yield return null;
            }

            loadingScreen.alpha = targetAlpha;
            if (Mathf.Approximately(targetAlpha, 0f))
            {
                loadingScreen.gameObject.SetActive(false);
            }
        }

        public void LoadMainMenu()
        {
            StartCoroutine(_loadMainMenu());
        }
        
        private IEnumerator _loadMainMenu() 
        {
            yield return StartCoroutine(FadeLoadingScreen(1f));
            _gameState = GameState.MainMenu;
            
            if (_activeWorldScene is not null)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_activeWorldScene.Value);
                _activeWorldScene = null;
            }
            
            // Load the new scene asynchronously
            AsyncOperation worldLoadOperation =
                UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("World", LoadSceneMode.Additive);
            if (worldLoadOperation is not null)
            {
                worldLoadOperation.allowSceneActivation = false;
            }
            else
            {
                Debug.LogError($"World scene failed to load.");
                throw new NullReferenceException("loadOperation was null, scene failed to load.");
            }
            
            // Wait until the scene is fully loaded
            while (worldLoadOperation.progress < 0.9f)
            {
                yield return null;
            }

            // Artificial delay
            yield return new WaitForSeconds(1f);
            
            // Activate the new scenes
            worldLoadOperation.completed += (operation) =>
            {
                _activeWorldScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("World");
            };
            
            worldLoadOperation.allowSceneActivation = true;
            gameplayObjects.ForEach(m => m.SetActive(false));
            mainMenuObjects.ForEach(m => m.SetActive(true));
            

            yield return new WaitForSeconds(0.1f);
            
            DataPersistenceManager.Instance.LoadRecentProfile();
            Cursor.lockState = CursorLockMode.None;
            
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(FadeLoadingScreen(0f));
        }
        
        public void LoadScenesForProfile(string profileName)
        {
            StartCoroutine(LoadWorldSceneForSave(profileName));
        }
        
        private IEnumerator LoadWorldSceneForSave(string profileName)
        {
            yield return StartCoroutine(FadeLoadingScreen(1f));
            _gameState = GameState.World;
            // Unload the old scene if it's not the persistent scene
            if (_activeWorldScene is not null)
            {
                UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(_activeWorldScene.Value);
                _activeWorldScene = null;
            }
            
            // Load the new scene asynchronously
            AsyncOperation worldLoadOperation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("World", LoadSceneMode.Additive);
            if (worldLoadOperation is not null)
            {
                worldLoadOperation.allowSceneActivation = false;
            }
            else
            {
                Debug.LogError($"World scene failed to load.");
                throw new NullReferenceException("loadOperation was null, scene failed to load.");
            }
            
            // Wait until the scene is fully loaded
            while (worldLoadOperation.progress < 0.9f)
            {
                yield return null;
            }

            // Artificial delay
            yield return new WaitForSeconds(1f);
            
            gameplayObjects.ForEach(m => m.SetActive(true));
            mainMenuObjects.ForEach(m => m.SetActive(false));
            
            // Activate the new scene
            worldLoadOperation.allowSceneActivation = true;
            
            yield return new WaitForSeconds(0.1f);
            
            _activeWorldScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("World");
            UnityEngine.SceneManagement.SceneManager.SetActiveScene(_activeWorldScene.Value);
            DataPersistenceManager.Instance.LoadProfile(profileName);
            
            yield return new WaitForSeconds(0.1f);
            yield return StartCoroutine(FadeLoadingScreen(0f));
        }
    }

    public enum GameState
    {
        MainMenu,
        World
    }
}
