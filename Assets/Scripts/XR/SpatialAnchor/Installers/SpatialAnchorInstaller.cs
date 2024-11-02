using UnityEngine;
using Zenject;

namespace XR.SpatialAnchors.Installers{
public class SpatialAnchorInstaller : MonoInstaller<SpatialAnchorInstaller> {
    [SerializeField] PoolableOVRSpatialAnchor spatialAnchoPrefab;

    public override void InstallBindings(){
        Container.BindFactory<OVRSpatialAnchor,SpatialAnchorFactory>().FromComponentInNewPrefab(spatialAnchoPrefab);
        Container.BindMemoryPool<PoolableOVRSpatialAnchor, PoolableOVRSpatialAnchor.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(spatialAnchoPrefab)
            .UnderTransformGroup("SpatialAnchorPool");
    }

    public class SpatialAnchorFactory : PlaceholderFactory<OVRSpatialAnchor> {}
}
}

