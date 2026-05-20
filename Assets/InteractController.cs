using ProjectDaydream.Logic;
using ProjectDaydream.Objects.Furniture;
using ProjectDaydream.Objects.Items;
using ProjectDaydream.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream
{
    public class InteractController : MonoBehaviour
    {
        public static InteractController Instance;
        
        public bool IsHoldingProp => _holdingProp is not null;
        public bool CanInteract => !IsHoldingProp && !movingFurniture;

        [Header("Misc")]
        [SerializeField]
        private MeshFilter furnitureHolderMeshFilter;
        [SerializeField]
        private MeshRenderer furnitureHolderMeshRenderer;
        [SerializeField]
        private FurniturePlacementMarker furniturePlacementMarker;
        [SerializeField]
        private PlayerController player;
        
        [HideInInspector]
        public Furniture movingFurniture;

        private const float FurniturePlacementRange = 4f;
        private Collider _furniturePlacementMarkerCollider;
        private Prop _holdingProp;
        private ItemObject _holdingItemObject;
        private Camera _camera;
        private InputAction _interactAction;
        private InputAction _rotateObjectAction;
        
        private void Awake()
        {
            Instance = this;
            _camera = Camera.main;
            _furniturePlacementMarkerCollider = furniturePlacementMarker.GetComponent<Collider>();
        }

        private void Start()
        {
            _interactAction = InputSystem.actions.FindAction("Interact");
            _rotateObjectAction = InputSystem.actions.FindAction("RotateObject");
        }

        void Update()
        {
            HandleFurnitureMoving();
        }

        private float holdingFurnitureDelay;
        private void HandleFurnitureMoving()
        {
            holdingFurnitureDelay -= Time.deltaTime;
            if (movingFurniture is not null)
            {
                if (Physics.Raycast(_camera.transform.position, _camera.transform.TransformDirection(Vector3.forward),
                        out var hit, FurniturePlacementRange, furnitureLayerMask))
                {
                    furniturePlacementMarker.transform.position = hit.point;
                    furniturePlacementMarker.gameObject.SetActive(true);
                    //if (hit.normal != Vector3.up) blocked = true;
                    SnapRotatedObjectToGround(_furniturePlacementMarkerCollider);
                }
                else
                {
                    furniturePlacementMarker.gameObject.SetActive(false);
                }
                if (_interactAction.WasPressedThisFrame() && holdingFurnitureDelay < 0f)
                {
                    PlaceFurniture(movingFurniture);
                }

                var scrollValue = _rotateObjectAction.ReadValue<float>();
                if (scrollValue != 0)
                {
                    furniturePlacementMarker.transform.rotation = 
                        Quaternion.Euler(
                            furniturePlacementMarker.transform.eulerAngles.x, 
                            furniturePlacementMarker.transform.eulerAngles.y + (Time.deltaTime * 1000f * scrollValue), 
                            furniturePlacementMarker.transform.eulerAngles.z);
                }
            }
        }

        public LayerMask furnitureLayerMask;

        private void SnapRotatedObjectToGround(Collider objectToSnap)
        {
            Collider objectCollider = objectToSnap;
            Vector3 highestPoint = objectCollider.bounds.max;
            RaycastHit hit;
            var leeway = 0.01f;
            if (Physics.Raycast(highestPoint + (Vector3.up * leeway), Vector3.down, out hit, Mathf.Infinity, furnitureLayerMask))
            {
                float newY = hit.point.y;
                objectToSnap.transform.position = new Vector3(objectToSnap.transform.position.x, newY + leeway, objectToSnap.transform.position.z);
            }
        }

        public bool MoveFurniture(Furniture furnitureToMove)
        {
            if (movingFurniture || holdingFurnitureDelay > 0f) return false;
            movingFurniture = furnitureToMove;
            furnitureHolderMeshRenderer.gameObject.SetActive(true);
            var mesh = furnitureToMove.GetComponent<MeshFilter>().mesh;
            furnitureHolderMeshFilter.mesh = mesh;
            furnitureHolderMeshRenderer.materials = furnitureToMove.GetComponent<MeshRenderer>().materials;
            furniturePlacementMarker.GetComponent<MeshFilter>().mesh = mesh;
            furniturePlacementMarker.transform.localScale = furnitureToMove.transform.localScale;
            furniturePlacementMarker.GetComponent<MeshCollider>().sharedMesh = mesh;
            holdingFurnitureDelay = 0.25f;
            return true;

        }

        private void PlaceFurniture(Furniture furnitureToPlace)
        {
            if (movingFurniture == furnitureToPlace)
            {
                if (furniturePlacementMarker.IsFree)
                {
                    if (Physics.Raycast(_camera.transform.position,
                            _camera.transform.TransformDirection(Vector3.forward),
                            out _, FurniturePlacementRange, furnitureLayerMask))
                    {
                        movingFurniture.Place(furniturePlacementMarker.transform.position, furniturePlacementMarker.transform.rotation);
                        ReleaseHeldFurniture();
                    }
                }
                else
                {
                    FeedbackMessageUIManager.instance.ShowMessage("Object is blocked by something");
                }
            }
        }

        public void ReleaseHeldFurniture()
        {
            holdingFurnitureDelay = 0.25f;
            movingFurniture = null;
            furnitureHolderMeshRenderer.gameObject.SetActive(false);
            furniturePlacementMarker.gameObject.SetActive(false);
        }

        public bool CanClean() => _holdingItemObject?.itemDefinition.canClean ?? false;
    }
}
