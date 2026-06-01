using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.Common;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.Logic;
using ProjectDaydream.UI;
using UnityEngine;

namespace ProjectDaydream.Objects.Items
{
    [RequireComponent(typeof(SaveAgent))]
    public class ItemObject : Prop, IInteractionProvider
    {
        public ItemDefinition itemDefinition;
        private List<Collider> _colliders;
        private SaveAgent saveAgent;

        protected new void Awake()
        {
            base.Awake();
            _colliders = GetComponentsInChildren<Collider>().ToList();
            saveAgent = GetComponent<SaveAgent>();
        }

        public new List<string> GetInteractOptions(InteractContext context)
        {
            // Get the base prop interactions
            var interactList = base.GetInteractOptions(context);
            // Add the "pick up" interaction for items
            interactList.Add(context == InteractContext.Inventory ? "Drop" : "Pickup");
            // Return the modified list
            return interactList;
        }
        
        public new void Interact(string option, InteractContext context)
        {
            base.Interact(option, context);
            switch (option)
            {
                case "Drop":
                    var targetPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;
                    transform.position = targetPosition;
                    _colliders.ForEach(m => m.enabled = true);
                    Rigidbody.isKinematic = false;
                    Rigidbody.linearVelocity = Vector3.zero;
                    break;
                case "Pickup":
                    var success = InventoryController.Instance.TryAddItem(new ContainerGridItem(itemDefinition, saveAgent.id));

                    if (!success)
                    {
                        FeedbackMessageUIManager.instance.ShowMessage("Not enough inventory space");
                    }
                    else
                    {
                        Rigidbody.isKinematic = true;
                        _colliders.ForEach(m => m.enabled = false);
                        transform.position = Utilities.InventoryPoolPosition;
                    }
                    break;
            }
        }
    }
}
