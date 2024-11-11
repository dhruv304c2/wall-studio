using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class SceneObject : MonoBehaviour, ISceneObject{
	[SerializeField] Collider bounds;
	[SerializeField] Material fadeMat;
	[SerializeField] Material reagularMat;
	[SerializeField] Vector2 UpDirection;
	public string id;

	MeshRenderer _renderer;

	void Start(){
		if(bounds) bounds.isTrigger = true;
		_renderer = GetComponent<MeshRenderer>();
	}

	public Collider Bounds => bounds;

	public Vector2 Up => UpDirection.normalized;

	public void Fade(){
		_renderer.material = fadeMat;
	}

	public void UnFade(){
		_renderer.material = reagularMat;
	}

	public void Hide(){
		gameObject.SetActive(false);
	}

	public void Show(){
		gameObject.SetActive(true);
	}
}
