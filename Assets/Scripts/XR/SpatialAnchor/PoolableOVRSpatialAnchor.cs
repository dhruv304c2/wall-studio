using UnityEngine;
using Zenject;

namespace XR.SpatialAnchors{
public class PoolableOVRSpatialAnchor : MonoBehaviour, IPoolable<IMemoryPool> {
    private OVRSpatialAnchor _spatialAnchor;
    public OVRSpatialAnchor spatialAnchor => _spatialAnchor; 

    private IMemoryPool _pool;

    public void OnDespawned() {
        if(_spatialAnchor != null)_spatialAnchor.EraseAnchorAsync();
        gameObject.SetActive(false);
    }

    public void OnSpawned(IMemoryPool pool) {
        _pool = pool;
        if(_spatialAnchor == null) _spatialAnchor = GetComponent<OVRSpatialAnchor>();
        if(_spatialAnchor != null) _spatialAnchor.SaveAnchorAsync();
        else throw new System.Exception($"OVRSpatialAnchor component anchor not found on poolable spatial anchor");
        gameObject.SetActive(true);
    }

    public void Despawn(){
        _pool.Despawn(this);
    }

    public class Pool : MemoryPool<PoolableOVRSpatialAnchor>{ 
    }
}}
