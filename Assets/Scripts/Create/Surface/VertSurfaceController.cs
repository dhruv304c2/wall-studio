using UnityEngine;

namespace Create.Surface{
public class VertSurfaceController{
	public VertSurface _surface;
	public VertSurfaceVisualizer _visualizer;
	public float _minHeight;
	public float _maxHeight;

	public VertSurfaceController(VertSurface surface,
		VertSurfaceVisualizer preview,
		float minHeight,
		float maxHeight){

		_surface = surface;
		_visualizer = preview;
		_minHeight = minHeight;
		_maxHeight = maxHeight;
	}

	public void Extrude(float y){
		_surface.Extrude(y);
		var height = Mathf.Clamp(_surface.height, _minHeight, _maxHeight);
		_surface.height = height;
		_visualizer.Update();
	}

	public void Show() => _visualizer.Hide();
	public void Hide() => _visualizer.Hide();
}
}
