using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ProjectDaydream.Logic
{
    [RequireComponent(typeof(DecalProjector))]
    public class CleanableDecal : MonoBehaviour, IInteractable
    {
        private DecalProjector projector;

        private float minOpacity => 0.25f;
        
        [SerializeField] 
        [Range(1, 10)]
        private float stubbornness;
        private void Awake()
        {
            projector = GetComponent<DecalProjector>();
        }
        
        private void Clean(float amount)
        {
            if (projector.fadeFactor > 0)
            {
                amount /= stubbornness;
                projector.fadeFactor -= amount;

                // If the decal is transparent enough to be considered cleaned
                if (projector.fadeFactor <= minOpacity)
                {
                    // Then just clean it anyway
                    projector.fadeFactor = 0;
                }
            }
        }

        public bool IsInteractable => true // TODO: PlayerController.Instance.character.CanClean()
                                    && projector.fadeFactor > 0;
        public List<string> GetInteractOptions(InteractContext context)
        {
            var interactList = new List<string>()
            {
                // Add the "clean" interaction for the decal
                "Clean"
            };
            // Return the modified list
            return interactList;
        }

        public void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Clean":
                    Clean(0.18f);
                    break;
            }
        }
    }
}
