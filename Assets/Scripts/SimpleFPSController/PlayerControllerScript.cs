using System;
using Portals;
using UnityEngine;
using UnityEngine.Serialization;

namespace SimpleFPSController {
    public class PlayerControllerScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerLocomotionInputScript input;
        [SerializeField] private Transform cameraPivot; 
        [SerializeField] private Transform groundCheckPos; 
        [SerializeField] private OrientationFixer fixer; 
        [SerializeField] private Traveller traveller; 
        private CharacterController _controller;
        private Rigidbody _rb;

        [Header("Movement Settings")]
        [SerializeField] private float moveForce = 5f;
        [SerializeField] private float sprintFactor = 3f;
        [FormerlySerializedAs("jumpHeight")] [SerializeField] private float jumpForce = 1.2f;
        [SerializeField] private float gravity = -20f;

        [Header("Look Settings")]
        [SerializeField] private float sensitivity = 1.5f;
        [SerializeField] private float pitchLimit = 85f;

        [Header("Ground check Settings")]
        [SerializeField] private float groundCheckRadius = 0.3f;
        [SerializeField] private float groundCheckDistance = 0.2f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector3 offset;
        [SerializeField] private bool debug;
        
        private Vector3 _velocity;
        private float _pitch = 0f;
        
        public bool IsGrounded { get; private set; }
        public Vector3 GroundNormal { get; private set; }

        private void Awake() {
            input = GetComponent<PlayerLocomotionInputScript>();
            _controller = GetComponent<CharacterController>();
            _rb = GetComponent<Rigidbody>();
            traveller = GetComponent<Traveller>();
            fixer = GetComponent<OrientationFixer>();

            if (cameraPivot == null) {
                Camera cam = Camera.main;
                if (cam != null) cameraPivot = cam.transform;
            }

            if (groundCheckPos == null) {
                groundCheckPos = transform;
            }
        }

        private void Update() {
            CheckGround();
            HandleLook();
            HandleMovement();

            if (traveller.WasTeleported) fixer.BeginCorrection();
        }

        // --------------------
        //     LOOK SYSTEM
        // --------------------
        private void HandleLook() {
            Vector2 look = input.LookInput * sensitivity;

            // Horizontal -> rotate body
            transform.Rotate(Vector3.up, look.x);

            // Vertical -> pitch camera
            _pitch -= look.y;
            _pitch = Mathf.Clamp(_pitch, -pitchLimit, pitchLimit);

            if (cameraPivot) cameraPivot.localRotation = Quaternion.Euler(_pitch, 0, 0);
        }

        // --------------------
        //   MOVEMENT SYSTEM
        // --------------------
        private void HandleMovement() {
            Vector2 moveInput = input.MovementInput;

            float speed = input.Sprint ? moveForce * sprintFactor : moveForce ;

            Vector3 move = (transform.forward * moveInput.y + transform.right * moveInput.x).normalized;
            //_controller.Move(move * (speed * Time.deltaTime));

            // Apply gravity
            // if (IsGrounded && _velocity.y < 0)
            //     _velocity.y = -2f;

            // Jump

            if (input.Jump) {
                //if (IsGrounded) _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                if (IsGrounded) _rb.AddForce(jumpForce * Vector3.up, ForceMode.Impulse);
                else Debug.Log("Cant jump, not grounded!");
            }

            _rb.AddForce(move * speed, ForceMode.Force);
            //_velocity.y += gravity * Time.deltaTime;
            //_controller.Move(_velocity * Time.deltaTime);
        }
        
        private void CheckGround()
        {
            Vector3 origin = groundCheckPos.position + offset;

            bool overlap = Physics.CheckSphere(
                origin,
                groundCheckRadius,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

            if (overlap) {
                IsGrounded = true;
                GroundNormal = Vector3.up; 
                return;
            }
            
            IsGrounded = Physics.SphereCast(
                origin,
                groundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                groundCheckDistance,
                groundLayer,
                QueryTriggerInteraction.Ignore
            );

            if (IsGrounded)
                GroundNormal = hit.normal;
            else
                GroundNormal = Vector3.up;
        }

        private void OnDrawGizmosSelected() {
            if (!debug) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Vector3 origin = groundCheckPos ? groundCheckPos.position + offset : transform.position + offset;
            Gizmos.DrawWireSphere(origin, groundCheckRadius);
        }
    }
}
