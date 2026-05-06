using UnityEditor;
using UnityEngine;

namespace ProjectDaydream.EditorTools
{
    public class FindMissingScripts : Editor
    {
        [MenuItem("Tools/Find Missing Scripts")]
        static void Find()
        {
            GameObject[] objects = Resources.FindObjectsOfTypeAll<GameObject>();
            int found = 0;

            foreach (var go in objects)
            {
                Component[] components = go.GetComponents<Component>();
                foreach (var c in components)
                {
                    if (c == null)
                    {
                        Debug.LogWarning($"Missing script on: {go.name}", go);
                        found++;
                    }
                }
            }

            Debug.Log($"Search complete. Found {found} missing scripts.");
        }
    }
}