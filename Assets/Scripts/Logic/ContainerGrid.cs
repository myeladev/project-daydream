using System;
using ProjectDaydream.UI;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class ContainerGrid
    {
        public int Size => grid.Length;
        private ContainerGridItem[] grid;
        private Func<ContainerGridItem, bool> itemConstraint;

        public ContainerGrid(int size, Func<ContainerGridItem, bool> itemConstraint = null)
        {
            grid = new ContainerGridItem[size];
            this.itemConstraint = itemConstraint;
        }

        public bool CanPlaceItem(ContainerGridItem containerGridItem, int index)
        {
            if (itemConstraint is not null)
            {
                if (!itemConstraint(containerGridItem))
                    return false;
            }
            return grid[index] == null;
        }

        public bool PlaceItem(ContainerGridItem containerGridItem, int index)
        {
            if (!CanPlaceItem(containerGridItem, index))
                return false;

            grid[index] = containerGridItem;

            containerGridItem.Index = index;
            return true;
        }

        public ContainerGridItem RemoveItem(ContainerGridItem containerGridItem)
        {
            for (int i = 0; i < Size; i++)
            {
                if (grid[i] == containerGridItem)
                {
                    var returnItem = grid[i];
                    grid[i] = null;
                    return returnItem;
                }
            }

            return null;
        }

        public ContainerGridItem GetItemAt(int index)
        {
            return grid[index];
        }

        public bool HasAnyAvailableSpace()
        {
            for (int i = 0; i < Size; i++)
            {
                if (grid[i] == null)
                    return true;
            }

            return false;
        }
        
        
        public bool TryAddItem(ContainerGridItem item, int? index)
        {
            if (index.HasValue)
            {
                bool added = PlaceItem(item, index.Value);
                if (added)
                {
                    InventoryUI.Instance.Refresh();
                }
                return added;
            }
            
            for (int i = 0; i < Size; i++)
            {
                if (CanPlaceItem(item, i))
                {
                    bool added = PlaceItem(item, i);
                    if (added)
                    {
                        InventoryUI.Instance.Refresh();
                    }
                    return added;
                }
            }

            // No space found
            return false;
        }
        
        public bool TrySwapItems(int sourceIndex, ContainerGrid targetContainer, int targetIndex)
        {
            ContainerGridItem item1 = GetItemAt(sourceIndex);
            ContainerGridItem item2 = targetContainer.GetItemAt(targetIndex);

            if (item1 == null)
            {
                return false;
            }

            // Temporarily remove items from their slots to let the swap happen correctly
            var removedItem1 = RemoveItem(item1);
            var removedItem2 = targetContainer.RemoveItem(item2);
            
            if ((item2 is null || CanPlaceItem(item2, sourceIndex)) &&
                targetContainer.CanPlaceItem(item1, targetIndex))
            {
                if(item2 is not null) PlaceItem(item2, sourceIndex);
                targetContainer.PlaceItem(item1, targetIndex);
                    
                InventoryUI.Instance.Refresh();
                return true;
            }
            else
            {
                PlaceItem(removedItem1, sourceIndex);
                if(item2 is not null) targetContainer.PlaceItem(removedItem2, targetIndex);
            }
                
            return false;
        }
    }
}