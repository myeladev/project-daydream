using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class ContainerGridItem
    {
        public ItemDefinition ItemDefinition;
        public Sprite Icon => ItemDefinition.icon;

        // Reference to where it's placed
        public Vector2Int Position;

        public ContainerGridItem(ItemDefinition itemDefinition)
        {
            ItemDefinition = itemDefinition;
        }
    }
}