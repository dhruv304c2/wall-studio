using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XR.SpatialAnchors;
using XR.Input;
using CreateMode.Preview;
using Zenject;
using UniRx;
using System.Threading;
using ContextMenu = Context.ContextMenu;
using System;

namespace CreateMode.Surface {
public class SurfaceBuilder : MonoBehaviour, ISurfaceBuilder {	
	[Inject] IOVRRaycastService _raycastService;
	[Inject] PoolableOVRSpatialAnchor.Pool _spatialAnchorPool;
	[Inject] SurfaceAnchorVisualizer.Pool _anchorPreviewPool;
	[Inject] PoolableSurfaceLineRenderer.Pool _surfaceLinePreviewPool;
	[Inject] ContextMenu.Pool _contextMenuPool;

	[Header("Vertical Surface Refs")]
	[SerializeField] LineRenderer3D aimLine;
	[SerializeField] LayerMask floorLayer;
	[SerializeField] Gradient edgeLineColor;
	[SerializeField] float extrusionPerStep;

	public List<VertSurfaceRenderer> _activeSurfaces;

	void Start(){

		HidePreview();
		_activeSurfaces = new();
	}	

	ContextMenu CreateHoldPrompt(Func<long,bool> trackWhile){
		var holdPrompt = _contextMenuPool.Spawn(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));

		holdPrompt.AddText("Create Base Outline For Vertical Surface");
		holdPrompt.AddKeyMap(OVRInput.Button.PrimaryIndexTrigger, "Hold to start placing");
		holdPrompt.AddKeyMap(OVRInput.Button.One, "Confirm");	
		
		Observable
			.EveryUpdate()
			.TakeWhile(trackWhile)
			.Subscribe(_ => {
				if(!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger,OVRInput.Controller.RTouch)){	
					holdPrompt.Show();
				}
				else{
					holdPrompt.Hide();
				}
				holdPrompt.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
			});

		return holdPrompt;
	}

	ContextMenu CreatePostionPrompt(Transform track, Func<long,bool> trackWhile){
		var releasePrompt = _contextMenuPool.Spawn(track.position);
		var posText = releasePrompt.AddText("");
		
		Observable
			.EveryUpdate()
			.TakeWhile(trackWhile)
			.Subscribe(_ => {
				if(track.gameObject.activeSelf){	
					releasePrompt.Show();
				}
				else{
					releasePrompt.Hide();
				}
				releasePrompt.position = track.position; 
				posText.SetText($"(x:{track.position.x.ToString("F2")},y:{track.position.y.ToString("F2")},z:{track.position.z.ToString("F2")}");
			});

		return releasePrompt;
	}
	
	ContextMenu CreateReleasePrompt(Func<long,bool> trackWhile){
		var releasePrompt = _contextMenuPool.Spawn(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));

		releasePrompt.AddText("Create Base Outline For Vertical Surface");
		releasePrompt.AddKeyMap(OVRInput.Button.PrimaryIndexTrigger, "Release To Place");
		
		Observable
			.EveryUpdate()
			.TakeWhile(trackWhile)
			.Subscribe(_ => {
				if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch)){
					releasePrompt.Show();
				}else{
					releasePrompt.Hide();
				}
				releasePrompt.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
			});

		return releasePrompt;
	}

	ContextMenu CreateExtrudePrompt(Func<long,bool> trackWhile){
		var extrudePrompt = _contextMenuPool.Spawn(OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch));

		extrudePrompt.AddText("Adjust Surface Height");
		extrudePrompt.AddKeyMap(OVRInput.Button.PrimaryThumbstick, "Adjust Height");	
		extrudePrompt.AddKeyMap(OVRInput.Button.One, "Confirm");	

		Observable
			.EveryUpdate()
			.TakeWhile(trackWhile)
			.Subscribe(_ => {
				extrudePrompt.position = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
			});

		return extrudePrompt;
	}

	public async UniTask<VertSurfaceRenderer> RequestVerticalSurfaceFromUserAsync(int pointLimit = 2){
		var baseEdgeAnchors = await RequestBaseEdge(pointLimit);
		if(baseEdgeAnchors.Count == 0) return null;
		if(baseEdgeAnchors.Count == 1){
			baseEdgeAnchors[0].Despawn();
			return null;
		}

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

		var posPrompt = CreatePostionPrompt(aimPreview.transform, _ => !placementCTS.IsCancellationRequested);
//		var releasePrompt = CreateReleasePrompt(_ => !placementCTS.IsCancellationRequested);
		var holdPrompt = CreateHoldPrompt(_ => !placementCTS.IsCancellationRequested);


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
		posPrompt.Dispose();
//		releasePrompt.Dispose();
		holdPrompt.Dispose();
		
		return baseEdgeAnchors;
	}

	async UniTask<VertSurfaceRenderer> RequestVertSurfaceFromBaseEdge(List<PoolableOVRSpatialAnchor> baseEdgeAnchors){
		var surface = new VertSurface(baseEdgeAnchors, extrusionPerStep);
		var visualizer = new VertSurfaceVisualizer(surface, _anchorPreviewPool);
		visualizer.Update();
		
		var minHeight = 0.1f;
		var maxHeight = 20f;

		var vertSurface = new VertSurfaceRenderer(surface, visualizer, minHeight, maxHeight);
		await RequestAdjustedHeight(vertSurface);
		return vertSurface;
	}

	async UniTask RequestAdjustedHeight(VertSurfaceRenderer vertSurface){
		vertSurface.SetColliderActive(false);
		bool heightConfirmed = false;

		Observable
			.EveryUpdate()
			.TakeWhile(_ => !heightConfirmed)
			.Where(_ => OVRInput.GetDown(OVRInput.Button.One))
			.Subscribe(_ => heightConfirmed = true);

		var prompt = CreateExtrudePrompt(_ => !heightConfirmed);
		
		Observable
			.EveryUpdate()
			.Where(_ => {
				var axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				return axis.y > 0.1f || axis.y < -0.1f;
			})
			.TakeWhile(_ => !heightConfirmed)
			.Subscribe(_ => {
				var axis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);
				vertSurface.Extrude(extrusionPerStep * axis.y);
			});
	

		await UniTask.WaitUntil(() => heightConfirmed);
		vertSurface.UpdateCollider();
		vertSurface.SetColliderActive(true);

		prompt.Dispose();
	}

	void HidePreview(){
		aimLine.Hide();
	}
}
}

