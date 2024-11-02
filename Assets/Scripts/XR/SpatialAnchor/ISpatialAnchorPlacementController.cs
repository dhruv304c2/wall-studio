using Cysharp.Threading.Tasks;

namespace XR.SpatialAnchors{
    public interface ISpatialAnchorPlacementController{
        public UniTask<PoolableOVRSpatialAnchor> WaitNextSpatialAnchor();
    }
}

