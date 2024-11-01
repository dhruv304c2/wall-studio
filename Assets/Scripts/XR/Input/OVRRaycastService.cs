using System;
using UniRx;
using UnityEngine;

namespace XR.Input{
public class OVRRaycastService : IOVRRaycastService{
    float _maxRayDistance;

    public OVRRaycastService(float maxRayDistance){
        _maxRayDistance = maxRayDistance;
    }

    public void SubscribeToControllerRaycastWhileTriggerHeld(Action<OVRRaycastEvent> listener, LayerMask raycastLayerMask){
        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.RTouch));

        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.LTouch));
    }


    public void SubscribeToControllerRaycastWhenTriggerReleased(Action<OVRRaycastEvent> listener, LayerMask raycastLayerMask){
        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            .Skip(1)
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.RTouch));

        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            .Skip(1)
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.LTouch));
    }


    void CheckAndEmitRaycastEvent(Action<OVRRaycastEvent> listener, LayerMask raycastLayerMask, OVRInput.Controller controller){
        var controllerName = controller == OVRInput.Controller.RTouch ? "Right touch" : "Left touch";
        Debug.Log($"Performing controller raycast from {controller} controller");

        var raycastEvent = PerformRaycast(controller, raycastLayerMask);
        listener.Invoke(raycastEvent); 
    }

    OVRRaycastEvent PerformRaycast(OVRInput.Controller controller, LayerMask raycastLayerMask) {
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(controller);
        Quaternion controllerRotation = OVRInput.GetLocalControllerRotation(controller);
        Vector3 controllerForward = controllerRotation * Vector3.forward;

        var raycastEvent = new OVRRaycastEvent(){
            controller = controller,
            controllerPosition = controllerPosition,
            controllerRotation = controllerRotation
        };
 

        RaycastHit hit;
        if (Physics.Raycast(controllerPosition, controllerForward, out hit, _maxRayDistance, raycastLayerMask)) {
            Debug.DrawRay(controllerPosition, controllerForward * hit.distance, Color.red);
            raycastEvent.raycastHit = hit;
        }
        else {
            Debug.DrawRay(controllerPosition, controllerForward * _maxRayDistance, Color.green);
            raycastEvent.raycastHit = null;
        }

        return raycastEvent;
    }

}

public class OVRRaycastEvent{
    public RaycastHit? raycastHit;
    public OVRInput.Controller controller;
    public Vector3 controllerPosition;
    public Quaternion controllerRotation;
}
}
