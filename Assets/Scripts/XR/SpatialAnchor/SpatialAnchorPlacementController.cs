using UnityEngine;
using XR.Input;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace XR.SpatialAnchors{
public class SpatialAnchorPlacementController : ISpatialAnchorPlacementController {
    PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
    IOVRRaycastService _ovrRaycastService;

    LayerMask _raycastLayerMask;
    LineRenderer3D _line;
    GameObject _previewObj;

    public bool _spatialAnchorRequested = false;
    bool _canceled;

    Action<PoolableOVRSpatialAnchor> onAnchorSpawn;

    public async UniTask<PoolableOVRSpatialAnchor> RequestSpatialAnchorUserAsync (CancellationToken token = new()){
        _spatialAnchorRequested = true;
        PoolableOVRSpatialAnchor spawnedAnchor = null;
        onAnchorSpawn += (spawned) => spawnedAnchor = spawned;
        await UniTask.WaitUntil(() => spawnedAnchor != null || _canceled || token.IsCancellationRequested);
        _spatialAnchorRequested = false;
        _canceled = false;
        return spawnedAnchor;
    }

    public SpatialAnchorPlacementController(
                IOVRRaycastService ovrRaycastService, 
                PoolableOVRSpatialAnchor.Pool spatialAnchorPool,
                LayerMask raycastLayerMask,
                GameObject previewObj = null,
                LineRenderer3D line = null
            ){

        _ovrRaycastService = ovrRaycastService;
        _spatialAnchorPool = spatialAnchorPool;
        _raycastLayerMask = raycastLayerMask;
        _line = line;
        _previewObj = previewObj;

        HidePreview();

        _ovrRaycastService.SubscribeToControllerRaycastWhileTriggerHeld(OnRaycastTriggerHeld, _raycastLayerMask);
        _ovrRaycastService.SubscribeToControllerRaycastWhenTriggerReleased(OnRaycastTriggerReleased, _raycastLayerMask);
        _ovrRaycastService.SubscribeToControllerRaycastCancel(OnCancel);
    }

    void OnCancel(){
        HidePreview();
        _canceled = true;
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

        if(_line) _line.UpdateLine(e.controllerPosition, (Vector3) e.raycastHit?.point);
        if(_previewObj) _previewObj.transform.position = (Vector3) e.raycastHit?.point;
        ShowPreview();
    }

    void ShowPreview(){
        if(_line) _line.Show();
        if(_previewObj) _previewObj.SetActive(true);
    }

    void HidePreview(){
        if(_line) _line.Hide();
        if(_previewObj) _previewObj.SetActive(false);
    }
}
}
