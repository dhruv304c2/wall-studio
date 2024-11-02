using System;
using UnityEngine;

namespace XR.Input{
public interface IOVRRaycastService {
    public void SubscribeToControllerRaycastWhileTriggerHeld(Action<OVRRaycastEvent> listener, LayerMask raycastLayermask);
    public void SubscribeToControllerRaycastWhenTriggerReleased(Action<OVRRaycastEvent> listener, LayerMask raycastLayermase);
    public void SubscribeToControllerRaycastCancel(Action listener);
}
}

