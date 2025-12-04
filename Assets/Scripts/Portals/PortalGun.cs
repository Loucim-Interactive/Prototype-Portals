using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Portals {
    public class PortalGun : MonoBehaviour {
        
        [Header("References")]
        [SerializeField] private Transform gun;
        [SerializeField] private GameObject prefab;
        [SerializeField] private MainCameraScript camScript;

        
        [Header("Settings")]
        [SerializeField] private LayerMask portalSurfaceLayer;
        [SerializeField] private float portalRot = 180;
        [SerializeField] private float offset;

        [Header("Portals")]
        [SerializeField] private Portal portalA;
        [SerializeField] private Portal portalB;

        private void Awake() {
            camScript = FindFirstObjectByType<MainCameraScript>();
        }

        private void Update() {
            
            if (Input.GetMouseButtonDown(0)) {
                FirePortal(portalA, true);
            }
            
            if (Input.GetMouseButtonDown(1)) {
                FirePortal(portalB, false);
            }

            LinkPortals();
        }

        private void FirePortal(Portal portal, bool primary)
        {
            if (!GetPortalPos(out RaycastHit hit))
                return;

            if (portal)
                DestroyPortal(portal, primary);

            GameObject newPortalObj = Instantiate(prefab);
            Portal newPortal = newPortalObj.GetComponent<Portal>();

            Vector3 pos = hit.point + hit.normal * offset;

            Quaternion rot = Quaternion.LookRotation(hit.normal);

            newPortalObj.transform.SetPositionAndRotation(pos, rot);
            if (!primary) {
                newPortalObj.transform.Rotate(0, 180, 0);
            }
            newPortalObj.name = primary ? "Primary Portal" : "Secondary Portal";
            SetupPortal(newPortal, primary);
        }


        private void SetupPortal(Portal portal, bool primary) {
            if (primary) {
                portalA = portal;
                camScript.AddPortal(portal);
                if (portalB) portalA.SetLinked(portalB);
            }
            else {
                portalB = portal;
                camScript.AddPortal(portal);
                if (portalA) portalB.SetLinked(portalA); // we add to the render list, we set the B portal to this one, and link it with A
            }
        }

        private void DestroyPortal(Portal portal, bool primary) {
            if (primary) {
                camScript.RemovePortal(portalA);
                portalA = null;
            }
            else {
                camScript.RemovePortal(portalB);
                portalB = null;
            }
            Destroy(portal.gameObject);
        }
        
        private void LinkPortals() {
            if (portalA && portalB && !portalA.IsLinked) portalA.SetLinked(portalB); // if both portals, and A not linked, link.
            if (portalB && portalA && !portalB.IsLinked) portalB.SetLinked(portalA); // if both portals, and B not linked, link.
        }
        
        private bool GetPortalPos(out RaycastHit hit)
        {
            Vector3 origin = gun.position;
            Vector3 dir = gun.forward;

            if (Physics.Raycast(origin, dir, out hit, 1000f, portalSurfaceLayer))
            {
                return true;
            }

            hit = default;
            return false;
        }

        private void OnDrawGizmos() {
            if (portalA) Debug.DrawRay(portalA.transform.position, portalA.transform.forward * 2, Color.red, 5f);
            if (portalB) Debug.DrawRay(portalB.transform.position, portalB.transform.forward * 2, Color.red, 5f);
        }
    }
}
