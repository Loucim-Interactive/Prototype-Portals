using Portals;
using UnityEngine;
using UnityEngine.Rendering;

public class MainCameraScript : MonoBehaviour {
    [Header("References")]
    [SerializeField] private Portal[] portals;
    
    [Header("Settings")]
    [SerializeField] private bool debug;

    void Awake () {
        portals = FindObjectsByType<Portal>(FindObjectsSortMode.None);
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
        for (int i = 0; i < portals.Length; i++) {
            //portals[i].Render(context);
            portals[i].Render();
        }

        // for (int i = 0; i < portals.Length; i++) {
        //     portals[i].PostPortalRender ();
        // }

    }
}

