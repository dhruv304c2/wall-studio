using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using Zenject;

namespace Context{
public class ContextKeyMap : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable {
    [SerializeField] KeySpriteMap spriteMap;
    [SerializeField] TextMeshProUGUI text;
    [SerializeField] Image buttonImage;

    IMemoryPool _pool;

    public void OnDespawned(){
        transform.SetParent(null);
        gameObject.SetActive(false);
    }

    public void OnSpawned(IMemoryPool p1){
        _pool = p1;
        gameObject.SetActive(true);
    } 

    public void SetKeyMap(OVRInput.Button button, string actionDescription){
        buttonImage.sprite = spriteMap.GetSprite(button);
        text.text = $": {actionDescription}";
    }

    public void Dispose(){
        _pool.Despawn(this);
    }

    public class Pool : MemoryPool<ContextKeyMap>{
        protected override void OnDespawned(ContextKeyMap item){
            item.OnDespawned();
        }

        protected override void OnCreated(ContextKeyMap item){
            item.OnDespawned();
        }

        protected override void OnSpawned(ContextKeyMap item){
            item.OnSpawned(this);
        }
    }

}

[Serializable]
public class KeySpriteMap{
    public List<KeySpriteMapEntry> map;

    public Sprite GetSprite(OVRInput.Button button){
        Func<KeySpriteMapEntry,bool> match = (KeySpriteMapEntry i) => {
            return i.button == button;
        };
        if(!map.Any(match)) return null;
        return map.First(match).sprite;
    }
}


[Serializable]
public class KeySpriteMapEntry{
    public OVRInput.Button button;
    public Sprite sprite;
}
}
