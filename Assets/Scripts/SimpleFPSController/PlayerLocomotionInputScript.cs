using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SimpleFPSController {
    public class PlayerLocomotionInputScript : MonoBehaviour, PlayerLocomotionMap.IPlayerActions
    {
        #region Public API

        public Vector2 MovementInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool Jump { get; private set; }
        public bool Sprint { get; private set; }

        #endregion

        private PlayerLocomotionMap _map;

        private void OnEnable() {
            if (_map == null) {
                _map = new PlayerLocomotionMap();
                _map.Player.SetCallbacks(this);   
            }
            _map.Player.Enable();              
        }

        private void OnDisable() {
            _map.Player.Disable();
        }

        private void LateUpdate() {
            Jump = false;
        }
        
        public void OnMove(InputAction.CallbackContext context) {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnLook(InputAction.CallbackContext context) {
            LookInput = context.ReadValue<Vector2>();
        }

        public void OnJump(InputAction.CallbackContext context) {
            if (context.performed) {
                Jump = true;
            }
        }

        public void OnSprint(InputAction.CallbackContext context) {
            if (context.started) {
                Sprint = true;
            }

            if (context.canceled) {
                Sprint = false;
            }
        }
    }
}
