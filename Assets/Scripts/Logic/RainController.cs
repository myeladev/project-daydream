using ProjectDaydream.Core;
using UnityEngine;

namespace ProjectDaydream.Logic
{
    public class FollowPlayer : MonoBehaviour
    {
        private Vector3 offset;
        public PlayerController player;
        
        void Awake()
        {
            offset = transform.localPosition;
        }

        // Update is called once per frame
        void Update()
        {
            var targetPosition = player.character.transform.position;
            if(SceneManager.Instance.IsInMainMenu) targetPosition = Camera.main.transform.position;
            transform.position = targetPosition + offset;
        }
    }
}
