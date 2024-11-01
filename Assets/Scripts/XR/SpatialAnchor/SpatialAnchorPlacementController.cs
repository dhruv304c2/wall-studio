using UnityEngine;
using Zenject;
using XR.Input;

namespace XR.SpatialAnchors{
public class SpatialAnchorPlacementController : MonoBehaviour {
    [Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
    [Inject] IOVRRaycastService _ovrRaycastService;

    [SerializeField] LayerMask raycastLayerMask;
    [SerializeField] LineRenderer3D line;
    [SerializeField] GameObject anchorPreview;

    public bool _cancelPlacement = false;

    void Start(){
        HidePreview();

        _ovrRaycastService.SubscribeToControllerRaycastWhileTriggerHeld(OnRaycastTriggerHeld, raycastLayerMask);
        _ovrRaycastService.SubscribeToControllerRaycastWhenTriggerReleased(OnRaycastTriggerReleased, raycastLayerMask);
    }

    void OnRaycastTriggerReleased(OVRRaycastEvent e){
        if(_cancelPlacement || e.raycastHit == null) {
            HidePreview();
            return;
        }

        Debug.Log("Checking raycast for placing spatial anchor");
        if(e == null) return;
        Debug.Log($"Raycast hit at x:{e.raycastHit?.point.x},y:{e.raycastHit?.point.y},z:{e.raycastHit?.point.z}");
        var anchor = _spatialAnchorPool.Spawn();
        var hitPoint = e.raycastHit?.point;
        anchor.transform.position = (Vector3)hitPoint;

        HidePreview();
    }

    void OnRaycastTriggerHeld(OVRRaycastEvent e){
        if(_cancelPlacement || e.raycastHit == null){
            HidePreview();
            return;
        }

        line.UpdateLine(e.controllerPosition, (Vector3) e.raycastHit?.point);
        anchorPreview.transform.position = (Vector3) e.raycastHit?.point;
        ShowPreview();
    }

    void ShowPreview(){
        line.Show();
        anchorPreview.SetActive(true);
    }

    void HidePreview(){
        line.Hide();
        anchorPreview.SetActive(false);
    }
}
}
