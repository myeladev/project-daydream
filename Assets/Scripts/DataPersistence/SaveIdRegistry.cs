using System.Collections.Generic;
using ProjectDaydream.DataPersistence;

namespace ProjectDaydream.SaveData
{
#if UNITY_EDITOR
    static class SaveIdRegistry
    {
        private static readonly Dictionary<string, SaveAgent> Map = new();

        public static bool TryGet(string guid, out SaveAgent agent) => Map.TryGetValue(guid, out agent);

        public static void Register(SaveAgent a, string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            Map[guid] = a;
        }

        public static void Unregister(SaveAgent a, string guid)
        {
            if (string.IsNullOrEmpty(guid)) return;
            if (Map.TryGetValue(guid, out var current) && current == a)
                Map.Remove(guid);
        }
    }
#endif

}
