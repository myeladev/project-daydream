using System.Collections.Generic;
using ProjectDaydream.Logic;
using ProjectDaydream.Objects.Items;
using ProjectDaydream.UI;

namespace ProjectDaydream.Objects
{
    public class DoorObject : Prop, IInteractionProvider
    {
        public bool shut;
        public bool locked;
        
        private PhysicsDoor _door;
        // Start is called before the first frame update
        protected new void Awake()
        {
            base.Awake();
            _door = GetComponentInParent<PhysicsDoor>();
        }

        void Start()
        { 
            if (shut || locked)
            {
                _door.Shut();
            }
        }
        
        public new List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = new List<string>();
            if (shut)
            {
                interactList.Add("Open");
            }
            else
            {
                interactList.Add("Grab");
            }
            // Return the modified list
            return interactList;
        }
        
        public new void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Open":
                    if (locked)
                    {
                        FeedbackMessageUIManager.instance.ShowMessage("It's locked");    
                    }
                    else if(shut)
                    {
                        shut = false;
                        _door.Open();
                    }
                    break;
                case "Grab":
                    if (!shut && !locked)
                    {
                        base.Interact(option, context);
                    }
                    break;
            }
        }
    }
}
