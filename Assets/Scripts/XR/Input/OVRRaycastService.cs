using System;
using UniRx;
using UnityEngine;

namespace XR.Input{
public class OVRRaycastService : IOVRRaycastService{
    float _maxRayDistance;

    public OVRRaycastService(float maxRayDistance){
        _maxRayDistance = maxRayDistance;
    }

    public void SubscribeToControllerRaycastObserver(Action<RaycastHit?> listener, LayerMask raycastLayerMask){
        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.RTouch));

        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.LTouch));
    }

    void CheckAndEmitRaycastEvent(Action<RaycastHit?> listener, LayerMask raycastLayerMask, OVRInput.Controller controller){
        var controllerName = controller == OVRInput.Controller.RTouch ? "Right touch" : "Left touch";
        Debug.Log($"Performing controller raycast from {controller} controller");

        var hit = PerformRaycast(controller, raycastLayerMask);
        listener.Invoke(hit); 
    }

    RaycastHit? PerformRaycast(OVRInput.Controller controller, LayerMask raycastLayerMask) {
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(controller);
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(controller);
        Vector3 controllerForward = controllerRotation * Vector3.forward;

        RaycastHit hit;
        if (Physics.Raycast(controllerPosition, controllerForward, out hit, _maxRayDistance, raycastLayerMask)) {
            Debug.DrawRay(controllerPosition, controllerForward * hit.distance, Color.red);
            return hit;
        }
        else {
            Debug.DrawRay(controllerPosition, controllerForward * _maxRayDistance, Color.green);
            return null;
        }
    }

}
}
