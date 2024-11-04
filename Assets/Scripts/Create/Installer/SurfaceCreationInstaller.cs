using UnityEngine;
using Zenject;
using Create.Preview;

namespace Create{
public class SurfaceCreationInstaller : MonoInstaller<SurfaceCreationInstaller> {
    [Header("Surface anchor preview")] 
    [SerializeField] int intiialSurfacePreviewPoolSize;
    [SerializeField] SurfaceAnchorPreview anchorPreviewPrefab;

    [Header("Surface line preview")]
    [SerializeField] int intitialSurfaceLinePoolSize;
    [SerializeField] PoolableSurfaceLineRenderer surfaceLinePrefab;

    public override void InstallBindings(){
	Container.BindMemoryPool<SurfaceAnchorPreview, SurfaceAnchorPreview.Pool>()
	    .WithInitialSize(intiialSurfacePreviewPoolSize)
	    .FromComponentInNewPrefab(anchorPreviewPrefab)
	    .UnderTransformGroup("SurfaceAnchorPreviewPool")
	    .Lazy();

	Container.BindMemoryPool<PoolableSurfaceLineRenderer, PoolableSurfaceLineRenderer.Pool>()
	    .WithInitialSize(intitialSurfaceLinePoolSize)
	    .FromComponentInNewPrefab(surfaceLinePrefab)
	    .UnderTransformGroup("SurfaceLinePreviewPool")
	    .Lazy();
    }
}
}

