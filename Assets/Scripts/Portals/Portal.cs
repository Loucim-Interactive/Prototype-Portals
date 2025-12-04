using System;
using System.Collections.Generic;
using UnityEngine;

namespace Portals {
    public class Portal : MonoBehaviour
    {
        #region Variables
        [Header("References")]
        [SerializeField] private Portal linkedPortal;
        [SerializeField] private MeshRenderer screen;

        [SerializeField] private Camera playerCam;
        [SerializeField] private Camera portalCam;
        
        [SerializeField] private List<Traveller> travellers = new List<Traveller>();

        
        [Header("Settings")]
        [SerializeField] private Color color;
        [SerializeField] private const float CrossingThreshold = 0.05f;
        [SerializeField] private bool isOvalPortal;
        [SerializeField] private LayerMask travellerColExcludeLayer;

        private RenderTexture _viewTexture;

        private Transform _debugT;
        private Matrix4x4 _clonePosMatrix;
        private Vector3 _initialScreenScale;
        private Vector3 _portalArea;
        #endregion

        #region Unity Methods
        
        private void Awake() {
            playerCam = Camera.main;
            portalCam = GetComponentInChildren<Camera>(true);
            _portalArea = GetComponent<Collider>().bounds.extents;
            portalCam.enabled = false;
            _debugT = transform;
            _initialScreenScale = screen.transform.localScale;
        }

        private void Update() {
            if ((playerCam.transform.position - transform.position).magnitude > _portalArea.magnitude) 
                ResetScreenPosAndThickness();
        }

        private void LateUpdate() {
            for (int i = 0; i < travellers.Count; i++) {
                var traveller = travellers[i];
                HandleTraveller(traveller);
            }
        }

        private void OnTriggerEnter(Collider other) {
            var traveller = other.GetComponent<Traveller>();
            if (traveller) {
                OnTravellerEnter(traveller);
                Debug.Log(traveller.name + " entered");
            }
        }

        private void OnTriggerExit(Collider other) {
            var traveller = other.GetComponent<Traveller>();
            if (traveller) {
                OnTravellerExit(traveller);
                Debug.Log(traveller.name + " exited");
            }
        }
        
        #endregion

        #region  PUBLIC API
        
        public bool IsLinked => linkedPortal != null;
        
        public void Render() {
            if (!linkedPortal) return;
            
            screen.enabled = false;
            CreateViewTexture();
            
            var m = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * playerCam.transform.localToWorldMatrix;
            portalCam.transform.SetPositionAndRotation(m.GetColumn(3), m.rotation);
            FixNearClipPlane();
            portalCam.Render();
            screen.enabled = true;
        }

        public void SetLinked(Portal linked) {
            linkedPortal = linked;
            SyncMaterial();
        }
        
        #endregion

        #region Traveller Logic
        
        private void HandleTraveller(Traveller traveller) {
            Transform travellerT = traveller.transform;
            Vector3 currentOffset = travellerT.position - transform.position;
            _debugT = travellerT;

            var prevSide = SideOfPortal(traveller.PrevOffset);
            var currentSide = SideOfPortal(currentOffset);
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            bool hasCrossed = (currentSide.side != prevSide.side); //&& Mathf.Abs(currentSide.dot) > CrossingThreshold;
            
            if (hasCrossed && !traveller.WasTeleported) {
                traveller.SetWasTeleported(true);
                traveller.Teleport(transform, linkedPortal.transform, m.GetColumn(3), m.rotation);
                Debug.Log("Teleported player!");
                linkedPortal.OnTravellerEnter(traveller);
                travellers.Remove(traveller);
            }
            else {
                traveller.SetWasTeleported(false);
                traveller.SetClonePosAndRot(m.GetColumn(3), m.rotation);
            }
            traveller.SliceTravellerAndClone(currentSide.side, linkedPortal.transform, transform);
            traveller.SetPrevOffset(currentOffset);
        }

        private void OnTravellerEnter(Traveller traveller) {
            if (!travellers.Contains(traveller)) {
                traveller.EnterVisualTeleportation();
                traveller.ExcludeLayerTravellerColliders(travellerColExcludeLayer);
                ProtectScreenFromClipping(playerCam.transform.position);
                traveller.SetPrevOffset(traveller.transform.position - transform.position);
                travellers.Add(traveller);
            }
        }
        
        private void OnTravellerExit(Traveller traveller) {
            if (travellers.Contains(traveller)) {
                traveller.ExitVisualTeleportation();
                traveller.ResetExcludeTravellerColliders(travellerColExcludeLayer);
                traveller.ResetMaterials();
                travellers.Remove(traveller);
            }
        }
        
