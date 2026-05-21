using ProjectDaydream.DataPersistence;

namespace ProjectDaydream.SaveData
{
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
    [CustomEditor(typeof(SaveAgent))]
    public class SaveAgentEditor : Editor
    {
        SerializedProperty idProp;
        bool IsPrefabAsset(SaveAgent sa) => PrefabUtility.IsPartOfPrefabAsset(sa.gameObject);

        void OnEnable()
        {
            idProp = serializedObject.FindProperty("id");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var sa = (SaveAgent)target;
            bool isPrefabAsset = IsPrefabAsset(sa);
            string id = idProp.stringValue;

            EditorGUILayout.LabelField("Save Agent", EditorStyles.boldLabel);

            // Show GUID (read-only)
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.TextField("ID", string.IsNullOrEmpty(id) ? "(none)" : id);
            }

            // Copy button (only if exists)
            if (!string.IsNullOrEmpty(id))
            {
                if (GUILayout.Button("Copy ID"))
                    EditorGUIUtility.systemCopyBuffer = id;
            }

            // Show "Generate GUID" only when empty AND not a prefab asset
            if (!isPrefabAsset && string.IsNullOrEmpty(id))
            {
                EditorGUILayout.Space(4);
                if (GUILayout.Button("Generate ID"))
                {
                    Undo.RecordObject(sa, "Generate SaveAgent ID");
                    idProp.stringValue = System.Guid.NewGuid().ToString("N");
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(sa);
                }
            }

            // Optional: small help when inspecting a prefab asset
            if (isPrefabAsset)
            {
                EditorGUILayout.HelpBox(
                    "Prefab assets do not store IDs. Place instances in a scene to generate one.",
                    MessageType.Info);
            }

            // Draw default
            //DrawDefaultInspector();
        }
    }
#endif
}