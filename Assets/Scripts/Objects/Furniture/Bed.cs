using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.UI;

namespace ProjectDaydream.Objects.Furniture
{
    public class Bed : Furniture, IInteractionProvider
    {
        public new List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = base.GetInteractOptions(context);
            // Add the "pick up" interaction for items
            interactList.Add("Sleep");
            // Return the modified list
            return interactList;
        }
        
        public new void Interact(string option, InteractContext context)
        {
            base.Interact(option, context);
            switch (option)
            {
                case "Sleep":
                    if (true) // TODO: PlayerController.Instance.sleep <= 90f)
                    {
                        // TODO: PlayerController.Instance.Sleep(this);
                    }
                    else
                    {
                        FeedbackMessageUIManager.instance.ShowMessage("You're not tired enough to sleep");
                    }
                    break;
            }
        }
    }
}
