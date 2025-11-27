using System;
using UnityEngine;

namespace SimpleFPSController {
    public class InteractScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera cam;

        [Header("Settings")]
        [SerializeField] private float interactDistance;
        [SerializeField] private LayerMask interactableMask;

        [SerializeField] private Transform camT;

        private void Awake() {
            cam = Camera.main;
            if (cam) camT = cam.transform;
            else camT = transform; //fallback
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E)) return;
            Debug.Log("Trying to interact");
            RaycastHit hit;

            bool didHit = Physics.Raycast(
                camT.position,
                camT.forward,
                out hit,
                interactDistance,
                interactableMask
            );

            if (!didHit) return;
            var interactable = hit.collider.GetComponent<Interactable.Interactable>();
            if (!interactable) return;
            
            interactable.Interact();
        }

    }
}
