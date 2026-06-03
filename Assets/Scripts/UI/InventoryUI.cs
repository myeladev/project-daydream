using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.Objects.Items;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.UI
{
    /// <summary>
    /// Controls how the player's UI is displayed on screen.
    /// </summary>
    public class InventoryUI : UIPanel
    {
        private ItemObject _hoveredItemObject;
        private InputAction _interactAction;
        private InputAction _dropAction;
        
        private ContainerObject _openContainerObject;
        
        [SerializeField] private ContainerGridUI pocketGrid;
        [SerializeField] private ContainerGridUI backpackGrid;
        [SerializeField] private GameObject backpackText;
        [SerializeField] private TextMeshProUGUI containerNameText;
        [SerializeField] private ContainerGridUI equipmentBackpackGrid;
        [SerializeField] private ContainerGridUI equipmentFlashlightGrid;
        /// <summary>
        /// The grid of the container object that the player is currently interacting with.
        /// </summary>
        [SerializeField] private ContainerGridUI focusContainerGrid;
        [SerializeField] private GameObject focusContainerPanel;

        public static InventoryUI Instance;
        
        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }
        
        private void Start()
        {
            pocketGrid.Init(InventoryController.Instance.Pockets);
            backpackGrid.Init(InventoryController.Instance.backpack?.ContainerGrid);
            equipmentBackpackGrid.Init(InventoryController.Instance.EquippedBackpack);
            equipmentFlashlightGrid.Init(InventoryController.Instance.EquippedFlashlight);
            _interactAction = InputSystem.actions.FindAction("Interact");
            _dropAction = InputSystem.actions.FindAction("Drop");
        }
        
        void Update()
        {
            if (_hoveredItemObject && GameplayUI.Instance.IsPanelActive(this))
            {
                IInteractionProvider interactionProvider = _hoveredItemObject;
                if (_interactAction.WasPressedThisFrame()) interactionProvider.Interact("Use", InteractContext.Inventory);
                if (_dropAction.WasPressedThisFrame()) interactionProvider.Interact("Drop", InteractContext.Inventory);
            }
        }

        private int? _draggedItemIndex;
        private ContainerGrid _draggedItemContainer;

        public void StartDragging(ContainerCellUI cell)
        {
            _draggedItemIndex = cell.index;
            _draggedItemContainer = cell.ContainerGrid;
        }

        public void FinishDragging(ContainerCellUI cell)
        {
            if (!cell) return;
            if (_draggedItemIndex is null || _draggedItemContainer is null) return;

            // Invoke the TrySwapItems method from InventoryController
            _draggedItemContainer.TrySwapItems(_draggedItemIndex.Value, cell.ContainerGrid, cell.index);

            // Refresh the UI after the swap
            Refresh();
            
            // Clear the position of the dragged item
            _draggedItemIndex = null;
            _draggedItemContainer = null;
        }
        public override void OnShow()
        {
            Refresh();
        }
        
        public void Refresh()
        {
            focusContainerPanel.gameObject.SetActive(_openContainerObject);
            if(_openContainerObject) focusContainerGrid.Init(_openContainerObject.ContainerGrid);
            _hoveredItemObject = null;
            
            pocketGrid.Refresh();
            backpackGrid?.Refresh();
            equipmentBackpackGrid?.Refresh();
            equipmentFlashlightGrid?.Refresh();

            backpackGrid?.gameObject.SetActive(InventoryController.Instance.backpack);
            backpackText.SetActive(InventoryController.Instance.backpack);
        }

        protected override void OnHide()
        {
            _openContainerObject = null;
        }
        protected override void OnHidden()
        {
            
        }
        
        public void TryTransferItem(ContainerCellUI sourceCell)
        {
            var item = sourceCell.ContainerGrid.GetItemAt(sourceCell.index);
            if (item == null) return;

            var focusGrid = _openContainerObject?.ContainerGrid;
            var pockets = InventoryController.Instance.Pockets;
            var backpackGrid = InventoryController.Instance.backpack?.ContainerGrid;

            // Build priority-ordered list of transfer targets
            var targets = new List<ContainerGrid>();
            if (sourceCell.ContainerGrid == focusGrid)
            {
                // World container → pockets first, then backpack
                targets.Add(pockets);
                if (backpackGrid != null) targets.Add(backpackGrid);
            }
            else
            {
                // Player inventory → focus container if open, then the other player grid
                if (focusGrid != null) targets.Add(focusGrid);
                if (sourceCell.ContainerGrid == pockets && backpackGrid != null)
                    targets.Add(backpackGrid);
                else if (sourceCell.ContainerGrid == backpackGrid)
                    targets.Add(pockets);
            }

            foreach (var target in targets)
            {
                if (!target.HasAnyAvailableSpace()) continue;

                sourceCell.ContainerGrid.RemoveItem(item);
                if (target.TryAddItem(item, null))
                {
                    Refresh();
                    return;
                }

                // Target was unexpectedly full — restore and try the next one
                sourceCell.ContainerGrid.PlaceItem(item, sourceCell.index);
            }
        }

        public void ShowContainer(ContainerObject containerObject)
        {
            _openContainerObject = containerObject;
            focusContainerGrid.Init(containerObject.ContainerGrid);
            focusContainerPanel.gameObject.SetActive(true);
            containerNameText.text = containerObject.containerName ?? "Container";
        }
    }
}
