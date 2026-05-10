using System.Collections.Generic;
using NWH.Common.SceneManagement;
using NWH.VehiclePhysics2;
using ProjectDaydream.Logic;
using ProjectDaydream.Objects.Items;

namespace ProjectDaydream.Objects.Vehicles
{
    public class VehicleObject : Prop, IInteractable
    {
        private VehicleController controller;

        //private CarLights carLights;

        protected override void Awake()
        {
            base.Awake();
            controller = GetComponent<VehicleController>();
        }
        
        private void Start()
        {
            //carLights = GetComponent<CarLights>();
        }

        public new bool IsInteractable => InteractController.Instance.CanInteract;
        public new List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = base.GetInteractOptions(context);
            // Add the "drive" interaction for items
            interactList.Add("Drive");
            // Return the modified list
            return interactList;
        }
        public new void Interact(string option, InteractContext context)
        {
            base.Interact(option, context);
            switch (option)
            {
                case "Drive":
                    Drive();
                    break;
            }
        }

        private void Drive()
        {
            var vc = FindAnyObjectByType<VehicleChanger>();
            vc.EnterVehicle(controller);
            PlayerController.Instance.SetActiveVehicle(controller);

            //carLights.OperateFrontLights(true);
        }
    }
}
