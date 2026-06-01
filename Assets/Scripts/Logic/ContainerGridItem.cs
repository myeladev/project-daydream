using System;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class ContainerGridItem
    {
        public ItemDefinition ItemDefinition;
        public Sprite Icon => ItemDefinition.icon;
        public string ItemId => itemId;
        private string itemId;
        
        // Reference to where it's placed
        public int Index;

        public ContainerGridItem(ItemDefinition itemDefinition, string itemId)
        {
            ItemDefinition = itemDefinition;
            this.itemId = itemId;
        }
    }
}