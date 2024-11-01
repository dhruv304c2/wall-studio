using UnityEngine;
using Zenject;

namespace XR.Input{
    public class XRInputInstaller : MonoInstaller<XRInputInstaller> {
        [SerializeField] float maxRayDistance;

        public override void InstallBindings(){
        
            //OVR Raycast service
            Container.Bind<IOVRRaycastService>()
                .To<OVRRaycastService>()
                .FromInstance(new OVRRaycastService(maxRayDistance))
                .AsSingle();
        }
    }
}

