using System.Collections.Generic;
using System.Linq;
using Create.Preview;
using ModestTree;

namespace Create.Surface{
public class VertSurfaceVisualizer{
	VertSurface _surface;
	SurfaceAnchorVisualizer.Pool _surfaceAnchorPool;
	
	public VertSurface Surface => _surface;

	public VertSurfaceVisualizer(
		VertSurface surface,
		SurfaceAnchorVisualizer.Pool surfaceAnchorPool
		){

		_surface = surface;
		_surfaceAnchorPool = surfaceAnchorPool;
	}

	List<SurfaceAnchorVisualizer> _baseEdgeVisual = new();
	List<SurfaceAnchorVisualizer> _topEdgeVisual = new();

	public void Update(){
		//Dispose old preview
		Dispose();

		//Create base edge preview
		foreach(var p in _surface.GetBaseEdge()){
			var a = _surfaceAnchorPool.Spawn();
			a.transform.position = p;
			if(!_baseEdgeVisual.IsEmpty()) a.SetConnectedEdgeTransform(_baseEdgeVisual.Last().transform);
			_baseEdgeVisual.Add(a);
		}

		//Create top edge preview
		int idx = 0;
		foreach(var p in _surface.GetTopEdge()){
			var a = _surfaceAnchorPool.Spawn();
			a.transform.position = p;
			if(!_topEdgeVisual.IsEmpty()) a.SetConnectedEdgeTransform(_topEdgeVisual.Last().transform);
			a.SetConnectedBasePointTransform(_baseEdgeVisual[idx].transform);
			_topEdgeVisual.Add(a);
			idx++;
		}
	}

	public void Show(){
		_baseEdgeVisual.ForEach((p) => p.Show());
		_topEdgeVisual.ForEach((p) => p.Show());
	}

	public void Hide(){
		_baseEdgeVisual.ForEach((p) => p.Hide());
		_topEdgeVisual.ForEach((p) => p.Hide());
	}

	public void Dispose(){
		_baseEdgeVisual.ForEach((p) => p.Despawn());
		_topEdgeVisual.ForEach((p) => p.Despawn());

		_baseEdgeVisual = new();
		_topEdgeVisual = new();
	}
}
}

