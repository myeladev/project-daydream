using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.Logic;
using ProjectDaydream.SaveData;
using UnityEngine;

namespace ProjectDaydream.Objects.Items
{
    public class Prop : MonoBehaviour, IInteractable, IDataPersistence
    {
        public bool IsInteractable => InteractController.Instance.CanInteract;

        public List<string> GetInteractOptions(InteractContext context)
        {
            return context == InteractContext.Default ? 
                new List<string>
                {
                    "Grab",
                }
                : new List<string>();
        }

        protected Rigidbody Rigidbody;
        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        protected virtual void Awake()
        {
            Rigidbody = GetComponent<Rigidbody>();
            _originalPosition = transform.position;
            _originalRotation = transform.rotation;
        }

        private void Update()
        {
            // Item has fallen out of the world for some reason
            if (transform.position.y < -1000f)
            {
                // Reset it
                transform.position = _originalPosition;
                transform.rotation = _originalRotation;
                Rigidbody.linearVelocity = Vector3.zero;
            }
        }

        public void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Grab":
                    
                    break;
            }
        }

        public void LoadData(GameData data)
        {
            PropSaveData saveData = data.props.FirstOrDefault(m => GetComponent<SaveAgent>().id == m.id);
            if (saveData == null) return;
            
            LoadFromSaveData(saveData);
        }

        protected virtual void LoadFromSaveData(PropSaveData saveData)
        {
            transform.position = new Vector3(saveData.position[0], saveData.position[1], saveData.position[2]);
            transform.rotation = Quaternion.Euler(saveData.rotation[0], saveData.rotation[1], saveData.rotation[2]);
        }

        public void SaveData(ref GameData data)
        {
            data.props.Add(GetSaveData());
        }
        
        protected PropSaveData GetSaveData()
        {
            return new PropSaveData
            {
                id = GetComponent<SaveAgent>().id,
                position = new[] { transform.position.x, transform.position.y, transform.position.z },
                rotation = new[] { transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z },
            };
        }
    }
    
    [Serializable]
    public class PropSaveData
    {
        public string id;
        public float[] position;
        public float[] rotation;
    }
}