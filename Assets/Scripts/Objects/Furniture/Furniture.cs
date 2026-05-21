using System;
using System.Collections.Generic;
using System.Linq;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.Logic;
using ProjectDaydream.SaveData;
using UnityEngine;
using CharacterController = ProjectDaydream.Logic.CharacterController;

namespace ProjectDaydream.Objects.Furniture
{
    [RequireComponent(typeof(Interactable))]
    public class Furniture : MonoBehaviour, IInteractionProvider, IDataPersistence
    {
        public bool IsInteractable => InteractController.Instance.CanInteract;

        public Transform seatingAnchor;

        public List<string> GetInteractOptions(InteractContext context)
        {
            var list = new List<string>();
            list.Add("Move");
            if(seatingAnchor) list.Add("Sit");
            return list;
        }

        public void Interact(string option, InteractContext context)
        {
            switch (option)
            {
                case "Move":
                    if (InteractController.Instance.MoveFurniture(this))
                    {
                        gameObject.SetActive(false);
                    }
                    break;
                case "Sit":
                    PlayerController.Instance.character.Sit(seatingAnchor);
                    break;
            }
        }

        public void Place(Vector3 position, Quaternion rotation)
        {
            gameObject.SetActive(true);
            transform.position = position;
            transform.rotation = rotation;
            GetComponent<Collider>().enabled = true;
        }

        public void LoadData(GameData data)
        {
            FurnitureSaveData saveData = data.furniture.FirstOrDefault(m => GetComponent<SaveAgent>().id == m.id);
            if (saveData == null) return;

            transform.SetPositionAndRotation(
                new Vector3(saveData.position[0], saveData.position[1], saveData.position[2]),
                Quaternion.Euler(saveData.rotation[0], saveData.rotation[1], saveData.rotation[2])
            );
        }

        public void SaveData(ref GameData data)
        {
            data.furniture.Add(new FurnitureSaveData {
                id = GetComponent<SaveAgent>().id,
                position = new[]{ transform.position.x, transform.position.y, transform.position.z },
                rotation = new[]{ transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z },
            });
        }
    }
    
    [Serializable]
    public class FurnitureSaveData
    {
        public string id;
        public float[] position;
        public float[] rotation;
    }
}
