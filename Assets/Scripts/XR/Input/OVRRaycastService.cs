using System;
using UniRx;
using UnityEngine;

namespace XR.Input{
public class OVRRaycastService : IOVRRaycastService{
    float _maxRayDistance;
    bool _canceled = false;
    bool _cancellationRequested = false;

    public OVRRaycastService(float maxRayDistance){
        _maxRayDistance = maxRayDistance;

        Observable
            .EveryUpdate()
            .Where(_ => OVRInput.GetDown(OVRInput.Button.Two))
            .Subscribe(_ => CancelRaycast());

        Observable
            .EveryUpdate()
            .Where(_ => _cancellationRequested)
            .Subscribe(_ => {
                _canceled  = true;
                _cancellationRequested = false;
                Debug.Log("Controller Raycast cancelled");
            });
    }

    public void CancelRaycast(){
        _cancellationRequested = true;
    }

    public void SubscribeToControllerRaycastCancel(Action listener){
        Observable
            .EveryUpdate()
            .Where(_ => _cancellationRequested) 
            .Subscribe(_ => listener.Invoke());
    }

    public void SubscribeToControllerRaycastWhileTriggerHeld(Action<OVRRaycastEvent> listener, LayerMask raycastLayerMask){
        Observable
            .EveryUpdate()
            .SkipWhile(_ => _canceled)
            .Where(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) 
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.RTouch));

        Observable
            .EveryUpdate()
            .SkipWhile(_ => _canceled)
            .Where(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            .Subscribe(_ => CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.LTouch));
    }


    public void SubscribeToControllerRaycastWhenTriggerReleased(Action<OVRRaycastEvent> listener, LayerMask raycastLayerMask){
        Observable
            .EveryUpdate()
            .SkipWhile(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch)) //Ignoring frames when left trigger is still held
            .Where(_ => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            .Subscribe(_ => {
                if(!_canceled) CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.RTouch);
                _canceled = false;
            });

        Observable
            .EveryUpdate()
            .SkipWhile(_ => OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)) //Ignoring frames when right trigger is still held
            .Where(_ => OVRInput.GetUp(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            .Subscribe(_ => {
                if(!_canceled) CheckAndEmitRaycastEvent(listener, raycastLayerMask, OVRInput.Controller.LTouch);
                _canceled = false;
            });
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
