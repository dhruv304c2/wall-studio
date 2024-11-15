using UniRx;
using UnityEngine;

public class Billboard : MonoBehaviour {
    public Camera targetCamera;
    [SerializeField] bool maintainAngleOfVision;
    [Header("This is not an actaul angle but an arbitrary value corresponding to it")]
    [SerializeField] float angleOfVision;

    Vector3 originalScale;

    void Start() {
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }

        originalScale = transform.localScale;

        Observable.EveryLateUpdate()
            .TakeUntilDestroy(this)
            .Subscribe(_ => FaceCamera());
    }

    void FaceCamera() {
        if (targetCamera == null) return;

        var target= targetCamera.transform.position;
        transform.LookAt(2 * transform.position - target);

        if(maintainAngleOfVision) {
            var distance = Vector3.Distance(transform.position, target);
            var scale = - Mathf.Tan(angleOfVision) * distance;
            transform.localScale = originalScale * scale;
        }
    }
}

