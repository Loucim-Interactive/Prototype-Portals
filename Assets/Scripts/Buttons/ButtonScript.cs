using System;
using Door;
using UnityEngine;

namespace Buttons {
    public class ButtonScript : Interactable.Interactable
    {
        [Header("References")]
        public Transform buttonTransform;
        public DoorScript[] doors;
        
        [Header("Settings")]
        public float activatedThreshold = 0.1f;
        
        private bool _wasActivated = false;

        private void Awake() {
            doors = FindObjectsByType<DoorScript>(FindObjectsSortMode.None);
        }

        private void Update()
        {
            if (buttonTransform.localPosition.y < activatedThreshold) {
                if (!_wasActivated) Interact();
                _wasActivated = true;
            }
            else {
                if (_wasActivated) Interact();
                _wasActivated = false;
            }
        }

        public override void Interact() {
            foreach (var door in doors) {
                StartCoroutine(_wasActivated ? door.CloseDoor() : door.OpenDoor());
            }
        }
    }   
}
