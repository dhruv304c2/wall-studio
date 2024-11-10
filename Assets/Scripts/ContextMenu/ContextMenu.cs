using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace Context{
    [RequireComponent(typeof(Canvas))]
    public class ContextMenu : MonoBehaviour, IPoolable<IMemoryPool>, IDisposable{
        [Inject] ContextKeyMap.Pool _keyMapPool;
        [Inject] ContextText.Pool _contextTextPool;

        IMemoryPool _pool;

        public Vector3 position;

        [SerializeField] LineRenderer labelPointer;
        [SerializeField] Transform lableConnectionPoint;
        [SerializeField] Transform contentRoot;
        [SerializeField] float trackSpeed = 1f;
        [SerializeField] float maxDistance = 5f;
        [SerializeField] Vector3 offset = new Vector3(0.5f,0.1f,0.1f);
        [SerializeField] float opacity = 0.85f;

        List<IDisposable> _disposables = new();
        CanvasGroup _canvas;
        bool _hidden;

        void Awake(){
            Observable
                .EveryUpdate()
                .TakeUntilDestroy(this)
                .Where(_ => !_hidden)
                .Subscribe(_ => UpdatePosition());

            _canvas = GetComponentInChildren<CanvasGroup>();
        }

        void UpdatePosition(){
            labelPointer.positionCount = 2;
            labelPointer.SetPosition(0, position);
            labelPointer.SetPosition(1, lableConnectionPoint.position);

            var cam = Camera.main.transform;
            var relOffset = (cam.forward.normalized * offset.z) + (cam.right.normalized * offset.x) + (cam.up.normalized * offset.y);

            var targetRaw = relOffset + position;

            if(Vector3.Distance(transform.position,targetRaw) > 1f){
                transform.position = targetRaw;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetRaw, trackSpeed * Time.deltaTime);
        }

        void SetPosition(){
            var cam = Camera.main.transform;
            var offset = (cam.forward.normalized * 0.1f) + (cam.right.normalized * 0.3f) + (cam.up.normalized * 0.25f);
            var targetRaw = offset + position;
            transform.position = targetRaw;
        }

        public ContextText AddText(string text){
            var t =_contextTextPool.Spawn();
            t.SetText(text);
            t.transform.SetParent(contentRoot);
            t.transform.localScale = Vector3.one;
            t.transform.localRotation = Quaternion.identity;
            t.transform.localPosition = Vector3.zero;
            _disposables.Add(t);
            return t;
        }

        public ContextKeyMap AddKeyMap(OVRInput.Button button, string actionDescription){
            var m = _keyMapPool.Spawn();
            m.SetKeyMap(button, actionDescription);
            m.transform.SetParent(contentRoot);
            m.transform.localScale = Vector3.one;
            m.transform.localRotation = Quaternion.identity;
            m.transform.localPosition = Vector3.zero;
            _disposables.Add(m);
            return m;
        }

        public void Hide(){
            _canvas.alpha = 0;
            labelPointer.enabled = false;
            _hidden = true;
        }

        public void Show(){
            _canvas.alpha = opacity;
            labelPointer.enabled = true;
            SetPosition();
            _hidden = false;
        } 

        public void OnDespawned(){
            Hide();
            labelPointer.positionCount = 0;
            _disposables.ForEach((d) => d.Dispose());
            _disposables = new();
        }

        public void OnSpawned(IMemoryPool p1){
            Show();
            _pool = p1;
        }

        public void Dispose(){
            _pool.Despawn(this);
        }


        public class Pool : MemoryPool<ContextMenu>{
            public ContextMenu Spawn(Vector3 position){
                var spawned = Spawn();
                spawned.position = position;
                spawned.SetPosition();
                return spawned;
            }

            protected override void OnCreated(ContextMenu item){
                item.OnDespawned();
            }

            protected override void OnSpawned(ContextMenu item){
                item.OnSpawned(this);
            }

            protected override void OnDespawned(ContextMenu item){
                item.OnDespawned();
            }
        }
    }
}
