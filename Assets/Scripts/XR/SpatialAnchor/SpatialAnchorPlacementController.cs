using UnityEngine;
using Zenject;
using XR.Input;

namespace XR.SpatialAnchors{
public class SpatialAnchorPlacementController : MonoBehaviour {
    [Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
    [Inject] IOVRRaycastService _ovrRaycastService;
    [SerializeField] LayerMask raycastLayerMask;

    void Start(){
        _ovrRaycastService.SubscribeToControllerRaycastObserver(DebugOnHit, raycastLayerMask);
    }

    void DebugOnHit(RaycastHit? hit){
        Debug.Log("Checking raycast for placing spatial anchor");
        if(hit == null) return;
        Debug.Log($"Raycast hit at x:{hit?.point.x},y:{hit?.point.y},z:{hit?.point.z}");
        var anchor = _spatialAnchorPool.Spawn();
        var hitPoint = hit?.point;
        anchor.transform.position = (Vector3)hitPoint;
    }
}
}
