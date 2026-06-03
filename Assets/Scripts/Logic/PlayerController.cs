using System.Collections.Generic;
using System.Linq;
using NWH.Common.SceneManagement;
using NWH.Common.Vehicles;
using NWH.VehiclePhysics2;
using ProjectDaydream.Core;
using ProjectDaydream.DataPersistence;
using ProjectDaydream.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.Logic
{
    public class PlayerController : MonoBehaviour, IDataPersistence
    {
        public CharacterCamera orbitCamera;
        public Transform cameraFollowPoint;
        public CharacterController character;
        public static PlayerController Instance;
        public Transform flashlightTransform;
        public bool IsDriving => _activeVehicle is not null;
        
        [SerializeField] 
        private List<Light> flashlight = new();
        private Vehicle _activeVehicle;
        private InputAction _moveAction;
        private InputAction _lookAction;
        private InputAction _jumpAction;
        private InputAction _sprintAction;
        private InputAction _crouchAction;
        private InputAction _interactAction;
        private InputAction _cancelAction;
        private InputAction _flashlightAction;
        private InputAction _inventoryAction;

        private void Awake()
        {
            Instance = this;
        }
        
        private void Start()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _lookAction = InputSystem.actions.FindAction("Look");
            _jumpAction = InputSystem.actions.FindAction("Jump");
            _sprintAction = InputSystem.actions.FindAction("Sprint");
            _crouchAction = InputSystem.actions.FindAction("Crouch");
            _interactAction = InputSystem.actions.FindAction("Interact");
            _cancelAction = InputSystem.actions.FindAction("Cancel");
            _flashlightAction = InputSystem.actions.FindAction("Flashlight");
            _inventoryAction = InputSystem.actions.FindAction("Inventory");
            
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            orbitCamera.SetFollowTransform(cameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            orbitCamera.IgnoredColliders.Clear();
            orbitCamera.IgnoredColliders.AddRange(character.GetComponentsInChildren<Collider>());
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame
                && (!SceneManager.Instance.IsInMainMenu && !GameplayUI.Instance.IsAnyPanelActive()))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            
            // Handle exiting things
            if (_cancelAction.WasPressedThisFrame() && _activeVehicle is not null)
            {
                var vc = FindAnyObjectByType<VehicleChanger>();
                var exitPosition = _activeVehicle.transform.GetComponentsInChildren<Transform>()
                    .FirstOrDefault(t => t.CompareTag(VehicleChanger.Instance.enterExitTag));
                vc.ExitVehicle(_activeVehicle);
                character.TransitionToState(CharacterState.Default);
                _activeVehicle = null;
                if (exitPosition)
                {
                    character.Motor.SetPosition(exitPosition.position);
                }
            }
                
            // Handle flashlight control
            if (_flashlightAction.WasPressedThisFrame() && Cursor.lockState == CursorLockMode.Locked && !_activeVehicle)
            {
                SetFlashlight(!flashlight.FirstOrDefault()?.enabled ?? false);
            }

            if(flashlightTransform != null)
            {
                flashlightTransform.position = Vector3.Lerp(flashlightTransform.position, cameraFollowPoint.position - (cameraFollowPoint.up * 0.25f), Time.deltaTime * 30f);
                flashlightTransform.rotation = Quaternion.Lerp(flashlightTransform.rotation, Camera.main.transform.rotation, Time.deltaTime * 10f);
            }

            /* TODO: Move to own player stats manager class?
            hunger -= Time.deltaTime * HungerFallRate;
            hunger = Mathf.Clamp(hunger, 0, 120);

            if (isSleeping)
            {
                Time.timeScale = 20f;
                sleep += Time.deltaTime * SleepRecoveryRate;
            }
            else
            {
                Time.timeScale = 1f;
                sleep -= Time.deltaTime * SleepFallRate;
            }
            sleep = Mathf.Clamp(sleep, 0, 100);
            */
            
            HandleCharacterInput();
        }

        private void SetFlashlight(bool on)
        {
            foreach (var fLight in flashlight)
            {
                fLight.enabled = on;
            }
        }

        public void SetActiveVehicle(VehicleController controller)
        {
            SetFlashlight(false);
            character.TransitionToState(CharacterState.Driving);
            _activeVehicle = controller;
        }

        private void LateUpdate()
        {
            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = _lookAction.ReadValue<Vector2>().y;
            float mouseLookAxisRight = _lookAction.ReadValue<Vector2>().x;
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);

            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Apply inputs to the camera
            orbitCamera.UpdateWithInput(Time.deltaTime, lookInputVector);
        }
        
        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = _moveAction.ReadValue<Vector2>().y;
            characterInputs.MoveAxisRight = _moveAction.ReadValue<Vector2>().x;
            characterInputs.CameraRotation = orbitCamera.Transform.rotation;
            characterInputs.JumpDown = _jumpAction.WasPressedThisFrame();
            characterInputs.JumpHeld = _jumpAction.ReadValue<float>() > 0.5f;
            characterInputs.SprintHeld = _sprintAction.ReadValue<float>() > 0.5f;
            characterInputs.CrouchDown = _crouchAction.WasPressedThisFrame();
            characterInputs.CrouchUp = _crouchAction.WasReleasedThisFrame();
            characterInputs.CrouchHeld = _crouchAction.ReadValue<float>() > 0.5f;
            characterInputs.ClimbLadder = _interactAction.ReadValue<float>() > 0.5f;

            // Apply inputs to character
            character.SetInputs(ref characterInputs);
        }
        
        public void LoadData(GameData data)
        {
            if (SceneManager.Instance.IsInMainMenu) return;
            if(data.player == null) return;
            
            if (data.player.position?.Length == 3 && 
                (data.player.position[0] != 0 || data.player.position[1] != 0 || data.player.position[2] != 0))
            {
                character.Motor.SetPosition(new Vector3(data.player.position[0], data.player.position[1], data.player.position[2]));
                
                // TODO: This isn't working, fix it
                Quaternion rotation =  Quaternion.Euler(new Vector3(data.player.rotation[0], data.player.rotation[1], data.player.rotation[2]));
                Camera.main.transform.rotation = rotation;
            }
            else
            {
                var playerSpawn = GameObject.FindGameObjectWithTag("PlayerSpawn");
                character.Motor.SetPosition(playerSpawn.transform.position);
            }

            character.TransitionToState(CharacterState.Default);
        }

        public void SaveData(ref GameData data)
        {
            data.player = new PlayerSaveData
            {
                position = new[] { character.transform.position.x, character.transform.position.y, character.transform.position.z },
                rotation = new[] { character.transform.eulerAngles.x, character.transform.eulerAngles.y, character.transform.eulerAngles.z },
            };
        }
    }
}
