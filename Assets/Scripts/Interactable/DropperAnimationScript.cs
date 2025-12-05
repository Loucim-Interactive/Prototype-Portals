using System;
using System.Collections;
using System.Collections.Generic;
using Buttons;
using UnityEngine;

namespace Interactable {
    public class DropperAnimationScript : MonoBehaviour
    {
        [SerializeField] private WallButtonScript wallButtonScript;
        [SerializeField] private List<Transform> joints;
        [SerializeField] private float rotAmount = 20f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private float openTime = 3f;

        private List<JointRotations> _jointMaps;
        private Coroutine _coroutine;
        private float _timer = 0f;

        private bool _closed = true;
            
        private class JointRotations {
            public Transform Transform;
            public Quaternion InitialRot;

            public JointRotations(Transform t, Quaternion initialRot) {
                Transform = t;
                InitialRot = initialRot;
            }
        }

        private void Awake() {
            _jointMaps = new List<JointRotations>();
            
            foreach (var joint in joints) {
                _jointMaps.Add(new JointRotations(joint, joint.localRotation));
            }
        }

        public void Update() {
            if (!_closed) {
                _timer += Time.deltaTime;
            }
            
            if (wallButtonScript.Clicked && _coroutine == null) {
                _coroutine = StartCoroutine(OpenDropper());
                Debug.Log("Opening dropper");
                _timer = 0f;
                _closed = false;
            }

            if (_timer > openTime && _coroutine == null) {
                _coroutine = StartCoroutine(CloseDropper());
                Debug.Log("Closing dropper");
                _timer = 0f;
                _closed = true;
            }
        }

        public IEnumerator OpenDropper()
        {
            float t = 0f;

            List<Quaternion> targetRotations = new List<Quaternion>();
            foreach (var j in _jointMaps) {
                Quaternion target = j.InitialRot * Quaternion.Euler(0f, -rotAmount, 0f);
                targetRotations.Add(target);
            }

            while (t < 1f)
            {
                t += Time.deltaTime * speed;
                float tt = Mathf.Clamp01(t);

                float smoothT = 0.5f * (1f - Mathf.Cos(tt * Mathf.PI));

                for (int i = 0; i < _jointMaps.Count; i++) {
                    var j = _jointMaps[i];
                    j.Transform.localRotation = Quaternion.Lerp(j.InitialRot, targetRotations[i], smoothT);
                }

                yield return null;
            }
            _coroutine = null;
        }


        public IEnumerator CloseDropper()
        {
            float t = 0f;

            // current rotations are targets, reset to initial
            List<Quaternion> startRot = new List<Quaternion>();
            foreach (var j in _jointMaps)
                startRot.Add(j.Transform.localRotation);

            while (t < 1f) {
                t += Time.deltaTime * speed;

                for (int i = 0; i < _jointMaps.Count; i++) {
                    var j = _jointMaps[i];
                    j.Transform.localRotation = Quaternion.Lerp(startRot[i], j.InitialRot, t);
                }

                yield return null;
            }
            _coroutine = null;
        }

        [ContextMenu("Open Dropper")]
        private void StartOpen() {
            StartCoroutine(OpenDropper());
        }
        
        [ContextMenu("Close Dropper")]
        private void StartClose() {
            StartCoroutine(CloseDropper());
        }
    }
}
