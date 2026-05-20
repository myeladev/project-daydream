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
            
            for (int x = 0; x < _containerGrid.Width; x++)
            {
                for (int y = 0; y < _containerGrid.Height; y++)
                {
                    var spawnedCellGameobject = Instantiate(cellPrefab.gameObject, transform);
                    var newCell = spawnedCellGameobject.GetComponent<ContainerCellUI>();
                    newCell.Init(_containerGrid, x % _containerGrid.Width, x / _containerGrid.Width);
                    _cells.Add(newCell);
                }
            }
        }
        
        public List<ContainerCellUI> GetCells() => _cells;
    }
}