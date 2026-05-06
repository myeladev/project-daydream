// Scripts/Editor/BootstrapSceneLoader.cs
#if UNITY_EDITOR
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

        static PersistentSceneLoader()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                bool saved = EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();

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