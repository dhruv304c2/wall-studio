using UnityEngine;
using Zenject.Asteroids;
using Cysharp.Threading.Tasks;

namespace XR.FloorPlane {
    public class ImplementedFloorPlane : IFloorPlane {
        int _width;
        int _height;
        Material _material;
        LayerMask _layer;

        public ImplementedFloorPlane(int width, int height, LayerMask floorLayer, Material floorMaterial = null){
            _width = width;
            _height = height;
            _material = floorMaterial;
            _layer = floorLayer;
        }

        GameObject _floorPlane;

        public async void Initialize(){
            await UniTask.WaitUntil(() => OVRManager.instance != null && OVRManager.boundary != null); //delaying initialization until OVRManager is initialized

            if(OVRManager.boundary.GetConfigured()){
                var geometry = OVRManager.boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
                if(geometry.Length < 1) throw new System.Exception("Play area geometry has no points");
                _floorPlane = ConstructFloorPlane(_width, _height, _layer);
                _floorPlane.transform.SetY(geometry[0].y);

                HideVisualization();
            } else{
                throw new System.Exception("Boundry not configured, cannot intialize Floor Plane");
            }
        }
        public void HideVisualization(){
            if(_floorPlane) _floorPlane.GetComponent<MeshRenderer>().enabled = false;
        }

        public void Visualize(){
            if(_floorPlane) _floorPlane.GetComponent<MeshRenderer>().enabled = true;
        }

        public GameObject ConstructFloorPlane(float width, float height, LayerMask floorPlaneCollisionLayer) {
            GameObject floorPlane = new GameObject("FloorPlane");
            MeshFilter meshFilter = floorPlane.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = floorPlane.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[] {
                new Vector3(-width / 2, 0, -height / 2),
                new Vector3(width / 2, 0, -height / 2),
                new Vector3(-width / 2, 0, height / 2),
                new Vector3(width / 2, 0, height / 2)
            };

            int[] triangles = new int[] {
                0, 2, 1,
                2, 3, 1
            };

            Vector2[] uvs = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.RecalculateNormals();

            meshFilter.mesh = mesh;
            if(_material != null) meshRenderer.material = _material;
            floorPlane.layer = LayerMaskToLayer(floorPlaneCollisionLayer);

            var meshCollider = floorPlane.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;

            Rigidbody rb = floorPlane.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            return floorPlane;
        }

        public int LayerMaskToLayer(LayerMask layer){
            int layerIdx = (int)Mathf.Log(layer.value, 2);
            return layerIdx;
        }
    }
}
