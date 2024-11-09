using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Create.Surface{
public class VertSurfaceController{
	public VertSurface surface;
	public VertSurfaceVisualizer visualizer;
	public Collider collider;
	public float minHeight;
	public float maxHeight;

	public VertSurfaceController(VertSurface surface,
		VertSurfaceVisualizer visualizer,
		float minHeight,
		float maxHeight){

		this.surface = surface;
		this.visualizer = visualizer;
		this.minHeight = minHeight;
		this.maxHeight = maxHeight;

		collider = null;
	}

	public void Extrude(float y){
		surface.Extrude(y);
		var height = Mathf.Clamp(surface.height, minHeight, maxHeight);
		surface.height = height;
		visualizer.Update();
	}

	public void CreateCollider(){
		CreateCollider(surface);
	}

	public void Show() => visualizer.Hide();
	public void Hide() => visualizer.Hide();

	public static Collider CreateCollider(VertSurface surface){

		var colliderGameObject = new GameObject("Surface");
		colliderGameObject.transform.position = Vector3.zero;
		
		var collider = colliderGameObject.AddComponent<MeshCollider>();
		var mesh = new Mesh();

		var basePoints = surface.GetBaseEdge().ToList();
		var topPoints = surface.GetTopEdge().ToList();
		int vertexCount = basePoints.Count + topPoints.Count;

		var vertices = new Vector3[vertexCount];
		var triangles = new List<int>();

		for (int i = 0; i < basePoints.Count; i++) {
			vertices[i] = basePoints[i];
			vertices[i + basePoints.Count] = topPoints[i];

			if (i < basePoints.Count - 1) {
				int base1 = i;
				int base2 = i + 1;
				int top1 = i + basePoints.Count;
				int top2 = i + 1 + basePoints.Count;

				triangles.AddRange(new[] { base1, top1, base2 });
				triangles.AddRange(new[] { top1, top2, base2 });
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles.ToArray();
		mesh.RecalculateNormals();

		collider.sharedMesh = mesh;

		//adding surfaces under common parent
		var parent = GameObject.Find("GeneratedSurfaces");
		if(parent == null) parent = new GameObject("GeneratedSurface");
		colliderGameObject.transform.SetParent(parent.transform);

		return collider;
	}
}
}
