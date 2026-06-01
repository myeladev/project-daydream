using ProjectDaydream.Objects.Items;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Items/New Item")]
    public class ItemDefinition : ScriptableObject
    {
        public new string name;
        [TextArea]
        public string description;
        public Sprite icon;
        public ItemObject prefab;
        public bool canClean;
        public ItemCategory category;
    }
}