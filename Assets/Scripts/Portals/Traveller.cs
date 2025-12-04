using System.Collections.Generic;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Portals {
    public class Traveller : MonoBehaviour {
        [Header("References")] 
        public GameObject cloneGraphics;
        public GameObject travellerGraphics;
        
        public Vector3 PrevOffset { get; private set; }
        public bool WasTeleported { get; private set; }
            
        public Collider[] TravellerColliders {get; private set;}
        public Material[] TravellerMaterials { get; private set; }
        public Material[] CloneMaterials {get; private set;}

        public virtual void Teleport(Transform from, Transform to, Vector3 pos, Quaternion rot) {
            transform.position = pos;
            transform.rotation = rot;
            
            Rigidbody rb = GetComponent<Rigidbody>();

            if (rb)
            {
                Vector3 localVel = from.InverseTransformDirection(rb.linearVelocity);
                Vector3 localAngVel = from.InverseTransformDirection(rb.angularVelocity);

                rb.linearVelocity = to.TransformDirection(localVel);
                rb.angularVelocity = to.TransformDirection(localAngVel);
            }
        }
        
        public void SetPrevOffset(Vector3 newOffset) {
            PrevOffset = newOffset;
        }
        
        public void SetWasTeleported(bool teleported) {
            WasTeleported = teleported;
        }

        public void EnterVisualTeleportation() {
            if (!cloneGraphics) {
                SpawnClone();
                // get materials for slicing later (visually slicing the graphics)
                TravellerMaterials = GetMaterials(travellerGraphics);
                CloneMaterials = GetMaterials(cloneGraphics);
            }
            else {
                cloneGraphics.SetActive(true);
            }
        }
        
        public void ExitVisualTeleportation() {
            cloneGraphics.SetActive(false);
        }
        
        public void SetClonePosAndRot(Vector3 pos, Quaternion rot) {
            cloneGraphics.transform.SetPositionAndRotation(pos, rot);
        }
        
        public void SliceTravellerAndClone(int side, Transform linkedPortalT, Transform portalT) {

            Vector3 slicePos = portalT.position;
            Vector3 sliceNormal = transform.forward * -side;
            
            Vector3 cloneSlicePos = linkedPortalT.position;
            Vector3 cloneSliceNormal = linkedPortalT.forward * side;
            
            for (int i = 0; i < TravellerMaterials.Length; i++) {
                TravellerMaterials[i].SetVector("_SliceCentre", slicePos);
                TravellerMaterials[i].SetVector("_SliceNormal", sliceNormal);
                
                CloneMaterials[i].SetVector("_SliceCentre", cloneSlicePos);
                CloneMaterials[i].SetVector("_SliceNormal", cloneSliceNormal);
            }
        }

        public void ExcludeLayerTravellerColliders(LayerMask layerMask) {
            var cols = GetColliders(gameObject);
            foreach (var col in cols) {
                col.excludeLayers |= layerMask; // add this layer to excluded set            
            }
        }
        
        public void ResetExcludeTravellerColliders(LayerMask layerMask) {
            var cols = GetColliders(gameObject);
            foreach (var col in cols) {
                col.excludeLayers &= ~layerMask;  // remove this layer
            }
        }

        
        public void ResetMaterials() {
            for (int i = 0; i < TravellerMaterials.Length; i++) {
                TravellerMaterials[i].SetVector("_SliceCentre", Vector3.zero);
                TravellerMaterials[i].SetVector("_SliceNormal", Vector3.zero);
                
                CloneMaterials[i].SetVector("_SliceCentre", Vector3.zero);
                CloneMaterials[i].SetVector("_SliceNormal", Vector3.zero);
            }
        }
        
        private Material[] GetMaterials(GameObject obj) {
            var renderers = obj.GetComponentsInChildren<MeshRenderer>();
            var mats = new List<Material>();

            foreach (MeshRenderer r in renderers) {
                foreach (var mat in r.materials) {
                    mats.Add(mat);
                }
            }
            return mats.ToArray();
        }
        
        private Collider[] GetColliders(GameObject obj) {
            var colliders = obj.GetComponentsInChildren<Collider>();
            var cols = new List<Collider>();

            foreach (Collider col in colliders) {
                cols.Add(col);
            }
            return cols.ToArray();
        }

        private void SpawnClone() {
            cloneGraphics = Instantiate(travellerGraphics);
            cloneGraphics.transform.SetParent(travellerGraphics.transform.parent);
            cloneGraphics.transform.localScale = travellerGraphics.transform.localScale;
        }
    }
}
