using UnityEngine;
using Zenject;

namespace CreateMode.Preview{
[RequireComponent(typeof(LineRenderer))]
public class PoolableSurfaceLineRenderer : MonoBehaviour, IPoolable<IMemoryPool>{
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

    public class Pool: MemoryPool<PoolableSurfaceLineRenderer>{
		protected override void OnCreated(PoolableSurfaceLineRenderer item){
			item.OnDespawned();	
		}

		protected override void OnDespawned(PoolableSurfaceLineRenderer item){
			item.OnDespawned();
		}
		
		protected override void OnSpawned(PoolableSurfaceLineRenderer item){
			item.OnSpawned(this);
		}
    }
}
}

