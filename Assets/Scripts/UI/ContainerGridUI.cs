using System.Collections.Generic;
using ProjectDaydream.Logic;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectDaydream.UI
{
    [RequireComponent(typeof(GridLayoutGroup))]
    public class ContainerGridUI : MonoBehaviour
    {
        private ContainerGrid _containerGrid;
        [SerializeField] private ContainerCellUI cellPrefab;
        private List<ContainerCellUI> _cells = new ();

        public void Init(ContainerGrid containerGrid)
        {
            _containerGrid = containerGrid;
            Refresh();
        }
        
        public void Refresh()
        {
            foreach (var cell in _cells)
            {
                Destroy(cell.gameObject);
            }

            _cells.Clear();

            if (_containerGrid is null)
            {
                Debug.LogWarning("ContainerGrid has not been initialized yet!");
                return;
            }
            
            for (int i = 0; i < _containerGrid.Size; i++)
            {
                var spawnedCellGameobject = Instantiate(cellPrefab.gameObject, transform);
                var newCell = spawnedCellGameobject.GetComponent<ContainerCellUI>();
                newCell.Init(_containerGrid, i);
                _cells.Add(newCell);
            }
        }
        
        public List<ContainerCellUI> GetCells() => _cells;
    }
}