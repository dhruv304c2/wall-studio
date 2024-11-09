using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XR.SpatialAnchors;
using XR.Input;
using Create.Preview;
using Zenject;
using UniRx;
using System.Threading;

namespace Create.Surface {
public class SurfaceBuilder : MonoBehaviour {	
	[Inject] IOVRRaycastService _raycastService;
	[Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
	[Inject] SurfaceAnchorVisualizer.Pool _anchorPreviewPool;
	[Inject] PoolableSurfaceLineRenderer.Pool _surfaceLinePreviewPool;

	[Header("Vertical Surface Refs")]
	[SerializeField] LineRenderer3D aimLine;
	[SerializeField] LayerMask floorLayer;
	[SerializeField] Gradient edgeLineColor;
	[SerializeField] float extrusionPerStep;

	public List<VertSurfaceController> _activeSurfaces;

	void Start(){
		HidePreview();
		RequestVerticalSurfaces(10); //TODO: Remove Debug method call
		_activeSurfaces = new();
	}

	public async void RequestVerticalSurfaces(int pointLimit = 2){
		while(true){
			await RequestVerticalSurfaceFromUserAsync(pointLimit);
		}
	}

	public async UniTask<VertSurfaceController> RequestVerticalSurfaceFromUserAsync(int pointLimit = 2){
		var baseEdgeAnchors = await RequestBaseEdge(pointLimit);
		var vertSurface = await RequestVertSurfaceFromBaseEdge(baseEdgeAnchors);
		_activeSurfaces.Add(vertSurface);
		return vertSurface;
	}		

	async UniTask<List<PoolableOVRSpatialAnchor>> RequestBaseEdge(int pointLimit = 2){
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
		aimPreview.Despawn();
		outlineRenderer.Despawn();
		
		return baseEdgeAnchors;
	}

	async UniTask<VertSurfaceController> RequestVertSurfaceFromBaseEdge(List<PoolableOVRSpatialAnchor> baseEdgeAnchors){
		var surface = new VertSurface(baseEdgeAnchors, extrusionPerStep);
		var visualizer = new VertSurfaceVisualizer(surface, _anchorPreviewPool);
		
		var minHeight = 0.1f;
		var maxHeight = 20f;

		var surfaceController = new VertSurfaceController(surface, visualizer, minHeight, maxHeight);
	
		visualizer.Update();

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
				surfaceController.Extrude(extrusionPerStep * axis.y);
			});
	

		await UniTask.WaitUntil(() => heightConfirmed);
		surfaceController.CreateCollider();
		return surfaceController;
	}

	void HidePreview(){
		aimLine.Hide();
	}
}
}

