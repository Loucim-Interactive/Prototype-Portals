using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Door {
    public class DoorScript : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform spinnerLeft;
        [SerializeField] private Transform spinnerRight;
        [SerializeField] private Transform doorLeft;
        [SerializeField] private Transform doorRight;
        
        [SerializeField] private List<Collider> doorColliders;

        [Header("Settings")]
        [SerializeField] private float doorSpeed = 1.5f;         // smoothing factor for sliding (higher -> faster)
        [SerializeField] private float spinnerDegreesPerSecond = 180f; // degrees per second for spinner motion
        [SerializeField] private float slideDist = 1.5f;        // how far doors slide in local X
        [SerializeField] private float waitTime = 0.25f;        // how far doors slide in local X

        // cached initial states (LOCAL space)
        private Quaternion _initialRotLeftSpinner;
        private Quaternion _initialRotRightSpinner;
        private Vector3 _initialLocalLeftDoor;
        private Vector3 _initialLocalRightDoor;

        // internal state
        private bool _isBusy = false;    // prevents overlapping coroutines
        private bool _openDoors = false; // current logical state

        // spinner angle variables (in degrees, relative to initial rotation)
        // We drive rotation via these signed angles to guarantee directionality.
        private float _currentLeftAngle = 0f;
        private float _currentRightAngle = 0f;

        private void Awake()
        {
            // store local transforms so prefab rotation doesn't break logic
            _initialLocalLeftDoor  = doorLeft.localPosition;
            _initialLocalRightDoor = doorRight.localPosition;

            _initialRotLeftSpinner  = spinnerLeft.localRotation;
            _initialRotRightSpinner = spinnerRight.localRotation;

            // current angles start at 0 (representing the "closed" spinner state)
            _currentLeftAngle  = 0f;
            _currentRightAngle = 0f;
            
            doorColliders = gameObject.GetComponentsInChildren<Collider>().ToList();
        }

        // ---------- Public API ----------

        /// <summary>
        /// Open sequence: rotate spinners to "open" then slide doors out.
        /// </summary>
        public IEnumerator OpenDoor()
        {
            if (_isBusy) yield break;
            _isBusy = true;

            // 1) Spin spinners to the "open" angle first (e.g. 180)
            yield return StartCoroutine(RotateSpinnersTo(180f));
            yield return new WaitForSeconds(waitTime);
            // 2) Slide doors out (open)
            yield return StartCoroutine(SlideDoors(true));

            _isBusy = false;
            SetColliderTriggers(true);
        }

        /// <summary>
        /// Close sequence: slide doors closed first, then rotate spinners back to zero.
        /// </summary>
        public IEnumerator CloseDoor()
        {
            if (_isBusy) yield break;
            _isBusy = true;

            // 1) Slide doors closed first
            yield return StartCoroutine(SlideDoors(false));

            yield return new WaitForSeconds(waitTime);
            // 2) Then rotate spinners back to 0 (locked)
            yield return StartCoroutine(RotateSpinnersTo(0f));

            _isBusy = false;
            SetColliderTriggers(false);
        }

        // ---------- Internal coroutines ----------

        /// <summary>
        /// Slides doors to open (opening = true) or closed (opening = false).
        /// Uses local positions and Lerp until within threshold.
        /// </summary>
        private IEnumerator SlideDoors(bool opening)
        {
            Vector3 targetLeft  = opening
                ? _initialLocalLeftDoor  + Vector3.left  * -slideDist
                : _initialLocalLeftDoor;

            Vector3 targetRight = opening
                ? _initialLocalRightDoor + Vector3.right * -slideDist
                : _initialLocalRightDoor;

            // we lerp the localPosition each frame until we reach the target
            while (true)
            {
                doorLeft.localPosition  = Vector3.Lerp(doorLeft.localPosition,  targetLeft,  Time.deltaTime * doorSpeed);
                doorRight.localPosition = Vector3.Lerp(doorRight.localPosition, targetRight, Time.deltaTime * doorSpeed);

                float leftDist  = (doorLeft.localPosition  - targetLeft).sqrMagnitude;
                float rightDist = (doorRight.localPosition - targetRight).sqrMagnitude;

                // threshold: small distance
                if (leftDist < 0.0005f && rightDist < 0.0005f)
                {
                    doorLeft.localPosition  = targetLeft;
                    doorRight.localPosition = targetRight;
                    _openDoors = opening;
                    yield break;
                }

                yield return null;
            }
        }
        
        private IEnumerator RotateSpinnersTo(float targetAngle)
        {
            float targetLeft  = targetAngle;
            float targetRight = targetAngle;
            while (true)
            {
                // move angles towards targets with a constant degrees/sec speed
                _currentLeftAngle  = Mathf.MoveTowards(_currentLeftAngle,  targetLeft,  spinnerDegreesPerSecond * Time.deltaTime);
                _currentRightAngle = Mathf.MoveTowards(_currentRightAngle, targetRight, spinnerDegreesPerSecond * Time.deltaTime);

                // apply the angles relative to initial rotations (local rotation)
                spinnerLeft.localRotation  = _initialRotLeftSpinner  * Quaternion.Euler(0f, 0f, _currentLeftAngle);
                spinnerRight.localRotation = _initialRotRightSpinner * Quaternion.Euler(0f, 0f, _currentRightAngle);

                bool done = Mathf.Approximately(_currentLeftAngle,  targetLeft)
                         && Mathf.Approximately(_currentRightAngle, targetRight);

                if (done)
                {
                    // snap exact values to avoid tiny residuals
                    spinnerLeft.localRotation  = _initialRotLeftSpinner  * Quaternion.Euler(0f, 0f, targetLeft);
                    spinnerRight.localRotation = _initialRotRightSpinner * Quaternion.Euler(0f, 0f, targetRight);
                    yield break;
                }

                yield return null;
            }
        }

        private void SetColliderTriggers(bool trigger) {
            foreach (var col in doorColliders) {
                col.isTrigger = trigger;
            }
        }
    }
}
