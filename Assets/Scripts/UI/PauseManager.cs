using ProjectDaydream.Core;
using ProjectDaydream.Logic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.UI
{
    public class PauseManager : MonoBehaviour
    {
        public static PauseManager Instance;
        
        [HideInInspector] public bool isPaused;
        [SerializeField] private UIPanel pauseMenu;

        private InputAction _pauseAction;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            _pauseAction = InputSystem.actions.FindAction("Exit");
        }

        private void Update()
        {
            if (_pauseAction.WasPressedThisFrame() && !SceneManager.Instance.IsInMainMenu && !PlayerController.Instance.IsDriving)
            {
                if (GameplayUI.Instance.IsAnyPanelActive())
                {
                    GameplayUI.Instance.PopPanel();
                }
                else
                {
                    GameplayUI.Instance.PushPanel(pauseMenu);
                }
            }
        }

        public void Pause()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void Unpause()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
    }
}