using ProjectDaydream.Objects.Items;
using TMPro;
using UnityEngine;

namespace ProjectDaydream.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [SerializeField] private ContainerGridUI pocketGrid;
        [SerializeField] private RectTransform selectedIndicator;
        [SerializeField] private TextMeshProUGUI selectedItemNameText;
        private int SelectedIndex => InventoryController.Instance.selectedPocketIndex;
        
        protected void Awake()
        {
            pocketGrid.Init(InventoryController.Instance.Pockets);
        }

        void Update()
        {
            selectedIndicator.position = pocketGrid.GetCells()[SelectedIndex].transform.position;
            selectedItemNameText.text = InventoryController.Instance.Pockets.GetItemAt(SelectedIndex, 0)?.ItemDefinition?.name ?? string.Empty;
        }
    }
}
