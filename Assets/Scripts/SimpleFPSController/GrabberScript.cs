using System;
using UnityEngine;

namespace SimpleFPSController {
    public class GrabberScript : MonoBehaviour {
        [Header("Grab Settings")]
        [SerializeField] private Transform grabPoint;        
        [SerializeField] private float grabDistance = 3f;    
        [SerializeField] private float moveSpeed = 12f;     
        [SerializeField] private float rotateSpeed = 12f;    
        [SerializeField] private LayerMask grabMask;

        [Header("Highlight")]
        [SerializeField] private Color highlightColor = Color.yellow;
        private Renderer lastHighlighted;
        private Color originalColor;

        private Camera _cam;
        private GrabbedObject _grabbed;

        private class GrabbedObject {
            public GameObject Obj;
            public Rigidbody Rb;
        }

        private void Awake() {
            _cam = Camera.main;
        }

        private void Update() {
            HandleHighlight();
            
            if (_grabbed != null && Input.GetKeyDown(KeyCode.E))
                Release();
            
            if (_grabbed == null) TryGrab();
            else MoveGrabbedObject();
        }
        
        private void HandleHighlight() {
            // Reset last highlighted if needed
            if (lastHighlighted) {
                lastHighlighted.material.color = originalColor;
                lastHighlighted = null;
            }

            if (_grabbed != null) return; // don't highlight while holding something

            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, 
                out RaycastHit hit, grabDistance, grabMask)) {

                Renderer r = hit.collider.GetComponent<Renderer>();
                if (r) {
                    lastHighlighted = r;
                    originalColor = r.material.color;
                    r.material.color = highlightColor;
                }
            }
        }


        private void TryGrab() {
            if (!Input.GetKeyDown(KeyCode.E))
                return;

            if (!Physics.Raycast(_cam.transform.position, _cam.transform.forward, 
                out RaycastHit hit, grabDistance, grabMask))
                return;

            Rigidbody rb = hit.collider.attachedRigidbody;
            if (!rb) return;
            Debug.Log("Grabbing");
            _grabbed = new GrabbedObject {
                Obj = rb.gameObject,
                Rb = rb
            };

            rb.useGravity = false;
            rb.isKinematic = false; 
        }


        private void MoveGrabbedObject() {
            Rigidbody rb = _grabbed.Rb;

            Vector3 targetPos = grabPoint.position;
            Vector3 force = (targetPos - rb.position) * moveSpeed;

            rb.linearVelocity = force;

            // Optional: smoothly rotate object to face camera direction
            Quaternion targetRot = grabPoint.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, rotateSpeed * Time.deltaTime));
        }
        
        private void Release() {
            if (_grabbed == null) return;
            Debug.Log("Releasing");
            _grabbed.Rb.useGravity = true;
            _grabbed.Rb.linearVelocity = Vector3.zero;
            _grabbed.Rb.angularVelocity = Vector3.zero;

            _grabbed = null;
        }
    }
}
