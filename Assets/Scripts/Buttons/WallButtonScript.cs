using System;
using System.Collections;
using UnityEngine;

namespace Buttons {
    public class WallButtonScript : Interactable.Interactable
    {
        [Header("References")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private Transform spawnPos;
        [SerializeField] private Transform buttonTransform;

        [Header("Settings")]
        [SerializeField] private float turnAmount = 15f; 
        [SerializeField] private float turnDuration = 0.15f; // speed

        private Coroutine _coroutine;

        public bool Clicked { get; private set; }
        
        public override void Interact()
        {
            if (_coroutine != null) return;
            Clicked = true;
            Instantiate(prefab, spawnPos.position, Quaternion.identity);
            _coroutine = StartCoroutine(VisualButtonClick());
        }

        private IEnumerator VisualButtonClick()
        {
            Quaternion startRot = buttonTransform.localRotation;
            Quaternion endRot = startRot * Quaternion.Euler(turnAmount, 0f, 0f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / turnDuration;
                buttonTransform.localRotation = Quaternion.Lerp(startRot, endRot, t);
                yield return null;
            }
            _coroutine = null;
        }

        public void LateUpdate() {
            Clicked = false;
        }
    }
}
