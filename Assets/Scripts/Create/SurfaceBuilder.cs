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
using System.Threading;

namespace Create{
public class SurfaceBuilder : MonoBehaviour {	
	[Inject] IOVRRaycastService _raycastService;
	[Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
	[Inject] SurfaceAnchorPreview.Pool _anchorPreviewPool;
	[Inject] PoolableSurfaceLineRenderer.Pool _surfaceLinePreviewPool;

	[Header("Vertical Surface Refs")]
	[SerializeField] LineRenderer3D aimLine;
	[SerializeField] LayerMask floorLayer;
	[SerializeField] Gradient edgeLineColor;
	[SerializeField] float extrusionPerStep;


	void Start(){
		HidePreview();
		RequestVerticalSurfaces(10); //TODO: Remove Debug method call
	}

	public async void RequestVerticalSurfaces(int pointLimit = 2){
		while(true){
			await RequestVerticalSurfaceFromUserAsync(pointLimit);
		}
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
		
		var placementCTS = new CancellationTokenSource();
		Observable
			.EveryUpdate()
			.TakeWhile(_ => !placementCTS.IsCancellationRequested)
			.Where(_ => OVRInput.GetDown(OVRInput.Button.One))
			.Subscribe(_ => {
				placementCTS.Cancel(); //anchor placement sequence must be canceled if user confirms base outline
				placementCTS.Dispose();
			});
		

		for(int i = 0; i < pointLimit && !placementCTS.IsCancellationRequested;){
			if(baseEdgeAnchors.Count != 0) aimPreview.SetConnectedEdgeTransform(baseEdgeAnchors.Last().transform);
			var anchor  = await anchorPlacementController.RequestSpatialAnchorUserAsync(placementCTS.Token);	

			if(anchor == null) continue;

			//Increment only if a non null anchor is returned
			i++;
			baseEdgeAnchors.Add(anchor);
			outlineRenderer.lineRenderer.positionCount++;
			outlineRenderer.lineRenderer.SetPosition(outlineRenderer.lineRenderer.positionCount - 1, anchor.transform.position);
		}

		await UniTask.WaitUntil(() => placementCTS.IsCancellationRequested);

		var surface = new VertSurface(baseEdgeAnchors, extrusionPerStep);
		var preview = new VertSurfacePreview(surface, _anchorPreviewPool);
	
		preview.UpdatePreview();

		bool heightConfirmed = false;

		Observable
			.EveryUpdate()
			.TakeWhile(_ => !heightConfirmed)
			.Where(_ => OVRInput.GetDown(OVRInput.Button.One))
			.Subscribe(_ => heightConfirmed = true);

		Observable
			.EveryUpdate()
			.Where(_ => {
				var axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				return axis.y > 0.1f || axis.y < -0.1f;
			})
			.TakeWhile(_ => !heightConfirmed)
			.Subscribe(_ => {
				var axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				surface.Extrude(extrusionPerStep * axis.y);
				preview.UpdatePreview();
			});
	

		await UniTask.WaitUntil(() => heightConfirmed);

		aimPreview.Despawn();
		outlineRenderer.Despawn();
	}

	void HidePreview(){
		aimLine.Hide();
	}
}

public class VertSurfacePreview{
	VertSurface _surface;
	SurfaceAnchorPreview.Pool _surfaceAnchorPool;
	
	public VertSurface Surface => _surface;

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

		_baseEdgePreview = new();
		_topEdgePrewView = new();
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

