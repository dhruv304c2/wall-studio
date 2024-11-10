using UnityEngine;
using Zenject;

namespace Context{
public class ContextMenuInstaller : MonoInstaller<ContextMenuInstaller> {
    [SerializeField] ContextMenu menuPrefab;
    [SerializeField] ContextText textPrfab;
    [SerializeField] ContextKeyMap keyMapPrefab;

    public override void InstallBindings(){
        Container.BindMemoryPool<ContextText, ContextText.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(textPrfab)
            .UnderTransformGroup("ContextMenuTextPool")
            .AsCached();

        Container.BindMemoryPool<ContextKeyMap, ContextKeyMap.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(keyMapPrefab)
            .UnderTransformGroup("KeyMapTextPool")
            .AsCached();

        Container.BindMemoryPool<ContextMenu, ContextMenu.Pool>()
            .WithInitialSize(10)
            .FromComponentInNewPrefab(menuPrefab)
            .UnderTransformGroup("ContextMenuPool")
            .AsCached();
    }
}}
