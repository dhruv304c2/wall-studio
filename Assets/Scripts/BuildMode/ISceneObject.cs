using UnityEngine;

public interface ISceneObject {
	public Collider Bounds {get;}
	public Vector2 Up {get;}
	public void Fade();
	public void UnFade();
	public void Hide();
	public void Show();
}
