using System.Collections.Generic;
using ProjectDaydream.Logic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ProjectDaydream.UI
{
    public class ContainerCellUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public ContainerGrid ContainerGrid;
        public Image iconImage;
        [HideInInspector] public int x, y;
        public Vector2Int Position => new (x, y);

        public void Init(ContainerGrid containerGrid, int x, int y)
        {
            this.ContainerGrid = containerGrid;
            this.x = x;
            this.y = y;
        }

        public void UpdateVisual(Item item)
        {
            if (item && item.icon)
            {
                iconImage.enabled = true;
                iconImage.sprite = item.icon;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        public void Refresh()
        {
            GetComponent<Image>().raycastTarget = true;
        }
        
        public void Clear()
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }
        
        private Vector3 startPosition;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            GetComponent<Image>().raycastTarget = false;
            // Save the starting position and parent transform of the dragged object
            startPosition = iconImage.transform.position;
            
            // Make the object being dragged slightly transparent
            iconImage.color = new Color(1f, 1f, 1f, 0.7f);

            InventoryUI.Instance.StartDragging(this);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // Move the object being dragged to the pointer's position
            iconImage.transform.position = eventData.position;
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            iconImage.transform.position = startPosition;

            // Return the object to full opacity
            iconImage.color = new Color(1f, 1f, 1f, 1f);
            
            
            // Raycast to UI elements underneath the cursor
            var pointer = new PointerEventData(EventSystem.current)
            {
                position = Mouse.current.position.ReadValue()
            };

            var raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointer, raycastResults);

            ContainerCellUI otherCell = null;

            // Iterate through all hit objects
            foreach (var result in raycastResults)
            {
                // Retrieve the InventoryCellUI that is not the current one
                var hitCell = result.gameObject.GetComponent<ContainerCellUI>();

                // If the component is found, and it's not the current one
                if (hitCell != null && hitCell != this)
                {
                    otherCell = hitCell;
                    break;
                }
            }

            // Now 'otherCell' is the first 'InventoryCellUI' underneath the cursor different from 'this',
            // or null if no such object was found

            InventoryUI.Instance.FinishDragging(otherCell);
        }
    }
}