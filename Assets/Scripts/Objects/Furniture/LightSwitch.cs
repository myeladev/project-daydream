using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.Logic;
using UnityEngine;

namespace ProjectDaydream.Objects.Furniture
{
    public class LightSwitch : MonoBehaviour, IInteractionProvider
    {
        private List<LightController> attachedLights;
        
        [SerializeField]
        private bool isOn;
        
        [SerializeField]
        private AudioClip sfxOff;
        [SerializeField]
        private AudioClip sfxOn;
        
        private AudioSource _audio;

        private void Awake()
        {
            _audio = GetComponent<AudioSource>();
            attachedLights = GetComponentsInChildren<LightController>().ToList();
        }

        // Start is called before the first frame update
        void Start()
        {
            Refresh();
        }

        void Refresh()
        {
            foreach (var attachedLight in attachedLights)
            {
                attachedLight.Toggle(isOn);
            }
        }

        public bool IsInteractable => true;
        public List<string> GetInteractOptions(InteractContext context)
        {
            return context == InteractContext.Default ? 
                new List<string>
                {
                    "Toggle Lights",
                }
                : new List<string>();
        }

        public void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Toggle Lights":
                    Toggle();
                    break;
            }
        }

        public void Toggle()
        {
            isOn = !isOn;
            _audio.PlayOneShot(isOn ? sfxOn : sfxOff);
            Refresh();
        }
    }
}
