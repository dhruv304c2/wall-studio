using UnityEngine;
using Zenject;

namespace Create.Preview{
[RequireComponent(typeof(LineRenderer))]
public class PoolabeSurfaceLineRenderer : MonoBehaviour, IPoolable<IMemoryPool>{
    IMemoryPool _pool;
    public LineRenderer lineRenderer;

    public void OnDespawned(){
	gameObject.SetActive(false);
    }

    public void OnSpawned(IMemoryPool pool){
	_pool = pool;
	lineRenderer = GetComponent<LineRenderer>();
	gameObject.SetActive(true);
    }

    public void Despawn(){
	_pool.Despawn(this);
    }

    public class Pool: MemoryPool<PoolabeSurfaceLineRenderer>{
    }
}
}

