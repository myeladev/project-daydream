using ProjectDaydream.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectDaydream.Logic
{
    public class GrabController : MonoBehaviour
    {
        public Camera cam;
        public float spring = 800f;
        public float damper = 60f;

        private Rigidbody _anchorRb; // kinematic proxy that follows the mouse
        private ConfigurableJoint _joint;
        private float _grabDistance;

        private InputAction _grabAction;

        void Awake()
        {
            var go = new GameObject("GrabAnchor");
            _anchorRb = go.AddComponent<Rigidbody>();
            _anchorRb.isKinematic = true;
            _anchorRb.interpolation = RigidbodyInterpolation.Interpolate;
        }

        private void Start()
        {
            _grabAction = InputSystem.actions.FindAction("Grab");
        }

        private void FixedUpdate()
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            var target = ray.origin + ray.direction * _grabDistance;

            // Smooth towards target to avoid huge teleports
            var maxSpeed = 12f; // m/s
            var desired = target - _anchorRb.position;
            var step = Vector3.ClampMagnitude(desired, maxSpeed * Time.fixedDeltaTime);
            _anchorRb.MovePosition(_anchorRb.position + step);
        }

        void Update()
        {
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            if (_grabAction.WasPerformedThisFrame() && (!OptionsUI.Instance?.IsViewingOptions ?? false))
            {
                if (Physics.Raycast(ray, out var hit, 3f) && hit.rigidbody)
                {
                    _grabDistance = hit.distance;
                    _anchorRb.position = hit.point;

                    _joint = hit.rigidbody.gameObject.AddComponent<ConfigurableJoint>();
                    _joint.autoConfigureConnectedAnchor = false;
                    _joint.connectedBody = _anchorRb;

                    // anchor on the object at the hit point (in object local space)
                    _joint.anchor = hit.rigidbody.transform.InverseTransformPoint(hit.point);
                    _joint.connectedAnchor = Vector3.zero;

                    var jointDrive = new JointDrive() { positionSpring = spring, positionDamper = damper, maximumForce = float.MaxValue };
                    _joint.xDrive = jointDrive;
                    _joint.yDrive = jointDrive;
                    _joint.zDrive = jointDrive;
                    _joint.angularXDrive = jointDrive;
                    _joint.angularYZDrive = jointDrive;
                    _joint.xMotion = ConfigurableJointMotion.Limited;
                    _joint.yMotion = ConfigurableJointMotion.Limited;
                    _joint.zMotion = ConfigurableJointMotion.Limited;
                    _joint.linearLimit = new SoftJointLimit() { limit = 0.03f };
                    _joint.projectionMode = JointProjectionMode.PositionAndRotation;
                    
                    _joint.breakForce = 10e25f;

                    // extra stability
                    //hit.rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    //hit.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                }
            }

            if (_grabAction.WasReleasedThisFrame())
            {
                if (_joint)
                {
                    Destroy(_joint);
                    _joint = null;
                }
                _grabDistance = 0;
            }
        }
    }
}
