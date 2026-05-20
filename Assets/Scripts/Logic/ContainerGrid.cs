using ProjectDaydream.UI;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class ContainerGrid
    {
        public int Width;
        public int Height;
        public int Size => grid.Length;
        private ContainerGridItem[,] grid;

        public ContainerGrid(int width, int height)
        {
            Width = width;
            Height = height;
            grid = new ContainerGridItem[width, height];
        }

        public bool CanPlaceItem(int x, int y)
        {
            return grid[x, y] == null;
        }

        public bool PlaceItem(ContainerGridItem containerGridItem, int x, int y)
        {
            if (!CanPlaceItem(x, y))
                return false;

            grid[x, y] = containerGridItem;

            containerGridItem.Position = new Vector2Int(x, y);
            return true;
        }

        public ContainerGridItem RemoveItem(ContainerGridItem containerGridItem)
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    if (grid[i, j] == containerGridItem)
                    {
                        var returnItem = grid[i, j];
                        grid[i, j] = null;
                        return returnItem;
                    }
                }
            }

            return null;
        }

        public ContainerGridItem GetItemAt(int x, int y)
        {
            return grid[x, y];
        }

        public bool HasAnyAvailableSpace()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (grid[x, y] == null)
                        return true;
                }
            }

            return false;
        }
        
        
        public bool TryAddItem(ContainerGridItem item, int? x = null, int? y = null)
        {
            if (x.HasValue && y.HasValue)
            {
                bool added = PlaceItem(item, x.Value, y.Value);
                if (added)
                {
                    InventoryUI.Instance.Refresh();
                }
                return added;
            }
            
            // Try to find the first available space
            for (int row = 0; row <= Width; row++)
            {
                for (int col = 0; col <= Height; col++)
                {
                    if (CanPlaceItem(row, col))
                    {
                        bool added = PlaceItem(item, row, col);
                        if (added)
                        {
                            InventoryUI.Instance.Refresh();
                        }
                        return added;
                    }
                }
            }

            // No space found
            return false;
        }
        
        public bool TrySwapItems(Vector2Int sourcePosition, ContainerGrid targetContainer, Vector2Int targetPosition)
        {
            ContainerGridItem item1 = GetItemAt(sourcePosition.x, sourcePosition.y);
            ContainerGridItem item2 = targetContainer.GetItemAt(targetPosition.x, targetPosition.y);

            if (item1 == null)
            {
                return false;
            }

            // Temporarily remove items from their slots to let the swap happen correctly
            var removedItem1 = RemoveItem(item1);
            var removedItem2 = targetContainer.RemoveItem(item2);
            
            if ((item2 is null || CanPlaceItem(sourcePosition.x, sourcePosition.y)) &&
                targetContainer.CanPlaceItem(targetPosition.x, targetPosition.y))
            {
                if(item2 is not null) PlaceItem(item2, sourcePosition.x, sourcePosition.y);
                targetContainer.PlaceItem(item1, targetPosition.x, targetPosition.y);
                    
                InventoryUI.Instance.Refresh();
                return true;
            }
            else
            {
                targetContainer.PlaceItem(removedItem1, sourcePosition.x, sourcePosition.y);
                if(item2 is not null) PlaceItem(removedItem2, targetPosition.x, targetPosition.y);
            }
                
            return false;
        }
    }
}