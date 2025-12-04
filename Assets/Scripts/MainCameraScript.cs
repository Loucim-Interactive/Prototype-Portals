using System.Collections.Generic;
using System.Linq;
using Portals;
using UnityEngine;
using UnityEngine.Rendering;

public class MainCameraScript : MonoBehaviour {
    [Header("References")]
    [SerializeField] private List<Portal> portals;
    
    [Header("Settings")]
    [SerializeField] private bool debug;

    void Awake () {
        portals = FindObjectsByType<Portal>(FindObjectsSortMode.None).ToList();
        //RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
    }

    private void OnDisable()
    {
        //RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
    }

    void LateUpdate() {

        // for (int i = 0; i < portals.Length; i++) {
        //     portals[i].PrePortalRender ();
        // }
        for (int i = 0; i < portals.Count; i++) {
            //portals[i].Render(context);
            portals[i].Render();
        }

        // for (int i = 0; i < portals.Length; i++) {
        //     portals[i].PostPortalRender ();
        // }

    }

    public void AddPortal(Portal portal) {
        portals.Add(portal);
    }
    
    public void RemovePortal(Portal portal) {
        if (!portals.Contains(portal)) return;
        portals.Remove(portal);
    }
}

