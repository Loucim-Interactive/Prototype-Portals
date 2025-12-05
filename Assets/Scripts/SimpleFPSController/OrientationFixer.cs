using UnityEngine;

namespace SimpleFPSController {
    public class OrientationFixer : MonoBehaviour
    {
        public float fixSpeed = 3f;
        private bool correcting = false;

        public void BeginCorrection() => correcting = true;

        void LateUpdate()
        {
            if (!correcting) return;
            Debug.Log("Fixing orientation");

            // 1. Find the forward direction projected on the world horizontal plane
            Vector3 forward = Vector3.ProjectOnPlane(transform.forward, Vector3.up);
            if (forward.sqrMagnitude < 0.01f)
                forward = transform.forward; // fallback

            // 2. Build an upright rotation that preserves yaw, removes tilt/roll
            Quaternion desired = Quaternion.LookRotation(forward, Vector3.up);

            // 3. Smoothly rotate toward that upright version
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desired,
                Time.deltaTime * fixSpeed
            );

            // 4. Stop when close enough
            if (Quaternion.Angle(transform.rotation, desired) < 0.5f)
            {
                transform.rotation = desired;
                correcting = false;
            }
        }
    }
}