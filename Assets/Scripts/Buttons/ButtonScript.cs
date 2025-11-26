using System;
using Door;
using UnityEngine;

namespace Buttons {
    public class ButtonScript : MonoBehaviour
    {
        public Transform buttonTransform;
        public float activatedThreshold = 0.1f;
        public DoorScript[] doors;
        
        private bool _wasActivated = false;

        private void Awake() {
            doors = FindObjectsByType<DoorScript>(FindObjectsSortMode.None);
        }

        void Update()
        {
            if (buttonTransform.localPosition.y < activatedThreshold) {
                if (!_wasActivated) ActivateButton(true);
                _wasActivated = true;
            }
            else {
                if (_wasActivated) ActivateButton(false);
                _wasActivated = false;
            }
        }

        void ActivateButton(bool open) {
            Debug.Log("Activating button");
            foreach (var door in doors) {
                StartCoroutine(open ? door.OpenDoor() : door.CloseDoor());
            }
        }
    }
}
