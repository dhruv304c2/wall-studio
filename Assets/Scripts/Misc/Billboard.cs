using UniRx;
using UnityEngine;

public class Billboard : MonoBehaviour {
    public Camera targetCamera;

    void Start() {
        if (targetCamera == null) {
            targetCamera = Camera.main;
        }

        Observable.EveryLateUpdate()
            .TakeUntilDestroy(this)
            .Subscribe(_ => FaceCamera());
    }

    void FaceCamera() {
        if (targetCamera == null) return;

        var target= targetCamera.transform.position;
        transform.LookAt(2 * transform.position - target);
    }
}

