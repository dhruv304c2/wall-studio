using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XR.SpatialAnchors;
using XR.Input;
using Create.Preview;
using Zenject;
using UniRx;
using ModestTree;

namespace Create{
public class SurfaceBuilder : MonoBehaviour {	
	[Inject] IOVRRaycastService _raycastService;
	[Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
	[Inject] SurfaceAnchorPreview.Pool _anchorPreviewPool;
	[Inject] PoolabeSurfaceLineRenderer.Pool _surfaceLinePreviewPool;

	[Header("Vertical Surface Refs")]
	[SerializeField] LineRenderer3D aimLine;
	[SerializeField] LayerMask floorLayer;
	[SerializeField] Gradient edgeLineColor;
	[SerializeField] float extrusionPerStep;


	void Start(){
		HidePreview();
		RequestVerticalSurface(); //TODO: Remove Debug method call
	}

	public void RequestVerticalSurface(){
		RequestVerticalSurfaceFromUserAsync().Forget();
	}

	public async UniTask RequestVerticalSurfaceFromUserAsync(int pointLimit = 2){
		var aimPreview = _anchorPreviewPool.Spawn();

		var outlineRenderer = _surfaceLinePreviewPool.Spawn();
		outlineRenderer.lineRenderer.colorGradient = edgeLineColor;

		var anchorPlacementController = new SpatialAnchorPlacementController(
					_raycastService,
					_spatialAnchorPool,
					floorLayer,
					aimPreview.gameObject,
					aimLine	
				);

		List<PoolableOVRSpatialAnchor> baseEdgeAnchors = new();
		outlineRenderer.lineRenderer.positionCount = 0;
		bool outlineConfirmed = false;

		Observable
			.EveryUpdate()
			.Where(_ => OVRInput.GetDown(OVRInput.Button.Two))
			.Subscribe(_ => outlineConfirmed = true);
		

		for(int i = 0; i < pointLimit && !outlineConfirmed; i++){
			if(baseEdgeAnchors.Count != 0) aimPreview.SetConnectedEdgeTransform(baseEdgeAnchors[0].transform);
			var anchor  = await anchorPlacementController.RequestSpatialAnchorUserAsync();	
			baseEdgeAnchors.Add(anchor);
			outlineRenderer.lineRenderer.positionCount++;
			outlineRenderer.lineRenderer.SetPosition(outlineRenderer.lineRenderer.positionCount - 1, anchor.transform.position);
		}

		var surface = new VertSurface(baseEdgeAnchors, extrusionPerStep);
		var preview = new VertSurfacePreview(surface, _anchorPreviewPool);
		
		preview.UpdatePreview();

		Observable
			.EveryUpdate()
			.Where(_ => OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickUp))
			.Subscribe(_ => {
				surface.Extrude(extrusionPerStep);
				preview.UpdatePreview();
			});

		Observable
			.EveryUpdate()
			.Where(_ => OVRInput.GetDown(OVRInput.Button.PrimaryThumbstickDown))
			.Subscribe(_ => {
				surface.Extrude(-extrusionPerStep);
				preview.UpdatePreview();
			});


		aimPreview.Despawn();
		outlineRenderer.Despawn();
	}

	void UpdateTopEdgePreview(VertSurface surface){
			
	}

	void HidePreview(){
		aimLine.Hide();
	}
}

public class VertSurfacePreview{
	VertSurface _surface;
	SurfaceAnchorPreview.Pool _surfaceAnchorPool;
	
	public VertSurfacePreview(
		VertSurface surface,
		SurfaceAnchorPreview.Pool surfaceAnchorPool
		){

		_surface = surface;
		_surfaceAnchorPool = surfaceAnchorPool;
	}

	List<SurfaceAnchorPreview> _baseEdgePreview = new();
	List<SurfaceAnchorPreview> _topEdgePrewView = new();

	public void UpdatePreview(){
		//Dispose old preview
		Dispose();

		//Create base edge preview
		foreach(var p in _surface.GetBaseEdge()){
			var a = _surfaceAnchorPool.Spawn();
			a.transform.position = p;
			if(!_baseEdgePreview.IsEmpty()) a.SetConnectedEdgeTransform(_baseEdgePreview.Last().transform);
			_baseEdgePreview.Add(a);
		}

		//Create top edge preview
		int idx = 0;
		foreach(var p in _surface.GetTopEdge()){
			var a = _surfaceAnchorPool.Spawn();
			a.transform.position = p;
			if(!_topEdgePrewView.IsEmpty()) a.SetConnectedEdgeTransform(_topEdgePrewView.Last().transform);
			a.SetConnectedBasePointTransform(_baseEdgePreview[idx].transform);
			_topEdgePrewView.Add(a);
			idx++;
		}
	}

	public void Dispose(){
		_baseEdgePreview.ForEach((p) => p.Despawn());
		_topEdgePrewView.ForEach((p) => p.Despawn());
	}
}

public class VertSurface{
	public List<PoolableOVRSpatialAnchor> baseEdge = new();
	public float height; 

	public VertSurface(List<PoolableOVRSpatialAnchor> baseEdge, float height){
		this.baseEdge = baseEdge;
		this.height = height;
	}

	public void SetHeight(float height){
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

