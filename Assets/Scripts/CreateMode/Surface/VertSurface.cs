using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using XR.SpatialAnchors;

namespace CreateMode.Surface{
public class VertSurface{
	public List<PoolableOVRSpatialAnchor> baseEdge = new();
	public float height; 

	public VertSurface(List<PoolableOVRSpatialAnchor> baseEdge, float height){
		this.baseEdge = baseEdge;
		this.height = height;
	}	

	public void Extrude(float y){
		height += y;
	}

	public List<Vector3> GetBaseEdge(){
		return baseEdge.Select((p) => p.transform.position).ToList();
	}

	public List<Vector3> GetTopEdge(){
		return baseEdge.Select((p) => 
				new Vector3(
					p.transform.position.x,
					p.transform.position.y + height,
					p.transform.position.z)).ToList();
	}

	public void Dispose(){
		baseEdge.ForEach((p) => p.Despawn());
		baseEdge = new();
	}
}
}
