using System.Collections.Generic;
using ProjectDaydream.UI;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class ContainerObject : MonoBehaviour, IInteractionProvider
    {
        public int width = 4;
        public int height = 2;
        private void Awake()
        {
            ContainerGrid = new ContainerGrid(width, height);
        }
        
        public ContainerGrid ContainerGrid;
        public bool IsInteractable => InteractController.Instance.CanInteract;
        public List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = new List<string>();//base.GetInteractOptions(context);

            if (context == InteractContext.Default)
            {
                interactList.Add("Open");
            }
            
            return interactList;
        }
        
        public new void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Open":
                    GameplayUI.Instance.OpenContainer(this);
                    break;
            }
        }
    }
}