using UnityEngine;

public class LineRenderer3D : MonoBehaviour {
    [SerializeField] GameObject lineObject;

    public float lineWidth = 0.1f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    private Vector3 _startPoint;
    private Vector3 _endPoint;

    void Awake() {
        _meshFilter = lineObject.GetComponent<MeshFilter>();
        _meshRenderer = lineObject.GetComponent<MeshRenderer>();
    }

    public void UpdateLine(Vector3 start, Vector3 end) {
        _startPoint = start;
        _endPoint = end;

        Vector3 direction = _endPoint - _startPoint;
        float distance = direction.magnitude;
        Vector3 midpoint = (_startPoint + _endPoint) / 2;

        transform.position = midpoint;
        transform.LookAt(_endPoint);
        transform.localScale = new Vector3(lineWidth, lineWidth, distance);
    }

    public void Show(){
        _meshRenderer.enabled = true;
    }

    public void Hide(){
        _meshRenderer.enabled = false;
    }
}
