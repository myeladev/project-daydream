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
            pocketGrid.Init(InventoryController.Instance.Pockets);
            backpackGrid.Init(InventoryController.Instance.backpack?.ContainerGrid);
        }
        
        private void Start()
        {
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

        private Vector2Int? _draggedItemPosition;

        public void StartDragging(ContainerCellUI cell)
        {
            _draggedItemPosition = new Vector2Int(cell.x, cell.y);
            Debug.Log("Start", cell);
        }

        public void FinishDragging(ContainerCellUI cell)
        {
            if (!cell) return;
            Debug.Log("Finish", cell);
            if (_draggedItemPosition is null) return;
            
            // Invoke the TrySwapItems method from InventoryController
            cell.ContainerGrid.TrySwapItems(_draggedItemPosition.Value, cell.ContainerGrid, cell.Position);

            // Refresh the UI after the swap
            Refresh();
            
            // Clear the position of the dragged item
            _draggedItemPosition = null;
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
        }

        protected override void OnHide()
        {
            _openContainerObject = null;
        }
        protected override void OnHidden()
        {
            
        }
        
        public void ShowContainer(ContainerObject containerObject)
        {
            _openContainerObject = containerObject;
            focusContainerGrid.Init(containerObject.ContainerGrid);
            focusContainerPanel.gameObject.SetActive(true);
        }
    }
}
