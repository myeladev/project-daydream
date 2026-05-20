using ProjectDaydream.Core;
using ProjectDaydream.Logic;
using ProjectDaydream.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.Objects.Items
{

    public class InventoryController : MonoBehaviour
    {
        public static InventoryController Instance;
        
        private InputAction _inventoryAction;
        private InputAction _cycleHotbarAction;
        [HideInInspector]
        public int selectedPocketIndex = 0;
        [SerializeField] private UIPanel inventoryMenu;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
            _inventoryAction = InputSystem.actions.FindAction("Inventory");
            _cycleHotbarAction = InputSystem.actions.FindAction("CycleHotbar");
            Pockets = new ContainerGrid(8, 1);
        }
        
        private void Update()
        {
            if (_inventoryAction.WasPressedThisFrame() && !SceneManager.Instance.IsInMainMenu)
            {
                if (GameplayUI.Instance.IsPanelActive(inventoryMenu))
                {
                    GameplayUI.Instance.PopPanel();
                }
                else
                {
                    GameplayUI.Instance.PushPanel(inventoryMenu);
                }
            }
            
            var cycleHotbar = _cycleHotbarAction.ReadValue<Vector2>();
            if (cycleHotbar.y != 0)
            {
                Debug.Log(cycleHotbar);
                selectedPocketIndex = (selectedPocketIndex - (int)cycleHotbar.y + Pockets.Size) % Pockets.Size;
            }
        }
        
        public ContainerGrid Pockets;
        public ContainerObject backpack;

        public bool TryAddItem(ContainerGridItem item, ContainerGrid selectedContainerGrid = null, int? x = null, int? y = null)
        {
            if (selectedContainerGrid is not null && x.HasValue && y.HasValue)
            {
                bool added = selectedContainerGrid.PlaceItem(item, x.Value, y.Value);
                if (added)
                {
                    InventoryUI.Instance.Refresh();
                }
                return added;
            }
            
            ContainerGrid selectedContainer = Pockets;

            if (!Pockets.HasAnyAvailableSpace())
            {
                if (backpack is not null)
                {
                    selectedContainer = backpack.ContainerGrid;
                }
                else
                {
                    return false;
                }
            }
            
            // Try to find the first available space
            for (int row = 0; row <= selectedContainer.Width; row++)
            {
                for (int col = 0; col <= selectedContainer.Height; col++)
                {
                    if (selectedContainer.CanPlaceItem(row, col))
                    {
                        bool added = selectedContainer.PlaceItem(item, row, col);
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
    }
}