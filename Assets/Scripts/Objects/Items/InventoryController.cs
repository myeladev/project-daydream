using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.Common;
using ProjectDaydream.Core;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.Logic;
using ProjectDaydream.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.Objects.Items
{
    public class InventoryController : MonoBehaviour, IDataPersistence
    {
        public static InventoryController Instance;
        
        private InputAction _inventoryAction;
        private InputAction _cycleHotbarAction;
        [HideInInspector]
        public int selectedPocketIndex = 0;
        [SerializeField] private UIPanel inventoryMenu;

        public ContainerGrid Pockets;
        public ContainerObject backpack;
        public ContainerGrid EquippedBackpack { get; set; }
        public ContainerGrid EquippedFlashlight { get; set; }

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
            Pockets = new ContainerGrid(8);
            EquippedBackpack = new ContainerGrid(1, item => item.ItemDefinition.category.HasFlag(ItemCategory.Backpack));
            EquippedFlashlight = new ContainerGrid(1, item => item.ItemDefinition.category.HasFlag(ItemCategory.Flashlight));
        }

        private void Start()
        {
            _inventoryAction = InputSystem.actions.FindAction("Inventory");
            _cycleHotbarAction = InputSystem.actions.FindAction("CycleHotbar");
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
                selectedPocketIndex = (selectedPocketIndex - (int)cycleHotbar.y + Pockets.Size) % Pockets.Size;
            }
        }

        public void SaveData(ref GameData data)
        {
            if (data.player == null) data.player = new PlayerSaveData();
            data.player.pockets          = SaveGrid(Pockets);
            data.player.equippedBackpack  = SaveGrid(EquippedBackpack);
            data.player.equippedFlashlight = SaveGrid(EquippedFlashlight);
            // Note: backpack ContainerObject contents are saved by its own SaveAgent
        }

        public void LoadData(GameData data)
        {
            if (data.player == null) return;

            var allItems = FindObjectsByType<ItemObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (data.player.pockets != null)
                LoadGrid(Pockets, data.player.pockets, allItems);
            if (data.player.equippedBackpack != null)
                LoadGrid(EquippedBackpack, data.player.equippedBackpack, allItems);
            if (data.player.equippedFlashlight != null)
                LoadGrid(EquippedFlashlight, data.player.equippedFlashlight, allItems);

            InventoryUI.Instance?.Refresh();
        }

        private static List<ContainerSlotSaveData> SaveGrid(ContainerGrid grid)
        {
            var slots = new List<ContainerSlotSaveData>();
            for (int i = 0; i < grid.Size; i++)
            {
                var item = grid.GetItemAt(i);
                if (item != null)
                    slots.Add(new ContainerSlotSaveData { slotIndex = i, itemId = item.ItemId });
            }
            return slots;
        }

        private static void LoadGrid(ContainerGrid grid, List<ContainerSlotSaveData> slots, ItemObject[] allItems)
        {
            foreach (var slot in slots)
            {
                var itemObject = allItems.FirstOrDefault(io => io.GetComponent<SaveAgent>()?.id == slot.itemId);
                if (itemObject == null)
                {
                    Debug.LogWarning($"Could not find item with id {slot.itemId} for inventory load.");
                    continue;
                }

                grid.PlaceItem(new ContainerGridItem(itemObject.itemDefinition, slot.itemId), slot.slotIndex);

                // Restore in-inventory state
                if (itemObject.TryGetComponent<Rigidbody>(out var rb)) rb.isKinematic = true;
                foreach (var col in itemObject.GetComponentsInChildren<Collider>())
                    col.enabled = false;
                itemObject.transform.position = Utilities.InventoryPoolPosition;
            }
        }

        public bool TryAddItem(ContainerGridItem item, ContainerGrid selectedContainerGrid = null, int? index = null)
        {
            if (selectedContainerGrid is not null && index.HasValue)
            {
                var added = selectedContainerGrid.PlaceItem(item, index.Value);
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
            
            for (int i = 0; i <= selectedContainer.Size; i++)
            {
                if (selectedContainer.CanPlaceItem(item, i))
                {
                    bool added = selectedContainer.PlaceItem(item, i);
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
    }
}