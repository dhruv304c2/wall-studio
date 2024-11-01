using Zenject;
using UnityEngine;

namespace XR.FloorPlane{
public class FloorPlaneInstaller : MonoInstaller<FloorPlaneInstaller>{
    [SerializeField] int floorPlaneWidth;
    [SerializeField] int floorPlaneHeight;
    [SerializeField] LayerMask floorLayer;

    public override void InstallBindings(){
        var floorPlane = new ImplementedFloorPlane(floorPlaneWidth, floorPlaneHeight, floorLayer);
        Container.Bind<IFloorPlane>().FromInstance(floorPlane).AsSingle();
        floorPlane.Initialize();
    }
}
}

