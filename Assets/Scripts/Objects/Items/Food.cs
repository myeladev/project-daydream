using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.UI;

namespace ProjectDaydream.Objects.Items
{
    public class Food : ItemObject, IInteractionProvider
    {
        public float hungerRestoration;
        
        public new List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = base.GetInteractOptions(context);
            // Add the "pick up" interaction for items
            interactList.Add("Eat");
            // Return the modified list
            return interactList;
        }
        
        public new void Interact(string option, InteractContext context)
        {
            base.Interact(option, context);
            switch (option)
            {
                case "Eat":
                    // TODO: PlayerController.instance.EatFood(this);
                    if (context == InteractContext.Inventory)
                    {
                        //InventoryController.Instance.RemoveFromInventory(this);
                        InventoryUI.Instance.Refresh();
                    }
                    Destroy(gameObject);
                    break;
            }
        }
    }
}
