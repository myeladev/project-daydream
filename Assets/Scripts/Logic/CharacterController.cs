using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using ProjectDaydream.KinematicCharacterController.Core;
using DG.Tweening;

namespace ProjectDaydream.Logic
{
    public enum CharacterState
    {
        Default,
        Climbing,
        Swimming,
        Sitting,
        Driving
    }

    public enum ClimbingState
    {
        Anchoring,
        Climbing,
        DeAnchoring
    }
    
    public enum SittingState
    {
        Anchoring,
        Sitting,
        DeAnchoring
    }

    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool JumpDown;
        public bool JumpHeld;
        public bool SprintHeld;
        public bool CrouchDown;
        public bool CrouchUp;
        public bool CrouchHeld;
        public bool ClimbLadder;
    }

    public class CharacterController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 7f;
        public float MaxStableSprintSpeed = 10f;
        public float StableMovementSharpness = 15;
        public float OrientationSharpness = 10;
        public float MaxStableDistanceFromLedge = 5f;
        [Range(0f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 10f;
        public float AirAccelerationSpeed = 5f;
        public float Drag = 0.1f;
        
        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;

        [Header("Swimming")]
        public Transform SwimmingReferencePoint;
        public LayerMask WaterLayer;
        public float SwimmingSpeed = 4f;
        public float SwimmingMovementSharpness = 3;
        public float SwimmingOrientationSharpness = 2f;
        
        [Header("Ladder Climbing")]
        public float climbingSpeed = 4f;
        public float anchoringDuration = 0.25f;
        public LayerMask interactionLayer;

        [Header("Misc")]
        public List<Collider> IgnoredColliders = new List<Collider>();
        public bool OrientTowardsGravity = true;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform MeshRoot;

        public CharacterState CurrentCharacterState { get; private set; }

        private Collider[] _probedColliders = new Collider[8];
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private bool _jumpRequested = false;
        private bool _jumpConsumed = false;
        private bool _jumpedThisFrame = false;
        private bool _jumpInputIsHeld = false;
        private bool _crouchInputIsHeld = false;
        private bool _sprintInputIsHeld = false;
        private float _timeSinceJumpRequested = Mathf.Infinity;
        private float _timeSinceLastAbleToJump = 0f;
        private Vector3 _internalVelocityAdd = Vector3.zero;
        private bool _shouldBeCrouching = false;
        private bool _isCrouching = false;
        private Collider _waterZone;

        // Seating vars
        public Transform ActiveSeatingAnchor { get; set; }
        
        // Ladder vars
        private float _ladderUpDownInput;
        private LadderController ActiveLadder { get; set; }
        private SittingState _internalSittingState;
        private SittingState SittingState
        {
            get => _internalSittingState;
            set
            {
                _internalSittingState = value;
                _anchoringTimer = 0f;
                _anchoringStartPosition = Motor.TransientPosition;
                _anchoringStartRotation = Motor.TransientRotation;
            }
        }
        private ClimbingState _internalClimbingState;
        private ClimbingState ClimbingState
        {
            get => _internalClimbingState;
            set
            {
                _internalClimbingState = value;
                _anchoringTimer = 0f;
                _anchoringStartPosition = Motor.TransientPosition;
                _anchoringStartRotation = Motor.TransientRotation;
            }
        }
        private Vector3 _ladderTargetPosition;
        private Quaternion _ladderTargetRotation;
        private float _onLadderSegmentState = 0;
        private float _anchoringTimer = 0f;
        private Vector3 _anchoringStartPosition = Vector3.zero;
        private Quaternion _anchoringStartRotation = Quaternion.identity;
        private Quaternion _rotationBeforeAnchoring = Quaternion.identity;

        private void Start()
        {
            // Assign to motor
            Motor.CharacterController = this;

            // Handle initial state
            TransitionToState(CharacterState.Default);
        }

        /// <summary>
        /// Handles movement state transitions and enter/exit callbacks
        /// </summary>
        public void TransitionToState(CharacterState newState)
        {
            CharacterState tmpInitialState = CurrentCharacterState;
            OnStateExit(tmpInitialState, newState);
            CurrentCharacterState = newState;
            OnStateEnter(newState, tmpInitialState);
        }

        /// <summary>
        /// Event when entering a state
        /// </summary>
        public void OnStateEnter(CharacterState state, CharacterState fromState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    Motor.SetGroundSolvingActivation(true);
                    break;
                }
                case CharacterState.Swimming:
                {
                    Motor.SetGroundSolvingActivation(false);
                    break;
                }
                case CharacterState.Climbing:
                {
                    _rotationBeforeAnchoring = Motor.TransientRotation;

                    Motor.SetMovementCollisionsSolvingActivation(false);
                    Motor.SetGroundSolvingActivation(false);
                    ClimbingState = ClimbingState.Anchoring;

                    // Store the target position and rotation to snap to
                    _ladderTargetPosition = ActiveLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                    _ladderTargetRotation = ActiveLadder.transform.rotation;
                    break;
                }
                case CharacterState.Sitting:
                {
                    _rotationBeforeAnchoring = Motor.TransientRotation;

                    Motor.SetMovementCollisionsSolvingActivation(false);
                    Motor.SetGroundSolvingActivation(false);
                    SittingState = SittingState.Anchoring;
                    break;
                }
            }
        }

        /// <summary>
        /// Event when exiting a state
        /// </summary>
        public void OnStateExit(CharacterState state, CharacterState toState)
        {
            switch (state)
            {
                case CharacterState.Default:
                {
                    break;
                }
                case CharacterState.Climbing:
                {
                    Motor.SetMovementCollisionsSolvingActivation(true);
                    Motor.SetGroundSolvingActivation(true);
                    break;
                }
                case CharacterState.Sitting:
                {
                    Motor.SetMovementCollisionsSolvingActivation(true);
                    Motor.SetGroundSolvingActivation(true);
                    ActiveSeatingAnchor = null;
                    _shouldBeCrouching = false;
                    break;
                }
            }
        }

        /// <summary>
        /// This is called every frame by PlayerController in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Handle ladder transitions
            _ladderUpDownInput = inputs.MoveAxisForward;
            if (inputs.ClimbLadder)
            {
                if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, interactionLayer, QueryTriggerInteraction.Collide) > 0)
                {
                    if (_probedColliders[0] != null)
                    {
                        // Handle ladders
                        LadderController ladder = _probedColliders[0].gameObject.GetComponent<LadderController>();
                        if (ladder)
                        {
                            // Transition to ladder climbing state
                            if (CurrentCharacterState == CharacterState.Default)
                            {
                                ActiveLadder = ladder;
                                TransitionToState(CharacterState.Climbing);
                            }
                            // Transition back to default movement state
                            else if (CurrentCharacterState == CharacterState.Climbing)
                            {
                                ClimbingState = ClimbingState.DeAnchoring;
                                _ladderTargetPosition = Motor.TransientPosition;
                                _ladderTargetRotation = _rotationBeforeAnchoring;
                            }
                        }
                    }
                }
            }
            
            _jumpInputIsHeld = inputs.JumpHeld;
            _crouchInputIsHeld = inputs.CrouchHeld;
            _sprintInputIsHeld = inputs.SprintHeld;

            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);

            // Calculate camera direction and rotation on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.forward, Motor.CharacterUp).normalized;
            if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(inputs.CameraRotation * Vector3.up, Motor.CharacterUp).normalized;
            }
            Quaternion cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, Motor.CharacterUp);

            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                    // Move and look inputs
                    _moveInputVector = cameraPlanarRotation * moveInputVector;
                    _lookInputVector = cameraPlanarDirection;

                    // Jumping input
                    if (inputs.JumpDown)
                    {
                        _timeSinceJumpRequested = 0f;
                        _jumpRequested = true;
                    }

                    // Crouching input
                    if (inputs.CrouchDown)
                    {
                        _shouldBeCrouching = true;

                        if (!_isCrouching)
                        {
                            _isCrouching = true;
                            Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                            MeshRoot.DOScale(new Vector3(1f, 0.5f, 1f), 0.2f).SetEase(Ease.OutQuad);
                        }
                    }
                    else if (inputs.CrouchUp)
                    {
                        _shouldBeCrouching = false;
                    }
                    break;
                case CharacterState.Swimming:
                    _jumpRequested = inputs.JumpHeld;

                    _moveInputVector = inputs.CameraRotation * moveInputVector;
                    _lookInputVector = cameraPlanarDirection;
                    break;
                case CharacterState.Sitting:
                    _lookInputVector = cameraPlanarDirection;

                    _shouldBeCrouching = true;

                    if (!_isCrouching)
                    {
                        _isCrouching = true;
                        Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                        MeshRoot.DOScale(new Vector3(1f, 0.5f, 1f), anchoringDuration).SetEase(Ease.OutQuad);
                    }
                    
                    _jumpRequested = inputs.JumpDown;

                    _moveInputVector = inputs.CameraRotation * moveInputVector;
                    _lookInputVector = cameraPlanarDirection;
                    break;
                case CharacterState.Driving:
                    break;
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            // Do a character overlap test to detect water surfaces
            if (Motor.CharacterOverlap(Motor.TransientPosition, Motor.TransientRotation, _probedColliders, WaterLayer, QueryTriggerInteraction.Collide) > 0)
            {
                // If a water surface was detected
                if (_probedColliders[0] != null)
                {
                    // If the swimming reference point is inside the box, make sure we are in swimming state
                    if (Physics.ClosestPoint(SwimmingReferencePoint.position, _probedColliders[0], _probedColliders[0].transform.position, _probedColliders[0].transform.rotation) == SwimmingReferencePoint.position)
                    {
                        if (CurrentCharacterState == CharacterState.Default)
                        {
                            TransitionToState(CharacterState.Swimming);
                            _waterZone = _probedColliders[0];
                        }
                    }
                    // otherwise; default state
                    else
                    {
                        if (CurrentCharacterState == CharacterState.Swimming)
                        {
                            TransitionToState(CharacterState.Default);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                case CharacterState.Swimming:
                case CharacterState.Sitting:
                    {
                        if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
                        {
                            // Smoothly interpolate from current to target look direction
                            Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, _lookInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                            // Set the current rotation (which will be used by the KinematicCharacterMotor)
                            currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
                        }
                        if (OrientTowardsGravity)
                        {
                            // Rotate from current up to invert gravity
                            currentRotation = Quaternion.FromToRotation((currentRotation * Vector3.up), -Gravity) * currentRotation;
                        }
                        break;
                    }
                case CharacterState.Climbing:
                    {
                        switch (ClimbingState)
                        {
                            case ClimbingState.Climbing:
                                currentRotation = ActiveLadder.transform.rotation;
                                break;
                            case ClimbingState.Anchoring:
                            case ClimbingState.DeAnchoring:
                                currentRotation = Quaternion.Slerp(_anchoringStartRotation, _ladderTargetRotation, (_anchoringTimer / anchoringDuration));
                                break;
                        }
                        break;
                    }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    Vector3 targetMovementVelocity;
                    if (Motor.GroundingStatus.IsStableOnGround)
                    {
                        // Reorient velocity on slope
                        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                        // Calculate target velocity
                        Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                        Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
                        var usedMoveSpeed = _sprintInputIsHeld ? MaxStableSprintSpeed : MaxStableMoveSpeed;
                        usedMoveSpeed *= _isCrouching ? 0.5f : 1f;
                        targetMovementVelocity = reorientedInput * usedMoveSpeed;

                        // Smooth movement Velocity
                        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
                    }
                    else
                    {
                        // Add move input
                        if (_moveInputVector.sqrMagnitude > 0f)
                        {
                            targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                            // Prevent climbing on un-stable slopes with air movement
                            if (Motor.GroundingStatus.FoundAnyGround)
                            {
                                Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                                targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                            }

                            Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                            currentVelocity += velocityDiff * (AirAccelerationSpeed * deltaTime);
                        }

                        // Gravity
                        currentVelocity += Gravity * deltaTime;

                        // Drag
                        currentVelocity *= (1f / (1f + (Drag * deltaTime)));
                    }

                    // Handle jumping
                    {
                        _jumpedThisFrame = false;
                        _timeSinceJumpRequested += deltaTime;
                        if (_jumpRequested)
                        {

                            // See if we actually are allowed to jump
                            if (!_jumpConsumed && ((AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround) || _timeSinceLastAbleToJump <= JumpPostGroundingGraceTime))
                            {
                                // Calculate jump direction before ungrounding
                                Vector3 jumpDirection = Motor.CharacterUp;
                                if (Motor.GroundingStatus.FoundAnyGround && !Motor.GroundingStatus.IsStableOnGround)
                                {
                                    jumpDirection = Motor.GroundingStatus.GroundNormal;
                                }

                                // Makes the character skip ground probing/snapping on its next update. 
                                // If this line weren't here, the character would remain snapped to the ground when trying to jump. Try commenting this line out and see.
                                Motor.ForceUnground(0.1f);

                                // Add to the return velocity and reset jump state
                                currentVelocity += (jumpDirection * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                                _jumpRequested = false;
                                _jumpConsumed = true;
                                _jumpedThisFrame = true;
                            }
                        }
                    }

                    // Take into account additive velocity
                    if (_internalVelocityAdd.sqrMagnitude > 0f)
                    {
                        currentVelocity += _internalVelocityAdd;
                        _internalVelocityAdd = Vector3.zero;
                    }
                    break;
                }
                case CharacterState.Swimming:
                {
                    if (!_waterZone) break;
                    float verticalInput = 0f + (_jumpInputIsHeld ? 1f : 0f) + (_crouchInputIsHeld ? -1f : 0f);

                    // Smoothly interpolate to target swimming velocity
                    Vector3 targetMovementVelocity = (_moveInputVector + (Motor.CharacterUp * verticalInput)).normalized * SwimmingSpeed;
                    Vector3 smoothedVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-SwimmingMovementSharpness * deltaTime));

                    // See if our swimming reference point would be out of water after the movement from our velocity has been applied
                    {
                        Vector3 resultingSwimmingReferancePosition = Motor.TransientPosition + (smoothedVelocity * deltaTime) + (SwimmingReferencePoint.position - Motor.TransientPosition);
                        Vector3 closestPointWaterSurface = Physics.ClosestPoint(resultingSwimmingReferancePosition, _waterZone, _waterZone.transform.position, _waterZone.transform.rotation);

                        // if our position would be outside the water surface on next update, project the velocity on the surface normal so that it would not take us out of the water
                        if (closestPointWaterSurface != resultingSwimmingReferancePosition)
                        {
                            Vector3 waterSurfaceNormal = (resultingSwimmingReferancePosition - closestPointWaterSurface).normalized;
                            smoothedVelocity = Vector3.ProjectOnPlane(smoothedVelocity, waterSurfaceNormal);

                            // Jump out of water
                            if (_jumpRequested)
                            {
                                smoothedVelocity += (Motor.CharacterUp * JumpSpeed) - Vector3.Project(currentVelocity, Motor.CharacterUp);
                            }
                        }
                    }

                    currentVelocity = smoothedVelocity;
                    break;
                }
                case CharacterState.Climbing:
                {
                    currentVelocity = Vector3.zero;

                    switch (ClimbingState)
                    {
                        case ClimbingState.Climbing:
                            currentVelocity = (_ladderUpDownInput * ActiveLadder.transform.up).normalized * climbingSpeed;
                            break;
                        case ClimbingState.Anchoring:
                        case ClimbingState.DeAnchoring:
                            Vector3 tmpPosition = Vector3.Lerp(_anchoringStartPosition, _ladderTargetPosition, (_anchoringTimer / anchoringDuration));
                            currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpPosition, deltaTime);
                            break;
                    }
                    break;
                }
                case CharacterState.Sitting:
                {
                    currentVelocity = Vector3.zero;

                    switch (SittingState)
                    {
                        case SittingState.Sitting:
                            break;
                        case SittingState.Anchoring:
                            Vector3 tmpAnchorPosition = Vector3.Lerp(_anchoringStartPosition, ActiveSeatingAnchor.position, (_anchoringTimer / anchoringDuration));
                            currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpAnchorPosition, deltaTime);
                            break;
                        case SittingState.DeAnchoring:
                            Vector3 tmpDeanchorPosition = Vector3.Lerp(ActiveSeatingAnchor.position, _anchoringStartPosition, (_anchoringTimer / anchoringDuration));
                            currentVelocity = Motor.GetVelocityForMovePosition(Motor.TransientPosition, tmpDeanchorPosition, deltaTime);
                            break;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    // Handle jump-related values
                    {
                        // Handle jumping pre-ground grace period
                        if (_jumpRequested && _timeSinceJumpRequested > JumpPreGroundingGraceTime)
                        {
                            _jumpRequested = false;
                        }

                        if (AllowJumpingWhenSliding ? Motor.GroundingStatus.FoundAnyGround : Motor.GroundingStatus.IsStableOnGround)
                        {
                            // If we're on a ground surface, reset jumping values
                            if (!_jumpedThisFrame)
                            {
                                _jumpConsumed = false;
                            }
                            _timeSinceLastAbleToJump = 0f;
                        }
                        else
                        {
                            // Keep track of time since we were last able to jump (for grace period)
                            _timeSinceLastAbleToJump += deltaTime;
                        }
                    }

                    // Handle uncrouching
                    if (_isCrouching && !_shouldBeCrouching)
                    {
                        // Do an overlap test with the character's standing height to see if there are any obstructions
                        Motor.SetCapsuleDimensions(0.5f, 2f, 1f);
                        if (Motor.CharacterOverlap(
                            Motor.TransientPosition,
                            Motor.TransientRotation,
                            _probedColliders,
                            Motor.CollidableLayers,
                            QueryTriggerInteraction.Ignore) > 0)
                        {
                            // If obstructions, just stick to crouching dimensions
                            Motor.SetCapsuleDimensions(0.5f, 1f, 0.5f);
                        }
                        else
                        {
                            // If no obstructions, uncrouch
                            MeshRoot.DOScale(new Vector3(1f, 1f, 1f), 0.2f).SetEase(Ease.OutQuad);
                            _isCrouching = false;
                        }
                    }
                    break;
                }
                case CharacterState.Climbing:
                {
                    switch (ClimbingState)
                    {
                        case ClimbingState.Climbing:
                            // Detect getting off ladder during climbing
                            ActiveLadder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);
                            if (Mathf.Abs(_onLadderSegmentState) > 0.05f)
                            {
                                ClimbingState = ClimbingState.DeAnchoring;

                                // If we're higher than the ladder top point
                                if (_onLadderSegmentState > 0)
                                {
                                    _ladderTargetPosition = ActiveLadder.TopReleasePoint.position;
                                    _ladderTargetRotation = ActiveLadder.TopReleasePoint.rotation;
                                }
                                // If we're lower than the ladder bottom point
                                else if (_onLadderSegmentState < 0)
                                {
                                    _ladderTargetPosition = ActiveLadder.BottomReleasePoint.position;
                                    _ladderTargetRotation = ActiveLadder.BottomReleasePoint.rotation;
                                }
                            }
                            break;
                        case ClimbingState.Anchoring:
                        case ClimbingState.DeAnchoring:
                            // Detect transitioning out from anchoring states
                            if (_anchoringTimer >= anchoringDuration)
                            {
                                if (ClimbingState == ClimbingState.Anchoring)
                                {
                                    ClimbingState = ClimbingState.Climbing;
                                }
                                else if (ClimbingState == ClimbingState.DeAnchoring)
                                {
                                    TransitionToState(CharacterState.Default);
                                }
                            }

                            // Keep track of time since we started anchoring
                            _anchoringTimer += deltaTime;
                            break;
                    }
                    break;
                }
                case CharacterState.Sitting:
                {
                    switch (SittingState)
                    {
                        case SittingState.Sitting:
                            if (_jumpInputIsHeld)
                            {
                                SittingState = SittingState.DeAnchoring;
                            }
                            break;
                        case SittingState.Anchoring:
                        case SittingState.DeAnchoring:
                            // Detect transitioning out from anchoring states
                            if (_anchoringTimer >= anchoringDuration)
                            {
                                if (SittingState == SittingState.Anchoring)
                                {
                                    SittingState = SittingState.Sitting;
                                }
                                else if (SittingState == SittingState.DeAnchoring)
                                {
                                    TransitionToState(CharacterState.Default);
                                }
                            }

                            // Keep track of time since we started anchoring
                            _anchoringTimer += deltaTime;
                            break;
                    }
                    break;
                }
            }
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            if (IgnoredColliders.Contains(coll))
            {
                return false;
            }
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    
                    break;
                }
            }
        }

        public void AddVelocity(Vector3 velocity)
        {
            switch (CurrentCharacterState)
            {
                case CharacterState.Default:
                {
                    _internalVelocityAdd += velocity;
                    break;
                }
            }
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }

        public void Sit(Transform seatingAnchor)
        {
            if(!seatingAnchor) return;
            _anchoringTimer = 0f;
            _anchoringStartPosition = Motor.TransientPosition;
            _anchoringStartRotation = Motor.TransientRotation;
            ActiveSeatingAnchor = seatingAnchor;
            TransitionToState(CharacterState.Sitting);
        }
    }
}