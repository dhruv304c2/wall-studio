using UnityEngine;
using Zenject;

namespace Create.Preview{
public class SurfaceAnchorPreview : MonoBehaviour, IPoolable<IMemoryPool> {
	[SerializeField] LineRenderer edgeLine;
	[SerializeField] LineRenderer surfaceLine;

	Transform _connectedEdgePoint;
	Transform _connectedBasePoint;

	IMemoryPool _pool;

        public void OnDespawned(){
		gameObject.SetActive(false);
		_connectedEdgePoint = null;
		_connectedBasePoint = null;
        }

        public void OnSpawned(IMemoryPool pool){
		_pool = pool;
		gameObject.SetActive(true);
        }

        public void SetConnectedEdgeTransform(Transform transform){
		_connectedEdgePoint = transform;
	}

	public void SetConnectedBasePointTransform(Transform transform){
		_connectedBasePoint = transform;	
	}

	public void Despawn(){
		_pool.Despawn(this);
	}

	void Update(){
		if(_connectedEdgePoint == null){
			edgeLine.enabled = false;
		} else{
			edgeLine.positionCount = 2;
			edgeLine.enabled = true;
			edgeLine.SetPosition(1, transform.position);
			edgeLine.SetPosition(0, _connectedEdgePoint.position);
		}

		if(_connectedBasePoint == null){
			surfaceLine.enabled = false;	
		} else {
			surfaceLine.positionCount = 2;
			surfaceLine.enabled = true;
			surfaceLine.SetPosition(1, transform.position);
			surfaceLine.SetPosition(0, _connectedBasePoint.position);
		}

	}

	public class Pool : MemoryPool<SurfaceAnchorPreview>{
		protected override void OnCreated(SurfaceAnchorPreview item){
			item.OnDespawned();	
		}

		protected override void OnDespawned(SurfaceAnchorPreview item){
			item.OnDespawned();
		}
		
		protected override void OnSpawned(SurfaceAnchorPreview item){
			item.OnSpawned(this);
		}
	}
}
}

