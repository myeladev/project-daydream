using System.Collections.Generic;
using ProjectDaydream.Logic;
using UnityEngine;

namespace ProjectDaydream.Objects
{
    public class ElevatorButton : MonoBehaviour, IInteractionProvider
    {
        public ElevatorStation targetStation;
        private Light _buttonLight;
        private Elevator _elevator;
        
        private void Awake()
        {
            _buttonLight = GetComponentInChildren<Light>();
            _elevator = targetStation.GetComponentInParent<Elevator>();
            
        }

        public void OnPressed()
        {
            //_buttonLight.enabled = true;
            // TODO: Also play a sound
        }

        public void Reset()
        {
            //_buttonLight.enabled = false;
        }

        public bool IsInteractable => InteractController.Instance.CanInteract || InteractController.Instance.movingFurniture;
        
        public List<string> GetInteractOptions(InteractContext context)
        {
            return new List<string>(){"Press"};
        }
        
        public void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Press":
                    _elevator.PressElevatorButton(this);
                    break;
            }
        }
    }
}
