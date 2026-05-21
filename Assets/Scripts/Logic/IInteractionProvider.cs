using System.Collections.Generic;

namespace ProjectDaydream.Logic
{
    public interface IInteractionProvider
    {
        List<string> GetInteractOptions(InteractContext context);
        void Interact(string option, InteractContext context);
    }

    public enum InteractContext
    {
        Default,
        RightClick,
        Inventory
    }
}