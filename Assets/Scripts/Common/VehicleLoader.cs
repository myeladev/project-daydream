using NWH.Common.SceneManagement;
using NWH.VehiclePhysics2;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectDaydream.Common
{
    [RequireComponent(typeof(VehicleChanger))]
    public class VehicleLoader : MonoBehaviour
    {
        private VehicleChanger vehicleChanger;
        
        private void Awake()
        {
            vehicleChanger = GetComponent<VehicleChanger>();
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += LinkVehicles;
            SceneManager.sceneUnloaded += ClearVehicles;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= LinkVehicles;
            SceneManager.sceneUnloaded -= ClearVehicles;
        }

        private void LinkVehicles(Scene scene, LoadSceneMode mode)
        {
            var vehicles = FindObjectsByType<VehicleController>(FindObjectsSortMode.None);

            foreach (var vehicle in vehicles)
            {
                vehicleChanger.RegisterVehicle(vehicle);
            }
        }
        
        private void ClearVehicles(Scene scene)
        {
            vehicleChanger.vehicles.Clear();
        }
    }
}