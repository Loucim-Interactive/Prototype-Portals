using System;
using UnityEngine;

namespace Portals {
    public class PortalGun : MonoBehaviour {
        
        [Header("References")]
        [SerializeField] private Transform gun;
        [SerializeField] private GameObject prefab;
        
        [Header("Settings")]
        [SerializeField] private LayerMask portalSurfaceLayer;

        [Header("Portals")]
        [SerializeField] private Portal portalA;
        [SerializeField] private Portal portalB;

        private void Update() {
            
            if (Input.GetMouseButtonDown(0)) {
                FirePortal(portalA);
            }
            
            if (Input.GetMouseButtonDown(1)) {
                FirePortal(portalB);
            }
        }

        private void FirePortal(Portal portal) {
            if (portal) return;
            if (GetPortalPos(out RaycastHit hit))
            {
                // mirar hacia afuera de la superficie
                Quaternion portalRot = Quaternion.LookRotation(hit.normal);

                portal.transform.position = hit.point;
                portal.transform.rotation = portalRot;

                portal.gameObject.SetActive(true);
            }        }

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
    }
}
