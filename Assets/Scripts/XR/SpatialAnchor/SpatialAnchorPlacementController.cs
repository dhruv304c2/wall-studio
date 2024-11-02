using UnityEngine;
using Zenject;
using XR.Input;
using Cysharp.Threading.Tasks;
using System;

namespace XR.SpatialAnchors{
public class SpatialAnchorPlacementController : MonoBehaviour, ISpatialAnchorPlacementController {
    [Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
    [Inject] IOVRRaycastService _ovrRaycastService;

    [SerializeField] LayerMask raycastLayerMask;
    [SerializeField] LineRenderer3D line;
    [SerializeField] GameObject anchorPreview;

    public bool _spatialAnchorRequested = true; //TODO: This should be false by default and be set to true when a spatial anchor is requested

    Action<PoolableOVRSpatialAnchor> onAnchorSpawn;

    public async UniTask<PoolableOVRSpatialAnchor> WaitNextSpatialAnchor(){
        _spatialAnchorRequested = true;
        PoolableOVRSpatialAnchor spawnedAnchor = null;
        onAnchorSpawn += (spawned) => spawnedAnchor = spawned;
        await UniTask.WaitWhile(() => spawnedAnchor == null);
        _spatialAnchorRequested = false;
        return spawnedAnchor;
    }

    void Start(){
        HidePreview();

        _ovrRaycastService.SubscribeToControllerRaycastWhileTriggerHeld(OnRaycastTriggerHeld, raycastLayerMask);
        _ovrRaycastService.SubscribeToControllerRaycastWhenTriggerReleased(OnRaycastTriggerReleased, raycastLayerMask);
        _ovrRaycastService.SubscribeToControllerRaycastCancel(OnCancel);
    }

    void OnCancel(){
        HidePreview();
    }

    void OnRaycastTriggerReleased(OVRRaycastEvent e){
        if(!_spatialAnchorRequested || e.raycastHit == null) {
            return;
        }

        Debug.Log("Checking raycast for placing spatial anchor");
        if(e == null) return;
        Debug.Log($"Raycast hit at x:{e.raycastHit?.point.x},y:{e.raycastHit?.point.y},z:{e.raycastHit?.point.z}");
        var anchor = _spatialAnchorPool.Spawn();
        var hitPoint = e.raycastHit?.point;
        anchor.transform.position = (Vector3)hitPoint;

        onAnchorSpawn?.Invoke(anchor);
        HidePreview();
    }

    void OnRaycastTriggerHeld(OVRRaycastEvent e){
        if(!_spatialAnchorRequested || e.raycastHit == null){
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
