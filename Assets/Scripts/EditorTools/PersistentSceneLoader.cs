// Scripts/Editor/BootstrapSceneLoader.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace EditorTools
{
    [InitializeOnLoad]
    public static class PersistentSceneLoader
    {
        private const string PersistentScene = "Assets/Scenes/PersistentScene.unity";
        private const string PrefKey = "PreviousScene";

        private static readonly string[] scenesToLoadWithPersistentScene = new string[]
        {
            "World", "MainMenu", "LoadingScreen"
        };

        static PersistentSceneLoader()
        {
           EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                var activeScene = SceneManager.GetActiveScene().name;
                if (scenesToLoadWithPersistentScene.All(m => m != activeScene)) return;
                var saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

                if (!saved)
                {
                    // User cancelled — abort play mode
                    EditorApplication.isPlaying = false;
                    return;
                }

                // Save current scene so we can return to it
                SessionState.SetString(PrefKey, SceneManager.GetActiveScene().path);

                // Redirect to bootstrap
                if (SceneManager.GetActiveScene().path != PersistentScene)
                {
                    EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                    EditorSceneManager.OpenScene(PersistentScene);
                }
            }

            if (state == PlayModeStateChange.EnteredEditMode)
            {
                // Return to the scene you were working in
                string previousScene = SessionState.GetString(PrefKey, string.Empty);
                if (!string.IsNullOrEmpty(previousScene))
                    EditorSceneManager.OpenScene(previousScene);
            }
        }
    }
}
#endif