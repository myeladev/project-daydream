using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.Logic;
using UnityEngine;

namespace ProjectDaydream.Objects
{
    public class Interactable : MonoBehaviour
    {
        public bool isInteractable = true;

        public List<string> GetInteractOptions(InteractContext context)
        {
            if (!isInteractable) return new List<string>();
            var interactionProviders = GetComponents<MonoBehaviour>().OfType<IInteractionProvider>().ToList();
            var options = interactionProviders.SelectMany(ip => ip.GetInteractOptions(InteractContext.Default)).ToList();

            if (context == InteractContext.RightClick)
            {
                options.Remove("Grab");
            }

            options = options
                .OrderBy(o => o == "Grab" ? 0 : 1)
                // Then by text alphabetically
                .ThenBy(o => o)
                .ToList();
            
            return options;
        }

        public void Interact(string option, InteractContext context)
        {
            if (!isInteractable) return;
            var interactionProviders = GetComponents<MonoBehaviour>().OfType<IInteractionProvider>().ToList();
            interactionProviders.ForEach(ip => ip.Interact(option, context));
        }
    }
}