using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.Logic;
using UnityEngine;

namespace ProjectDaydream.Objects.Furniture
{
    public class ToggleableFurnitureLight : Furniture, IInteractionProvider
    {
        [SerializeField]
        private List<Light> lights = new();
        
        [SerializeField]
        private AudioClip sfxOff;
        [SerializeField]
        private AudioClip sfxOn;
        
        private AudioSource _audio;

        protected void Awake()
        {
            _audio = GetComponent<AudioSource>();
        }
        
        public new List<string> GetInteractOptions(InteractContext context)
        {
            var interactText = base.GetInteractOptions(context);
            interactText.Add("Toggle");
            return interactText;
        }

        public new void Interact(string option, InteractContext context)
        {
            base.Interact(option, context);
            switch (option)
            {
                case "Toggle":
                    foreach (var toggleLight in lights)
                    {
                        toggleLight.enabled = !toggleLight.enabled;
                    }
                    _audio.PlayOneShot(lights.Max(m => m.enabled) ? sfxOn : sfxOff);
                    break;
            }
        }
        
    }
}
