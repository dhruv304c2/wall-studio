using UnityEngine;
using TMPro;
using Zenject;
using System;

namespace Context{
public class ContextText : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable{
    [SerializeField] TextMeshProUGUI text;

    IMemoryPool _pool;

    public void OnDespawned(){
        transform.SetParent(null);
        gameObject.SetActive(false);
    }

    public void OnSpawned(IMemoryPool p1){
        _pool = p1;
        gameObject.SetActive(true);
    }

    public void SetText(string message){
        text.text = message;
    }

    public void Dispose(){
        _pool.Despawn(this);
    }

    public class Pool : MemoryPool<ContextText> {
        protected override void OnDespawned(ContextText item){
            item.OnDespawned();
        }

        protected override void OnSpawned(ContextText item){
            item.OnSpawned(this);
        }

        protected override void OnCreated(ContextText item){
            item.OnDespawned();
        }
    }
}}
