using ProjectDaydream.SaveData;
using UnityEditor;
using UnityEngine;

namespace ProjectDaydream.DataPersistence
{
    [DisallowMultipleComponent]
    public class SaveAgent : MonoBehaviour
    {
        public string id;

        public void SaveData(ref GameData gameData)
        {
            var saveTargets = GetComponents<IDataPersistence>();
            foreach (var saveTarget in saveTargets)
            {
                saveTarget.SaveData(ref gameData);
            }
        }
        public void LoadData(GameData gameData)
        {
            var saveTargets = GetComponents<IDataPersistence>();
            foreach (var saveTarget in saveTargets)
            {
                saveTarget.LoadData(gameData);
            }
        }
        
#if UNITY_EDITOR
        void OnEnable()
        {
            if (Application.isPlaying) return;
            // Never store GUID on prefab assets
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject)) { id = ""; return; }
            
            // If empty or colliding, generate a new one
            if (string.IsNullOrEmpty(id) || IsDuplicate(id))
            {
                id = System.Guid.NewGuid().ToString("N");
                EditorUtility.SetDirty(this);
            }
            SaveIdRegistry.Register(this, id);
        }
        
        void OnDestroy()
        {
            if (Application.isPlaying) return;
            SaveIdRegistry.Unregister(this, id);
        }
        
        void OnValidate()
        {
            if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
            {
                // Clear guid so prefab assets never store it
                if (!string.IsNullOrEmpty(id))
                {
                    id = "";
                    EditorUtility.SetDirty(this);
                }
            }
            
            // If this object’s guid matches another registered instance, regenerate
            if (!string.IsNullOrEmpty(id) && IsDuplicate(id))
            {
                id = System.Guid.NewGuid().ToString("N");
                EditorUtility.SetDirty(this);
            }

            SaveIdRegistry.Register(this, id);
        }
        
        // Check if another SaveAgent with the same GUID is registered
        bool IsDuplicate(string g)
        {
            return SaveIdRegistry.TryGet(g, out var other) && other && other != this;
        }
#endif
    }
}
