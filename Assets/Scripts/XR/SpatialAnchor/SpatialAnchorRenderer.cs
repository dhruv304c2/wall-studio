using UnityEngine;

namespace XR.SpatialAnchors{
public class SpatialAnchorRenderer : MonoBehaviour {
    private Transform defaultModel;
    
    void Start(){
        defaultModel = transform.Find("defaultModel");
    }

    void Show(){
        VerifyDefaultModel();
        defaultModel.gameObject.SetActive(true);
    }

    void Hide(){
        VerifyDefaultModel();
        defaultModel.gameObject.SetActive(false);
    }

    void VerifyDefaultModel(){
        if(defaultModel == null) throw new System.Exception("No child with name defaultModelFound");
    }
}
}