        #endregion

        #region Visual Effect Logic
        
        private void CreateViewTexture() {
            var w = Screen.width;
            var h = Screen.height;
            if (!_viewTexture || _viewTexture.width != w || _viewTexture.height != h) {
                if (_viewTexture) _viewTexture.Release();
                _viewTexture = new RenderTexture(w, h, 24);
                portalCam.targetTexture = _viewTexture;
                linkedPortal.screen.material.SetTexture("_MainTex", _viewTexture);
            }
        }
        
        private float ProtectScreenFromClipping(Vector3 viewPoint)
        {
            var screenThickness = isOvalPortal ? ProtectOval(viewPoint) : ProtectSquare(viewPoint);
            return screenThickness;
        }

        private float ProtectSquare(Vector3 viewPoint) {
            float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * playerCam.aspect;
            float dstToNearClipPlaneCorner = new Vector3(halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
            float screenThickness = dstToNearClipPlaneCorner;

            Transform screenT = screen.transform;
            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            screenT.localScale = new Vector3(screenT.localScale.x, screenT.localScale.y, screenThickness);
            screenT.localPosition = Vector3.forward * (screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f));
            return screenThickness;
        }
        
        private float ProtectOval(Vector3 viewPoint) {
            float halfHeight = playerCam.nearClipPlane * Mathf.Tan(playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float halfWidth = halfHeight * playerCam.aspect;
            float screenThickness = 10f;
            
            Transform screenT = screen.transform;
            Vector3 scale = screenT.localScale;

            bool camFacingSameDirAsPortal = Vector3.Dot(transform.forward, transform.position - viewPoint) > 0;
            
            scale.x = screenThickness; // we use X for some reason. ( other ones squish and crush the screen) and also sum the initial amount (the model was imported badly)
            screenT.localScale = scale;
            //
            screenT.localPosition = Vector3.left * (camFacingSameDirAsPortal ? 0.2f : -0.2f); // same with left, somthing about blender surely

            return screenThickness;
        }
        
        private void FixNearClipPlane() {
            Transform clipPlane = transform;
            int dot = Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - portalCam.transform.position));
            
            Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
            Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
            float camSpaceDist = -Vector3.Dot(camSpacePos, camSpaceNormal);
            
            Vector4 clipPlaneCamSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDist);
            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCamSpace);
        }
        
        #endregion

        #region Helpers

        private (int side, float dot) SideOfPortal(Vector3 offset) {
            float dotProduct = Vector3.Dot(offset, transform.forward);
            return (Math.Sign(dotProduct), dotProduct);
        }
        
        private void ResetScreenPosAndThickness() {
            screen.transform.localScale = _initialScreenScale;
            screen.transform.localPosition = Vector3.zero;
        }

        private void SyncMaterial()
        {
            if (!linkedPortal) return;

            // THIS is the correct new instance unity uses at runtime
            var mat = linkedPortal.screen.material;

            // force the renderTexture to be assigned to the fresh instance
            if (_viewTexture)
                mat.SetTexture("_MainTex", _viewTexture);
        }
        
        #endregion

        #region Debugging
        private void OnDrawGizmos()
        {
            if (!portalCam || !_debugT) return;
            Gizmos.color = color;

            Vector3 pos = portalCam.transform.position;
            Vector3 dir = portalCam.transform.forward;

            float length = 5f;      // main arrow length
            float headSize = 0.5f;  // arrowhead size
            float headAngle = 25f;  // degrees
            
            // Main line
            Vector3 end = pos + dir * length;
            Gizmos.DrawLine(pos, end);

            // Arrowhead
            Vector3 right = Quaternion.LookRotation(dir) * Quaternion.Euler(0, headAngle, 0) * Vector3.back;
            Vector3 left  = Quaternion.LookRotation(dir) * Quaternion.Euler(0, -headAngle, 0) * Vector3.back;

            Gizmos.DrawLine(end, end + right * headSize);
            Gizmos.DrawLine(end, end + left * headSize);

            // Optional: sphere at start
            Gizmos.DrawWireSphere(pos, 0.5f);
            Gizmos.DrawFrustum(portalCam.transform.position, portalCam.fieldOfView, portalCam.farClipPlane, portalCam.nearClipPlane, portalCam.aspect);
            
            //var teleportMatrix = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * _debugT.localToWorldMatrix;
            //Gizmos.DrawCube(teleportMatrix.GetColumn(3), Vector3.one);
        }
        #endregion
    }
}
