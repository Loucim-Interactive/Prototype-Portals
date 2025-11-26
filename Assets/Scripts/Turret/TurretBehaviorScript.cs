using System;
using UnityEngine;

namespace Turret {
    public class TurretBehaviorScript : MonoBehaviour {
        [Header("References")] 
        [SerializeField] private Transform _leftArm;
        [SerializeField] private Transform _rightArm;
        [SerializeField] private Transform eyePoint;    
        [SerializeField] private Transform player;     
        
        [Header("Settings")] 
        [SerializeField] private float _armDist = 0.2f;
        [SerializeField] private float _engageSpeed = 0.5f;
        [SerializeField] private float viewDistance = 20f;
        [SerializeField] private float viewAngle = 60f; 
        [SerializeField] private LayerMask obstructionMask;


        private bool _detected = false;


        private Vector3 _initialPosLeft;
        private Vector3 _initialPosRight; // initial is open
        
        private void Awake() {
            _initialPosLeft = _leftArm.localPosition;
            _initialPosRight = _rightArm.localPosition;
        }

        void Update()
        {
            _detected = DetectPlayer();

            // determine targets relative to each arm's initial local position.
            Vector3 leftClosedTarget  = _initialPosLeft  + Vector3.left  * _armDist;
            Vector3 leftOpenTarget    = _initialPosLeft;
            Vector3 rightClosedTarget = _initialPosRight + Vector3.right * _armDist;
            Vector3 rightOpenTarget   = _initialPosRight;

            float t = Time.deltaTime * _engageSpeed;

            if (!_detected) {
                _leftArm.localPosition  = Vector3.Lerp(_leftArm.localPosition,  leftClosedTarget,  t);
                _rightArm.localPosition = Vector3.Lerp(_rightArm.localPosition, rightClosedTarget, t);
            }
            else {
                _leftArm.localPosition  = Vector3.Lerp(_leftArm.localPosition,  leftOpenTarget,  t);
                _rightArm.localPosition = Vector3.Lerp(_rightArm.localPosition, rightOpenTarget, t);
            }
        }

        private bool DetectPlayer()
        {
            if (player == null || eyePoint == null) return false;

            Vector3 dirToPlayer = player.position - eyePoint.position;
            float distance = dirToPlayer.magnitude;

            if (distance > viewDistance)
                return false;

            Vector3 dirNormalized = dirToPlayer.normalized;

            float halfFov = viewAngle * 0.5f;
            float angle = Vector3.Angle(-eyePoint.up, dirNormalized);

            if (angle > halfFov)
                return false;

            if (Physics.Raycast(eyePoint.position, dirNormalized, out RaycastHit hit, viewDistance, ~0, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;
                return false;
            }

            return false;
        }


    }
}
