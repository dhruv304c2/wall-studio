using System;
using UnityEngine;

namespace XR.Input{
public interface IOVRRaycastService {
    public void SubscribeToControllerRaycastObserver(Action<RaycastHit?> listener, LayerMask raycastLayermask);
}
}

